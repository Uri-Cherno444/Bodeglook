namespace Bodeglook.Models;

public class Restaurantes
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? DireccionCalle { get; set; }
    public int? DireccionNumero { get; set; }
    public int? DireccionCodigoPostal { get; set; }
    public string? Descripcion { get; set; }
    public int? Telefono { get; set; }
    public DateTime? HorarioApertura { get; set; }
    public DateTime? HorarioCerrado { get; set; }
    public int? RangoPrecio { get; set; }
    public string? Foto { get; set; }
    public int? IdBarrio { get; set; }
    public int? IdCategoria { get; set; }
    public bool EsPro { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}
