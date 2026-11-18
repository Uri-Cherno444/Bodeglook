namespace Bodeglook.Models;

// No es una tabla: junta los datos de Restaurantes, Categorias, Barrios
// y el promedio de Reseñas que muestra cada tarjeta de lugar.
public class TarjetaRestaurante
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    // Siempre tiene una ruta: la portada del restaurante o la imagen por defecto (ver Fotos)
    private string? _foto;
    public string? Foto
    {
        get { return Fotos.Portada(Id, _foto); }
        set { _foto = value; }
    }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public double? Puntaje { get; set; }
    public bool EsFavorito { get; set; }

    // Para 05 · Descubrir: chip "Recomendado" y pin en el mapa
    public bool EsPro { get; set; }
    // "Manual" o "Geoapify": los de Geoapify llevan los créditos de OpenStreetMap
    public string? Origen { get; set; }
    // Las coordenadas del lugar o, si no tiene, las del centro de su barrio
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    // Distancia en km desde el usuario: su ubicación del navegador o, si no la dio, el centro del barrio de su perfil
    // (fórmula de Haversine). null si falta alguna coordenada.
    public double? DistanciaKm { get; set; }

    // ---------- Lo que trae ListarRestaurantesBuscador y completa CargarTodos ----------

    public int IdBarrio { get; set; }
    public int IdCategoria { get; set; }
    // El centro del barrio del lugar
    public decimal? LatitudBarrio { get; set; }
    public decimal? LongitudBarrio { get; set; }
    // Las reseñas publicadas (las que tienen más de 24 horas): cuántas son y la suma de sus puntajes
    public int CantidadPublicadas { get; set; }
    public int SumaPublicadas { get; set; }
    // El promedio de todas las reseñas, publicadas o no (null si no tiene)
    public double? PromedioTodas { get; set; }
    // Los Id de las restricciones que cumple el lugar
    public List<int> IdsRestricciones { get; set; } = new List<int>();

    // Todos los restaurantes, cada uno con su puntaje, sus restricciones y si es favorito del usuario.
    // Lo usan el buscador de 05 · Descubrir y "Recomendado para vos" de Inicio.
    public static List<TarjetaRestaurante> CargarTodos(int idUsuario)
    {
        List<TarjetaRestaurante> lugares = BD.ObtenerRestaurantesParaBuscar();
        List<Reseñas> resenas = BD.ObtenerPuntajesDeResenas();
        List<RestriccionesRestaurantes> restricciones = BD.ObtenerRestriccionesDeTodosLosRestaurantes();
        List<int> favoritos = BD.ObtenerIdsFavoritos(idUsuario);

        // Una reseña se publica 24 horas después de su fecha: el puntaje que se muestra solo cuenta las publicadas
        DateTime limite = DateTime.Now.AddHours(-24);
        foreach (TarjetaRestaurante lugar in lugares)
        {
            if (lugar.Latitud == null) lugar.Latitud = lugar.LatitudBarrio;
            if (lugar.Longitud == null) lugar.Longitud = lugar.LongitudBarrio;

            int cantidadTodas = 0;
            int sumaTodas = 0;
            foreach (Reseñas resena in resenas)
            {
                if (resena.IdRestaurante != lugar.Id) continue;
                cantidadTodas++;
                sumaTodas += resena.Puntaje ?? 0;
                if (resena.Fecha <= limite)
                {
                    lugar.CantidadPublicadas++;
                    lugar.SumaPublicadas += resena.Puntaje ?? 0;
                }
            }
            if (cantidadTodas > 0) lugar.PromedioTodas = (double)sumaTodas / cantidadTodas;
            if (lugar.CantidadPublicadas > 0) lugar.Puntaje = (double)lugar.SumaPublicadas / lugar.CantidadPublicadas;

            lugar.IdsRestricciones = restricciones.Where(r => r.IdRestaurante == lugar.Id).Select(r => r.IdRestriccion ?? 0).ToList();
            lugar.EsFavorito = favoritos.Contains(lugar.Id);
        }
        return lugares;
    }

    // "Solo lo apto para mí": si el usuario marcó restricciones, el lugar tiene que cumplir al menos una
    public bool EsAptoPara(List<int> restriccionesUsuario)
    {
        if (restriccionesUsuario.Count == 0) return true;
        return IdsRestricciones.Any(id => restriccionesUsuario.Contains(id));
    }

    // Para elegir "los mejores" cuando hay que mostrar pocos: el promedio de las reseñas publicadas
    // como si además tuviera 3 reseñas de 3,5. Así un lugar con una sola reseña de 5 no le gana a uno con muchas de 4,8.
    public double PuntajeParaOrdenar()
    {
        return (SumaPublicadas + 10.5) / (CantidadPublicadas + 3);
    }

    // Distancia en km entre el usuario y el lugar (fórmula de Haversine). null si falta alguna coordenada.
    public double? CalcularDistancia(double? latitudUsuario, double? longitudUsuario)
    {
        if (latitudUsuario == null || longitudUsuario == null || Latitud == null || Longitud == null) return null;

        double latitud1 = latitudUsuario.Value;
        double longitud1 = longitudUsuario.Value;
        double latitud2 = (double)Latitud.Value;
        double longitud2 = (double)Longitud.Value;

        double difLatitud = (latitud2 - latitud1) * Math.PI / 180;
        double difLongitud = (longitud2 - longitud1) * Math.PI / 180;
        double a = Math.Sin(difLatitud / 2) * Math.Sin(difLatitud / 2)
                 + Math.Cos(latitud1 * Math.PI / 180) * Math.Cos(latitud2 * Math.PI / 180)
                 * Math.Sin(difLongitud / 2) * Math.Sin(difLongitud / 2);
        // 6371 es el radio de la Tierra en km
        return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // ---------- 03 · Inicio ----------

    // "Tus favoritos" de Inicio: con un chip de restricción elegido, solo los que ofrecen esa opción
    public static List<TarjetaRestaurante> FavoritosConRestriccion(int idUsuario, int? idRestriccion)
    {
        List<TarjetaRestaurante> favoritos = BD.ObtenerFavoritos(idUsuario);
        if (idRestriccion == null) return favoritos;

        List<int> conRestriccion = BD.ObtenerRestaurantesConRestriccion(idRestriccion.Value);
        return favoritos.Where(f => conRestriccion.Contains(f.Id)).ToList();
    }

    // "Recomendado para vos": lugares que se parecen a los gustos del usuario y que cambian en cada visita.
    // Cada lugar suma puntos de afinidad:
    //   +2 si está en un barrio que le interesa (sus zonas o el barrio de su perfil)
    //   +2 si es de una categoría que ya marcó como favorita o reseñó con 4 o 5
    //   +1 si tiene buen puntaje (4 o más, contando todas sus reseñas)
    // A cada uno se le suma un número al azar (0 a 3) y se toman los de arriba: los que más le gustan salen seguido,
    // pero no son siempre los mismos. No muestra lo que ya tiene en favoritos (eso va en "Tus favoritos").
    // soloApto: solo los que cumplen al menos una restricción del usuario. idRestriccion: el chip elegido.
    public static List<TarjetaRestaurante> Recomendados(int cantidad, int idUsuario, bool soloApto, int? idRestriccion)
    {
        List<TarjetaRestaurante> todos = CargarTodos(idUsuario);
        List<int> restriccionesUsuario = BD.ObtenerRestriccionesDeUsuario(idUsuario).Select(r => r.Id).ToList();
        List<int> zonas = BD.ObtenerZonasDeUsuario(idUsuario);
        int? barrioUsuario = BD.ObtenerBarrioDeUsuario(idUsuario)?.Id;

        // Las categorías que le gustan: las de sus favoritos y las de los lugares que reseñó con 4 o 5
        List<int> categoriasQueLeGustan = todos.Where(lugar => lugar.EsFavorito).Select(lugar => lugar.IdCategoria).ToList();
        foreach (Reseñas resena in BD.ObtenerPuntajesDeResenas())
        {
            if (resena.IdUsuario != idUsuario || resena.Puntaje < 4) continue;
            TarjetaRestaurante? resenado = todos.FirstOrDefault(lugar => lugar.Id == resena.IdRestaurante);
            if (resenado != null) categoriasQueLeGustan.Add(resenado.IdCategoria);
        }

        List<TarjetaRestaurante> candidatos = new List<TarjetaRestaurante>();
        foreach (TarjetaRestaurante lugar in todos)
        {
            if (lugar.EsFavorito) continue;
            if (soloApto && !lugar.EsAptoPara(restriccionesUsuario)) continue;
            if (idRestriccion != null && !lugar.IdsRestricciones.Contains(idRestriccion.Value)) continue;

            int afinidad = 0;
            if (zonas.Contains(lugar.IdBarrio) || lugar.IdBarrio == barrioUsuario) afinidad += 2;
            if (categoriasQueLeGustan.Contains(lugar.IdCategoria)) afinidad += 2;
            if (lugar.PromedioTodas >= 4) afinidad += 1;
            lugar.Afinidad = afinidad;
            candidatos.Add(lugar);
        }

        Random azar = new Random();
        return candidatos
            .OrderByDescending(lugar => lugar.Afinidad + azar.NextDouble() * 3)
            .Take(cantidad)
            .ToList();
    }

    // Los puntos de afinidad de "Recomendado para vos" (ver Recomendados)
    public int Afinidad { get; set; }
}
