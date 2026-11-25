namespace Bodeglook.Models;

public class TarjetaRestaurante
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
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

    public bool EsPro { get; set; }
    public string? Origen { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    public double? DistanciaKm { get; set; }

    public int IdBarrio { get; set; }
    public int IdCategoria { get; set; }
    public decimal? LatitudBarrio { get; set; }
    public decimal? LongitudBarrio { get; set; }
    public int CantidadPublicadas { get; set; }
    public int SumaPublicadas { get; set; }
    public double? PromedioTodas { get; set; }
    public List<int> IdsRestricciones { get; set; } = new List<int>();

    public static List<TarjetaRestaurante> CargarTodos(int idUsuario)
    {
        List<TarjetaRestaurante> lugares = BD.ObtenerRestaurantesParaBuscar();
        ILookup<int, int> restricciones = BD.ObtenerRestriccionesPorRestaurante();
        HashSet<int> favoritos = BD.ObtenerIdsFavoritos(idUsuario);

        foreach (TarjetaRestaurante lugar in lugares)
        {
            if (lugar.Latitud == null) lugar.Latitud = lugar.LatitudBarrio;
            if (lugar.Longitud == null) lugar.Longitud = lugar.LongitudBarrio;

            if (lugar.CantidadPublicadas > 0) lugar.Puntaje = (double)lugar.SumaPublicadas / lugar.CantidadPublicadas;

            lugar.IdsRestricciones = restricciones[lugar.Id].ToList();
            lugar.EsFavorito = favoritos.Contains(lugar.Id);
        }
        return lugares;
    }

    public bool EsAptoPara(List<int> restriccionesUsuario)
    {
        if (restriccionesUsuario.Count == 0) return true;
        return IdsRestricciones.Any(id => restriccionesUsuario.Contains(id));
    }

    public double PuntajeParaOrdenar()
    {
        return (SumaPublicadas + 10.5) / (CantidadPublicadas + 3);
    }

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
        return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static List<TarjetaRestaurante> FavoritosConRestriccion(int idUsuario, int? idRestriccion)
    {
        List<TarjetaRestaurante> favoritos = BD.ObtenerFavoritos(idUsuario);
        if (idRestriccion == null) return favoritos;

        List<int> conRestriccion = BD.ObtenerRestaurantesConRestriccion(idRestriccion.Value);
        return favoritos.Where(f => conRestriccion.Contains(f.Id)).ToList();
    }

    public static List<TarjetaRestaurante> Recomendados(int cantidad, int idUsuario, bool soloApto, int? idRestriccion)
    {
        List<TarjetaRestaurante> todos = CargarTodos(idUsuario);
        List<int> restriccionesUsuario = BD.ObtenerIdsRestriccionesDeUsuario(idUsuario);
        List<int> zonas = BD.ObtenerZonasDeUsuario(idUsuario);
        int? barrioUsuario = BD.ObtenerBarrioDeUsuario(idUsuario)?.Id;

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

    public int Afinidad { get; set; }
}
