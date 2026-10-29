namespace Bodeglook.Models;

// No es una tabla: cada reseña de "Reseñas de la comunidad" en 04 · Detalle + Reseña,
// con el nombre de quien la escribió.
public class ResenaComunidad
{
    public int Id { get; set; }
    public int Puntaje { get; set; }
    public string? Comentario { get; set; }
    public DateTime Fecha { get; set; }
    public string? NombreCompleto { get; set; }

    // Los devuelve el procedimiento ListarResenasRestaurante
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }

    // Las iniciales se calculan igual que en el resto de la app
    public string ObtenerIniciales()
    {
        Usuarios autor = new Usuarios();
        autor.NombreCompleto = NombreCompleto;
        return autor.ObtenerIniciales();
    }

    // "Hace 2 días", "Hace 1 semana", "Hace 3 semanas" (como en el Figma)
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

    private static string Plural(int cantidad, string singular, string plural)
    {
        return "Hace " + cantidad + " " + (cantidad == 1 ? singular : plural);
    }
}
