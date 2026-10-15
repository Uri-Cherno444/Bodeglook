namespace Bodeglook.Models;

public class Reseñas
{
    public int Id { get; set; }
    public int? Puntaje { get; set; }
    public string? Comentario { get; set; }
    public string? Foto { get; set; }
    public DateTime? Fecha { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdRestaurante { get; set; }
}
