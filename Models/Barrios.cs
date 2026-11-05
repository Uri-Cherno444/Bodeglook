namespace Bodeglook.Models;

public class Barrios
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    // Centro del barrio. Solo lo completan BD.ObtenerBarriosConCoordenadas y BD.ObtenerBarrioDeUsuario.
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}
