using System.Globalization;

namespace Bodeglook.Models;

public static class BuscadorRestaurantes
{
    public const int ResultadosPorPagina = 5;
    public const int BusquedasRecientes = 3;

    public static bool CentroDelMapa(int idUsuario, out double latitud, out double longitud)
    {
        Barrios? barrioUsuario = BD.ObtenerBarrioDeUsuario(idUsuario);
        bool hayBarrio = barrioUsuario != null && barrioUsuario.Latitud != null && barrioUsuario.Longitud != null;
        latitud = hayBarrio ? (double)barrioUsuario!.Latitud!.Value : -34.6037;
        longitud = hayBarrio ? (double)barrioUsuario!.Longitud!.Value : -58.4000;
        return hayBarrio;
    }

    public static async Task<object> Buscar(int idUsuario, string? texto, int pagina, int? idBarrio, int? idCategoria, string? orden,
        double? latitud, double? longitud, double? norte, double? sur, double? este, double? oeste, int? zoom, bool pines)
    {
        texto = (texto ?? "").Trim();
        if (texto.Length > 100) texto = texto.Substring(0, 100);

        Barrios? barrioElegido = idBarrio == null ? null : BD.ObtenerBarrios().FirstOrDefault(b => b.Id == idBarrio);
        if (barrioElegido == null) idBarrio = null;
        if (idCategoria != null && !BD.ObtenerCategorias().Any(c => c.Id == idCategoria)) idCategoria = null;

        string ordenFinal = orden == "puntaje" ? "puntaje" : "cerca";

        string? desdeBarrio = null;
        if (!EsUbicacionValida(latitud, longitud))
        {
            latitud = null;
            longitud = null;
            Barrios? barrioUsuario = BD.ObtenerBarrioDeUsuario(idUsuario);
            if (barrioUsuario != null && barrioUsuario.Latitud != null && barrioUsuario.Longitud != null)
            {
                latitud = (double)barrioUsuario.Latitud.Value;
                longitud = (double)barrioUsuario.Longitud.Value;
                desdeBarrio = barrioUsuario.Nombre;
            }
        }

        bool soloApto = BD.ObtenerSoloApto(idUsuario);

        FiltroBusqueda filtro = new FiltroBusqueda();
        filtro.Texto = texto;
        filtro.IdUsuario = idUsuario;
        filtro.SoloApto = soloApto;
        filtro.IdBarrio = idBarrio;
        filtro.IdCategoria = idCategoria;
        bool hayZona = EsZonaValida(norte, sur, este, oeste);
        if (hayZona)
        {
            filtro.Norte = norte;
            filtro.Sur = sur;
            filtro.Este = este;
            filtro.Oeste = oeste;
        }
        filtro.Limite = LimiteSegunZoom(hayZona ? zoom : null);

        FiltroBusqueda filtroCiudad = new FiltroBusqueda();
        filtroCiudad.Texto = texto;
        filtroCiudad.IdUsuario = idUsuario;
        filtroCiudad.SoloApto = soloApto;
        filtroCiudad.IdCategoria = idCategoria;

        List<TarjetaRestaurante> todos = TarjetaRestaurante.CargarTodos(idUsuario);
        List<int> restriccionesUsuario = BD.ObtenerRestriccionesDeUsuario(idUsuario).Select(r => r.Id).ToList();
        List<TarjetaRestaurante> encontrados = Filtrar(todos, filtro, restriccionesUsuario);

        if (encontrados.Count == 0 && texto != "" && pagina <= 1 && !hayZona
            && (idBarrio == null || Filtrar(todos, filtroCiudad, restriccionesUsuario).Count == 0))
        {
            if (await Geoapify.BuscarYGuardar(texto) > 0)
            {
                todos = TarjetaRestaurante.CargarTodos(idUsuario);
                encontrados = Filtrar(todos, filtro, restriccionesUsuario);
            }
        }
        int total = encontrados.Count;
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)ResultadosPorPagina));
        if (pagina < 1) pagina = 1;
        if (pagina > totalPaginas) pagina = totalPaginas;

        if (pagina == 1 && texto != "" && !hayZona) BD.GuardarBusqueda(idUsuario, texto);

        foreach (TarjetaRestaurante lugar in encontrados)
        {
            lugar.DistanciaKm = lugar.CalcularDistancia(latitud, longitud);
        }

        int? idDestacado = null;
        if (idBarrio == null && ordenFinal == "cerca")
        {
            TarjetaRestaurante? destacado = encontrados.Where(lugar => lugar.EsPro)
                .OrderBy(lugar => lugar.DistanciaKm == null ? 1 : 0)
                .ThenBy(lugar => lugar.DistanciaKm)
                .ThenBy(lugar => lugar.Id)
                .FirstOrDefault();
            if (destacado != null) idDestacado = destacado.Id;
        }

        BarrioConResultados? otroBarrio = null;
        if (total == 0 && idBarrio != null && texto != "")
            otroBarrio = BarrioConMasResultados(Filtrar(todos, filtroCiudad, restriccionesUsuario), idBarrio.Value);

        List<object> resultados = new List<object>();
        foreach (TarjetaRestaurante lugar in Ordenar(encontrados, ordenFinal, idDestacado)
            .Skip((pagina - 1) * ResultadosPorPagina).Take(ResultadosPorPagina))
        {
            resultados.Add(new
            {
                id = lugar.Id,
                nombre = lugar.Nombre,
                foto = lugar.Foto,
                categoria = lugar.Categoria,
                barrio = lugar.Barrio,
                puntaje = TextoPuntaje(lugar.Puntaje),
                esFavorito = lugar.EsFavorito,
                destacado = lugar.Id == idDestacado,
                origen = lugar.Origen,
                latitud = lugar.Latitud,
                longitud = lugar.Longitud,
                distanciaKm = lugar.DistanciaKm == null ? (double?)null : Math.Round(lugar.DistanciaKm.Value, 3)
            });
        }

        List<object>? listaPines = null;
        if (pines)
        {
            listaPines = new List<object>();
            foreach (TarjetaRestaurante lugar in encontrados.Where(l => l.Latitud != null && l.Longitud != null).OrderBy(l => l.Id))
            {
                listaPines.Add(new
                {
                    id = lugar.Id,
                    nombre = lugar.Nombre,
                    foto = lugar.Foto,
                    categoria = lugar.Categoria,
                    puntaje = TextoPuntaje(lugar.Puntaje),
                    latitud = lugar.Latitud,
                    longitud = lugar.Longitud
                });
            }
        }

        return new
        {
            pagina,
            totalPaginas,
            total,
            resultados,
            pines = listaPines,
            recientes = BD.ObtenerBusquedasRecientes(idUsuario, BusquedasRecientes),
            desdeBarrio,
            barrioElegido = barrioElegido?.Nombre,
            otroBarrio = otroBarrio == null ? null : new { id = otroBarrio.Id, nombre = otroBarrio.Nombre, cantidad = otroBarrio.Cantidad }
        };
    }

    private static readonly List<string> PalabrasVacias = new List<string> { "en", "de", "la", "el", "los", "las", "del", "con", "y" };

    private static List<TarjetaRestaurante> Filtrar(List<TarjetaRestaurante> todos, FiltroBusqueda filtro, List<int> restriccionesUsuario)
    {
        List<string> palabras = new List<string>();
        foreach (string palabra in filtro.Texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!PalabrasVacias.Contains(palabra.ToLower()) && palabras.Count < 5) palabras.Add(palabra);
        }

        List<TarjetaRestaurante> encontrados = new List<TarjetaRestaurante>();
        foreach (TarjetaRestaurante lugar in todos)
        {
            if (filtro.SoloApto && !lugar.EsAptoPara(restriccionesUsuario)) continue;
            if (filtro.IdBarrio != null && lugar.IdBarrio != filtro.IdBarrio) continue;
            if (filtro.IdCategoria != null && lugar.IdCategoria != filtro.IdCategoria) continue;
            if (!TieneTodasLasPalabras(lugar, palabras)) continue;
            if (filtro.TieneZona && !EstaEnLaZona(lugar, filtro)) continue;
            encontrados.Add(lugar);
        }

        if (filtro.Limite == null) return encontrados;

        List<int> mejores = encontrados
            .OrderByDescending(lugar => lugar.PuntajeParaOrdenar())
            .ThenByDescending(lugar => lugar.EsPro)
            .ThenBy(lugar => lugar.Id)
            .Take(filtro.Limite.Value)
            .Select(lugar => lugar.Id)
            .ToList();
        return encontrados.Where(lugar => mejores.Contains(lugar.Id)).ToList();
    }

    private static bool TieneTodasLasPalabras(TarjetaRestaurante lugar, List<string> palabras)
    {
        string nombre = SinTildes(lugar.Nombre);
        string barrio = SinTildes(lugar.Barrio);
        string categoria = SinTildes(lugar.Categoria);

        foreach (string palabra in palabras)
        {
            string conS = SinTildes(palabra);
            string sinS = SinTildes(palabra.Length > 3 && palabra.EndsWith("s") ? palabra.Substring(0, palabra.Length - 1) : palabra);
            bool esta = nombre.Contains(conS) || barrio.Contains(conS) || categoria.Contains(conS)
                     || nombre.Contains(sinS) || barrio.Contains(sinS) || categoria.Contains(sinS);
            if (!esta) return false;
        }
        return true;
    }

    private static string SinTildes(string? texto)
    {
        string resultado = "";
        foreach (char letra in (texto ?? "").ToLower().Normalize(System.Text.NormalizationForm.FormD))
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(letra) != System.Globalization.UnicodeCategory.NonSpacingMark)
                resultado += letra;
        }
        return resultado;
    }

    private static bool EstaEnLaZona(TarjetaRestaurante lugar, FiltroBusqueda filtro)
    {
        if (lugar.Latitud == null || lugar.Longitud == null) return false;
        double latitud = (double)lugar.Latitud.Value;
        double longitud = (double)lugar.Longitud.Value;
        return latitud >= filtro.Sur && latitud <= filtro.Norte && longitud >= filtro.Oeste && longitud <= filtro.Este;
    }

    private static List<TarjetaRestaurante> Ordenar(List<TarjetaRestaurante> lugares, string orden, int? idDestacado)
    {
        return lugares
            .OrderBy(lugar => lugar.Id == idDestacado ? 0 : 1)
            .ThenBy(lugar => orden == "puntaje" && lugar.Puntaje == null ? 1 : 0)
            .ThenByDescending(lugar => orden == "puntaje" ? lugar.Puntaje : null)
            .ThenBy(lugar => lugar.DistanciaKm == null ? 1 : 0)
            .ThenBy(lugar => lugar.DistanciaKm)
            .ThenBy(lugar => lugar.Puntaje == null ? 1 : 0)
            .ThenByDescending(lugar => lugar.Puntaje)
            .ThenBy(lugar => lugar.Id)
            .ToList();
    }

    private static BarrioConResultados? BarrioConMasResultados(List<TarjetaRestaurante> enLaCiudad, int idBarrioElegido)
    {
        return enLaCiudad
            .Where(lugar => lugar.IdBarrio != idBarrioElegido)
            .GroupBy(lugar => lugar.IdBarrio)
            .Select(grupo => new BarrioConResultados { Id = grupo.Key, Nombre = grupo.First().Barrio, Cantidad = grupo.Count() })
            .OrderByDescending(barrio => barrio.Cantidad)
            .ThenBy(barrio => barrio.Nombre, StringComparer.Create(new System.Globalization.CultureInfo("es-ES"), true))
            .FirstOrDefault();
    }

    private static string? TextoPuntaje(double? puntaje)
    {
        return puntaje == null ? null : puntaje.Value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static int LimiteSegunZoom(int? zoom)
    {
        if (zoom == null) return 80;
        if (zoom <= 11) return 15;
        if (zoom == 12) return 25;
        if (zoom == 13) return 50;
        if (zoom == 14) return 100;
        if (zoom == 15) return 200;
        return 400;
    }

    private static bool EsZonaValida(double? norte, double? sur, double? este, double? oeste)
    {
        if (norte == null || sur == null || este == null || oeste == null) return false;
        if (!double.IsFinite(norte.Value) || !double.IsFinite(sur.Value) || !double.IsFinite(este.Value) || !double.IsFinite(oeste.Value)) return false;
        if (norte > 90 || sur < -90 || este > 180 || oeste < -180) return false;
        return norte > sur && este > oeste;
    }

    private static bool EsUbicacionValida(double? latitud, double? longitud)
    {
        if (latitud == null || longitud == null) return false;
        if (!double.IsFinite(latitud.Value) || !double.IsFinite(longitud.Value)) return false;
        return latitud >= -90 && latitud <= 90 && longitud >= -180 && longitud <= 180;
    }
}
