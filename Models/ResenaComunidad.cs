namespace Bodeglook.Models;

public class ResenaComunidad
{
    public int Id { get; set; }
    public int Puntaje { get; set; }
    public string? Comentario { get; set; }
    public string? Foto { get; set; }
    public DateTime Fecha { get; set; }
    public string? NombreCompleto { get; set; }
    public string? FotoPerfil { get; set; }

    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }

    public string ObtenerIniciales()
    {
        Usuarios autor = new Usuarios();
        autor.NombreCompleto = NombreCompleto;
        return autor.ObtenerIniciales();
    }

    public string TextoHace()
    {
        TimeSpan paso = DateTime.Now - Fecha;

        if (paso.TotalMinutes < 1) return "Hace un momento";
        if (paso.TotalHours < 1) return Plural((int)paso.TotalMinutes, "minuto", "minutos");
        if (paso.TotalDays < 1) return Plural((int)paso.TotalHours, "hora", "horas");
        if (paso.TotalDays < 7) return Plural((int)paso.TotalDays, "día", "días");
        if (paso.TotalDays < 30) return Plural((int)paso.TotalDays / 7, "semana", "semanas");
        if (paso.TotalDays < 365) return Plural((int)paso.TotalDays / 30, "mes", "meses");
        return Plural((int)paso.TotalDays / 365, "año", "años");
    }

    public object ParaJson()
    {
        return new
        {
            id = Id,
            puntaje = Puntaje,
            comentario = Comentario,
            nombre = NombreCompleto,
            iniciales = ObtenerIniciales(),
            foto = Fotos.Avatar(FotoPerfil),
            fotoResena = Fotos.Avatar(Foto),
            hace = TextoHace()
        };
    }

    public static List<object> ListaParaJson(int idRestaurante, int desde)
    {
        List<object> resenas = new List<object>();
        foreach (ResenaComunidad resena in BD.ObtenerResenasDeRestaurante(idRestaurante, desde, null))
        {
            resenas.Add(resena.ParaJson());
        }
        return resenas;
    }

    private static string Plural(int cantidad, string singular, string plural)
    {
        return "Hace " + cantidad + " " + (cantidad == 1 ? singular : plural);
    }
}
