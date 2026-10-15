namespace Bodeglook.Models;

public class Restaurantes
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Descripcion { get; set; }
    public string? Telefono { get; set; }
    public string? Horario { get; set; }
    public string? RangoPrecio { get; set; }
    public string? Foto { get; set; }
    public int? IdBarrio { get; set; }
    public int? IdCategoria { get; set; }
}
