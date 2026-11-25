using System.Text.Json;

namespace Bodeglook.Models;

public static class Geoapify
{
    public static string ApiKey = "";

    private const string Servidor = "https://api.geoapify.com";
    private const int CreditosPorDia = 2500;
    private const int PausaEntrePedidos = 250;
    private const int LugaresPorPagina = 40;
    private const int MaximoPorBarrio = 100;
    private const int GuardadosPorBarrio = 20;

    private const string FiltroCABA = "rect:-58.5315,-34.7056,-58.3350,-34.5265";

    private const string Categorias = "catering.restaurant,catering.bar,catering.pub,catering.cafe,catering.ice_cream," +
        "catering.fast_food.pizza,catering.fast_food.burger,catering.fast_food.tacos," +
        "commercial.food_and_drink.drinks,production.winery";

    private static readonly HttpClient Cliente = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    private static readonly SemaphoreSlim Turno = new SemaphoreSlim(1, 1);
    private static DateTime ultimoPedido = DateTime.MinValue;
    private static DateTime diaDeLosCreditos = DateTime.Today;
    private static int creditosUsados = 0;
    private static readonly HashSet<string> BusquedasDeHoy = new HashSet<string>();

    private static async Task<JsonDocument?> Pedir(string ruta, int creditos)
    {
        if (ApiKey == "") return null;

        await Turno.WaitAsync();
        try
        {
            if (diaDeLosCreditos != DateTime.Today)
            {
                diaDeLosCreditos = DateTime.Today;
                creditosUsados = 0;
                lock (BusquedasDeHoy) { BusquedasDeHoy.Clear(); }
            }
            if (creditosUsados + creditos > CreditosPorDia)
            {
                Console.WriteLine("Geoapify: se alcanzó el tope diario propio, hoy no se llama más.");
                return null;
            }

            int espera = PausaEntrePedidos - (int)(DateTime.Now - ultimoPedido).TotalMilliseconds;
            if (espera > 0) await Task.Delay(espera);
            ultimoPedido = DateTime.Now;
            creditosUsados += creditos;

            HttpRequestMessage pedido = new HttpRequestMessage(HttpMethod.Get, Servidor + ruta);
            pedido.Headers.Add("x-api-key", ApiKey);
            using (HttpResponseMessage respuesta = await Cliente.SendAsync(pedido))
            {
                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine("Geoapify respondió con el código " + (int)respuesta.StatusCode + ": " +
                        await respuesta.Content.ReadAsStringAsync());
                    return null;
                }
                return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("Geoapify no respondió: " + error.Message);
            return null;
        }
        finally
        {
            Turno.Release();
        }
    }

    private static string Codificar(string texto)
    {
        return Uri.EscapeDataString(texto);
    }

    private static async Task<string?> BuscarLimiteDeBarrio(string barrio)
    {
        string ruta = "/v1/geocode/search?text=" + Codificar(barrio + ", Ciudad Autónoma de Buenos Aires, Argentina") +
            "&filter=countrycode:ar&lang=es&limit=5&format=json";
        using (JsonDocument? respuesta = await Pedir(ruta, 1))
        {
            if (respuesta == null || !respuesta.RootElement.TryGetProperty("results", out JsonElement resultados)) return null;

            foreach (JsonElement resultado in resultados.EnumerateArray())
            {
                string? tipo = TextoDe(resultado, "result_type");
                if ((tipo == "suburb" || tipo == "district") && TextoDe(resultado, "place_id") != null)
                    return TextoDe(resultado, "place_id");
            }
            return null;
        }
    }

    private static async Task<List<LugarGeoapify>?> PedirLugares(string filtro, int desde, int cantidad, string? nombre)
    {
        string ruta = "/v2/places?categories=" + Codificar(Categorias) + "&filter=" + Codificar(filtro) +
            "&limit=" + cantidad + "&offset=" + desde + "&lang=es";
        if (nombre != null) ruta += "&name=" + Codificar(nombre);

        using (JsonDocument? respuesta = await Pedir(ruta, (int)Math.Ceiling(cantidad / 20.0)))
        {
            if (respuesta == null || !respuesta.RootElement.TryGetProperty("features", out JsonElement lugares)) return null;

            List<LugarGeoapify> lista = new List<LugarGeoapify>();
            foreach (JsonElement lugar in lugares.EnumerateArray())
            {
                JsonElement datos = lugar.GetProperty("properties");
                LugarGeoapify nuevo = new LugarGeoapify();
                nuevo.IdGeoapify = TextoDe(datos, "place_id");
                nuevo.Nombre = TextoDe(datos, "name");
                nuevo.Calle = TextoDe(datos, "street");
                nuevo.Numero = TextoDe(datos, "housenumber");
                nuevo.CodigoPostal = TextoDe(datos, "postcode");
                if (!datos.TryGetProperty("lat", out JsonElement latitud) || !datos.TryGetProperty("lon", out JsonElement longitud)) continue;
                nuevo.Latitud = Math.Round(latitud.GetDecimal(), 6);
                nuevo.Longitud = Math.Round(longitud.GetDecimal(), 6);
                if (datos.TryGetProperty("categories", out JsonElement categorias))
                {
                    foreach (JsonElement categoria in categorias.EnumerateArray())
                    {
                        nuevo.Categorias.Add(categoria.GetString() ?? "");
                    }
                }
                if (nuevo.IdGeoapify != null) lista.Add(nuevo);
            }
            return lista;
        }
    }

    private static string? TextoDe(JsonElement objeto, string propiedad)
    {
        if (!objeto.TryGetProperty(propiedad, out JsonElement valor) || valor.ValueKind != JsonValueKind.String) return null;
        return valor.GetString();
    }

    public static async Task<List<ResultadoImportacion>> ImportarBarrios(List<Barrios> barrios)
    {
        Dictionary<string, int> categorias = BD.ObtenerIdsCategoriasPorNombre();
        List<ResultadoImportacion> resultados = new List<ResultadoImportacion>();

        foreach (Barrios barrio in barrios)
        {
            ResultadoImportacion resultado = new ResultadoImportacion();
            resultado.Barrio = barrio.Nombre;
            resultados.Add(resultado);

            string? limite = await BuscarLimiteDeBarrio(barrio.Nombre!);
            string filtro;
            if (limite != null) filtro = "place:" + limite;
            else if (barrio.Latitud != null && barrio.Longitud != null)
                filtro = "circle:" + barrio.Longitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                         barrio.Latitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",1500";
            else
            {
                resultado.Aviso = "No se pudo ubicar el barrio (Geoapify no respondió o se pasó el tope diario).";
                continue;
            }

            for (int desde = 0; desde < MaximoPorBarrio; desde += LugaresPorPagina)
            {
                List<LugarGeoapify>? pagina = await PedirLugares(filtro, desde, LugaresPorPagina, null);
                if (pagina == null)
                {
                    resultado.Aviso = "Geoapify dejó de responder: quedó lo traído hasta ahí.";
                    break;
                }

                foreach (LugarGeoapify lugar in pagina)
                {
                    if (resultado.Nuevos + resultado.Actualizados == GuardadosPorBarrio) break;
                    resultado.Traidos++;
                    if (string.IsNullOrWhiteSpace(lugar.Nombre)) { resultado.SinNombre++; continue; }

                    string? categoria = lugar.NombreCategoria();
                    if (categoria == null || !categorias.ContainsKey(categoria)) { resultado.SinCategoria++; continue; }

                    try
                    {
                        if (BD.GuardarLugarGeoapify(lugar, barrio.Id, categorias[categoria])) resultado.Nuevos++;
                        else resultado.Actualizados++;
                    }
                    catch (Exception error)
                    {
                        resultado.ConError++;
                        if (resultado.PrimerError == null)
                            resultado.PrimerError = error.Message + " | Nombre(" + (lugar.Nombre ?? "").Length + "): " + lugar.Nombre +
                                " | Calle(" + (lugar.Calle ?? "").Length + "): " + lugar.Calle +
                                " | Id(" + (lugar.IdGeoapify ?? "").Length + "): " + lugar.IdGeoapify;
                    }
                }

                if (pagina.Count < LugaresPorPagina || resultado.Nuevos + resultado.Actualizados == GuardadosPorBarrio) break;
            }
        }
        return resultados;
    }

    public static async Task<int> BuscarYGuardar(string texto)
    {
        texto = texto.Trim();
        if (ApiKey == "" || texto.Length < 3) return 0;

        string clave = texto.ToLower();
        lock (BusquedasDeHoy)
        {
            if (!BusquedasDeHoy.Add(clave)) return 0;
        }

        List<LugarGeoapify>? lugares = await PedirLugares(FiltroCABA, 0, 20, texto);
        if (lugares == null)
        {
            lock (BusquedasDeHoy) { BusquedasDeHoy.Remove(clave); }
            return 0;
        }

        Dictionary<string, int> categorias = BD.ObtenerIdsCategoriasPorNombre();
        List<Barrios> barrios = BD.ObtenerBarriosUbicados();
        if (barrios.Count == 0) return 0;

        int guardados = 0;
        foreach (LugarGeoapify lugar in lugares)
        {
            if (string.IsNullOrWhiteSpace(lugar.Nombre)) continue;
            string? categoria = lugar.NombreCategoria();
            if (categoria == null || !categorias.ContainsKey(categoria)) continue;

            Barrios barrio = barrios.OrderBy(b => Math.Pow((double)(b.Latitud!.Value - lugar.Latitud), 2) + Math.Pow((double)(b.Longitud!.Value - lugar.Longitud), 2)).First();
            BD.GuardarLugarGeoapify(lugar, barrio.Id, categorias[categoria]);
            guardados++;
        }
        return guardados;
    }
}
