using System.Globalization;

namespace Bodeglook.Models;

// No es una tabla: la búsqueda de 05 · Descubrir Restaurantes (lista, mapa, filtros y Geoapify).
public static class BuscadorRestaurantes
{
    // 5 resultados por página y las últimas 3 búsquedas
    public const int ResultadosPorPagina = 5;
    public const int BusquedasRecientes = 3;

    // Donde arranca el mapa si el navegador no da la ubicación: el centro del barrio del perfil
    // (si no tiene, el centro de la Ciudad). Devuelve true si es el barrio del perfil.
    public static bool CentroDelMapa(int idUsuario, out double latitud, out double longitud)
    {
        Barrios? barrioUsuario = BD.ObtenerBarrioDeUsuario(idUsuario);
        bool hayBarrio = barrioUsuario != null && barrioUsuario.Latitud != null && barrioUsuario.Longitud != null;
        latitud = hayBarrio ? (double)barrioUsuario!.Latitud!.Value : -34.6037;
        longitud = hayBarrio ? (double)barrioUsuario!.Longitud!.Value : -58.4000;
        return hayBarrio;
    }

    // Lo que devuelve /Restaurantes/Resultados a descubrir.js:
    // { pagina, totalPaginas, total, resultados, pines, recientes, desdeBarrio, barrioElegido, otroBarrio }.
    // texto: lo que escribió. idBarrio: el desplegable "Barrio" (sin él, todos). idCategoria: el chip elegido (sin él, todas).
    // norte, sur, este, oeste y zoom: lo que se ve en el mapa. Con zona, la lista y los pins son solo de esa zona y
    //   cuanto menos zoom, menos lugares (los mejores). Sin zona (recién se buscó o se cambió un filtro) es toda la Ciudad.
    // pines: si es true también devuelve en "pines" los lugares para el mapa (los mismos que la lista, todas las páginas).
    // orden: "cerca" (Más cerca, el de siempre) o "puntaje" (Mejor puntuados).
    //   Sin barrio y ordenando por cercanía, 1 Pro destacado arriba de la página 1 (el más cercano).
    // latitud y longitud: la ubicación del navegador. Solo se usan para medir las distancias: no se guardan.
    //   Si no vienen, se mide desde el centro del barrio del perfil y se devuelve su nombre en desdeBarrio (para el aviso).
    // otroBarrio solo viene si no hubo nada en el barrio elegido pero el texto está en otro.
    // Cada búsqueda nueva (pagina 1 con texto, sin zona) se guarda.
    public static async Task<object> Buscar(int idUsuario, string? texto, int pagina, int? idBarrio, int? idCategoria, string? orden,
        double? latitud, double? longitud, double? norte, double? sur, double? este, double? oeste, int? zoom, bool pines)
    {
        texto = (texto ?? "").Trim();
        if (texto.Length > 100) texto = texto.Substring(0, 100);

        // Un barrio o una categoría que no existen se toman como "todos"
        Barrios? barrioElegido = idBarrio == null ? null : BD.ObtenerBarrios().FirstOrDefault(b => b.Id == idBarrio);
        if (barrioElegido == null) idBarrio = null;
        if (idCategoria != null && !BD.ObtenerCategorias().Any(c => c.Id == idCategoria)) idCategoria = null;

        // "puntaje" (Mejor puntuados) o, para cualquier otro valor, "cerca"
        string ordenFinal = orden == "puntaje" ? "puntaje" : "cerca";

        // Si el navegador no mandó una ubicación válida, se mide desde el barrio del perfil
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

        // "Solo lo apto para mí" del onboarding o de Editar perfil (Usuarios.SoloApto)
        bool soloApto = BD.ObtenerSoloApto(idUsuario);

        // Lo que se pide: la búsqueda, el barrio, la categoría y, si el mapa mandó una zona válida, esa zona con su límite por zoom
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

        // Lo mismo pero en toda la Ciudad y sin límite (para saber si el texto existe en otro lado y para avisar en qué barrio hay)
        FiltroBusqueda filtroCiudad = new FiltroBusqueda();
        filtroCiudad.Texto = texto;
        filtroCiudad.IdUsuario = idUsuario;
        filtroCiudad.SoloApto = soloApto;
        filtroCiudad.IdCategoria = idCategoria;

        // Todos los lugares se traen una sola vez y se filtran acá
        List<TarjetaRestaurante> todos = TarjetaRestaurante.CargarTodos(idUsuario);
        List<int> restriccionesUsuario = BD.ObtenerRestriccionesDeUsuario(idUsuario).Select(r => r.Id).ToList();
        List<TarjetaRestaurante> encontrados = Filtrar(todos, filtro, restriccionesUsuario);

        // Se busca primero en nuestra base. Solo si no hay nada se pregunta a Geoapify, se guarda lo que encuentre y se vuelve a buscar.
        // Con un barrio elegido, a Geoapify se le pregunta solo si el texto tampoco está en otros barrios (así no se gastan créditos de más).
        // Al mover el mapa no se pregunta a Geoapify. Si no responde o se pasó el límite, queda "sin resultados" como siempre.
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

        // Sin barrio y ordenando por cercanía: como máximo 1 Pro destacado ("Recomendado"), el más cercano
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

        // Nada en el barrio elegido: ¿en qué otro barrio sí está ese texto?
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
                // Solo el Pro destacado lleva el chip "Recomendado"
                destacado = lugar.Id == idDestacado,
                origen = lugar.Origen,
                latitud = lugar.Latitud,
                longitud = lugar.Longitud,
                // Con 3 decimales (metros): descubrir.js lo pasa a cuadras o km
                distanciaKm = lugar.DistanciaKm == null ? (double?)null : Math.Round(lugar.DistanciaKm.Value, 3)
            });
        }

        // Los pins del mapa: los mismos lugares que la lista (todas las páginas), solo con lo que necesita cada pin.
        // Los que no tienen coordenadas (ni propias ni de su barrio) no van.
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

    // ---------- Filtros y orden ----------

    // Las palabras que no sirven para buscar ("parrilla en Palermo" -> "parrilla", "Palermo")
    private static readonly List<string> PalabrasVacias = new List<string> { "en", "de", "la", "el", "los", "las", "del", "con", "y" };

    // Los lugares que cumplen el filtro:
    // - soloApto: solo los que cumplen al menos una restricción del usuario (si no marcó ninguna, todos)
    // - el barrio y la categoría elegidos (null = todos)
    // - el texto: cada palabra (hasta 5, sin las vacías) tiene que estar en el nombre, el barrio o la categoría.
    //   No importan tildes ni mayúsculas ("nunez" encuentra "Núñez") y "Parrillas" encuentra "Parrilla".
    // - si hay zona, solo lo que cae dentro de lo que se ve en el mapa
    // - si hay Limite, solo los Limite mejores (ver TarjetaRestaurante.PuntajeParaOrdenar; si empatan, primero los Pro)
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

    // Cada palabra tiene que estar en el nombre, el barrio o la categoría (también sin la "s" final)
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

    // "Núñez" -> "nunez": minúsculas y sin tildes, para comparar como lo escribió la persona
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

    // El lugar (o el centro de su barrio, si no tiene coordenadas) cae dentro de lo que se ve en el mapa
    private static bool EstaEnLaZona(TarjetaRestaurante lugar, FiltroBusqueda filtro)
    {
        if (lugar.Latitud == null || lugar.Longitud == null) return false;
        double latitud = (double)lugar.Latitud.Value;
        double longitud = (double)lugar.Longitud.Value;
        return latitud >= filtro.Sur && latitud <= filtro.Norte && longitud >= filtro.Oeste && longitud <= filtro.Este;
    }

    // El orden de la lista: el Pro destacado arriba de todo; después, con "puntaje", los mejor puntuados primero;
    // y siempre del más cerca al más lejos. Los que no tienen distancia o reseñas van al final.
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

    // El barrio (que no sea el elegido) con más lugares de la búsqueda. Si empatan, el primero por nombre.
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

    // 4.5 -> "4.5" (con punto, como lo espera descubrir.js). null si no tiene reseñas.
    private static string? TextoPuntaje(double? puntaje)
    {
        return puntaje == null ? null : puntaje.Value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    // Cuántos lugares se muestran según el zoom del mapa: cuanto más lejos, menos y los mejores; cuanto más cerca, más.
    // Sin zoom (se busca en toda la Ciudad) son los 80 mejores.
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

    // Una zona de mapa con números de verdad, norte arriba del sur y este a la derecha del oeste
    private static bool EsZonaValida(double? norte, double? sur, double? este, double? oeste)
    {
        if (norte == null || sur == null || este == null || oeste == null) return false;
        if (!double.IsFinite(norte.Value) || !double.IsFinite(sur.Value) || !double.IsFinite(este.Value) || !double.IsFinite(oeste.Value)) return false;
        if (norte > 90 || sur < -90 || este > 180 || oeste < -180) return false;
        return norte > sur && este > oeste;
    }

    // Una latitud entre -90 y 90 y una longitud entre -180 y 180 (y que sean números de verdad)
    private static bool EsUbicacionValida(double? latitud, double? longitud)
    {
        if (latitud == null || longitud == null) return false;
        if (!double.IsFinite(latitud.Value) || !double.IsFinite(longitud.Value)) return false;
        return latitud >= -90 && latitud <= 90 && longitud >= -180 && longitud <= 180;
    }
}
