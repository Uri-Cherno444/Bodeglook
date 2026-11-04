using System.Text.Json;

namespace Bodeglook.Models;

// Conexión con Geoapify (Places API y Geocoding API). Los datos son de OpenStreetMap.
// Todas las llamadas las hace el servidor. La API key viene de la configuración (user-secrets "Geoapify:ApiKey"),
// nunca se escribe en el código ni en los logs: si algo falla solo se anota el código de respuesta.
// Plan gratis: 3.000 créditos por día y 5 pedidos por segundo. Por eso:
//   - los usuarios siempre leen de nuestra base; Geoapify se llama solo al importar y cuando una búsqueda no encuentra nada
//   - hay una pausa entre pedidos y un tope diario propio, un poco menor al del plan
// Si Geoapify no responde o se pasó el tope, todos los métodos devuelven vacío y la página sigue con lo que hay en la base.
public static class Geoapify
{
    // Se carga en Program.cs desde la configuración
    public static string ApiKey = "";

    private const string Servidor = "https://api.geoapify.com";
    private const int CreditosPorDia = 2500;
    private const int PausaEntrePedidos = 250;
    private const int LugaresPorPagina = 100;
    private const int MaximoPorBarrio = 500;

    // Ciudad Autónoma de Buenos Aires: rect:lonOeste,latSur,lonEste,latNorte
    private const string FiltroCABA = "rect:-58.5315,-34.7056,-58.3350,-34.5265";

    // Restaurantes, bares, cafés, heladerías, pizzerías/hamburgueserías/tacos de paso, bodegas y vinotecas.
    // Un código padre incluye a sus hijos (catering.restaurant trae también catering.restaurant.pizza, etc.).
    private const string Categorias = "catering.restaurant,catering.bar,catering.pub,catering.cafe,catering.ice_cream," +
        "catering.fast_food.pizza,catering.fast_food.burger,catering.fast_food.tacos," +
        "commercial.food_and_drink.drinks,production.winery";

    private static readonly HttpClient Cliente = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    private static readonly SemaphoreSlim Turno = new SemaphoreSlim(1, 1);
    private static DateTime ultimoPedido = DateTime.MinValue;
    private static DateTime diaDeLosCreditos = DateTime.Today;
    private static int creditosUsados = 0;
    // Los textos que ya se buscaron hoy: si no dieron nada, no se vuelve a preguntar
    private static readonly HashSet<string> BusquedasDeHoy = new HashSet<string>();

    // ---------- Pedidos ----------

    // Hace un GET y devuelve el JSON, o null si no hay clave, se pasó el tope, no respondió o dio error.
    // creditos: lo que cuesta el pedido (Places: 1 por cada 20 lugares, Geocoding: 1)
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

            // Máximo 5 pedidos por segundo
            int espera = PausaEntrePedidos - (int)(DateTime.Now - ultimoPedido).TotalMilliseconds;
            if (espera > 0) await Task.Delay(espera);
            ultimoPedido = DateTime.Now;
            creditosUsados += creditos;

            // La clave va en un encabezado, no en la dirección
            HttpRequestMessage pedido = new HttpRequestMessage(HttpMethod.Get, Servidor + ruta);
            pedido.Headers.Add("x-api-key", ApiKey);
            using (HttpResponseMessage respuesta = await Cliente.SendAsync(pedido))
            {
                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine("Geoapify respondió con el código " + (int)respuesta.StatusCode + ".");
                    return null;
                }
                return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            }
        }
        catch
        {
            // No se anota el mensaje del error: podría traer la dirección con la clave
            Console.WriteLine("Geoapify no respondió.");
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

    // ---------- Geocoding: el límite de un barrio ----------

    // El place_id del barrio, para pedir los lugares que están adentro. null si no lo encuentra.
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

    // ---------- Places: los lugares ----------

    // Una página de lugares. filtro: "place:ID", "circle:lon,lat,metros" o "rect:...".
    // nombre: opcional, trae lugares que se llamen así. Devuelve null si Geoapify no respondió.
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

    // ---------- Importación ----------

    // Trae los lugares de cada barrio, de a páginas, y los guarda con Origen = "Geoapify".
    // Si el lugar ya existe (mismo IdGeoapify) se actualiza en vez de duplicarlo.
    public static async Task<List<ResultadoImportacion>> ImportarBarrios(List<Barrios> barrios)
    {
        Dictionary<string, int> categorias = BD.ObtenerCategorias().ToDictionary(c => c.Nombre!, c => c.Id);
        List<ResultadoImportacion> resultados = new List<ResultadoImportacion>();

        foreach (Barrios barrio in barrios)
        {
            ResultadoImportacion resultado = new ResultadoImportacion();
            resultado.Barrio = barrio.Nombre;
            resultados.Add(resultado);

            // El límite real del barrio; si no se encuentra, un círculo de 1,5 km alrededor de su centro
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
                    resultado.Traidos++;
                    if (string.IsNullOrWhiteSpace(lugar.Nombre)) { resultado.SinNombre++; continue; }

                    string? categoria = lugar.NombreCategoria();
                    if (categoria == null || !categorias.ContainsKey(categoria)) { resultado.SinCategoria++; continue; }

                    if (BD.GuardarLugarGeoapify(lugar, barrio.Id, categorias[categoria])) resultado.Nuevos++;
                    else resultado.Actualizados++;
                }

                if (pagina.Count < LugaresPorPagina) break;
            }
        }
        return resultados;
    }

    // ---------- Buscador ----------

    // Para cuando una búsqueda no encontró nada en nuestra base: busca ese nombre en Geoapify (dentro de CABA),
    // guarda lo que encuentre y devuelve cuántos lugares guardó. Un mismo texto se pregunta una sola vez por día.
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
            // No respondió: se puede volver a intentar más tarde
            lock (BusquedasDeHoy) { BusquedasDeHoy.Remove(clave); }
            return 0;
        }

        Dictionary<string, int> categorias = BD.ObtenerCategorias().ToDictionary(c => c.Nombre!, c => c.Id);
        List<Barrios> barrios = BD.ObtenerBarriosConCoordenadas().Where(b => b.Latitud != null && b.Longitud != null).ToList();
        if (barrios.Count == 0) return 0;

        int guardados = 0;
        foreach (LugarGeoapify lugar in lugares)
        {
            if (string.IsNullOrWhiteSpace(lugar.Nombre)) continue;
            string? categoria = lugar.NombreCategoria();
            if (categoria == null || !categorias.ContainsKey(categoria)) continue;

            // Nuestra base tiene pocos barrios: el lugar queda en el que tiene el centro más cerca
            Barrios barrio = barrios.OrderBy(b => Math.Pow((double)(b.Latitud!.Value - lugar.Latitud), 2) + Math.Pow((double)(b.Longitud!.Value - lugar.Longitud), 2)).First();
            BD.GuardarLugarGeoapify(lugar, barrio.Id, categorias[categoria]);
            guardados++;
        }
        return guardados;
    }
}
