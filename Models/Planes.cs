namespace Bodeglook.Models;

public class Planes
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public DateTime? FechaCierreVotacion { get; set; }
    public DateTime? FechaSalida { get; set; }
    // false = En votación, true = Confirmado
    public bool Estado { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdRestauranteElegido { get; set; }
}
