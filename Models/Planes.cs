namespace Bodeglook.Models;

public class Planes
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public DateTime? FechaSalida { get; set; }
    public DateTime? FechaCierreVotacion { get; set; }
    public string? Estado { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdRestauranteElegido { get; set; }
    public TimeSpan? HoraSalida { get; set; }
    public int? IdUsuarioCreador { get; set; }
}
