namespace Bodeglook.Models;

public class Planes
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public DateTime? FechaCierreVotacion { get; set; }
    // Mientras se vota es solo el día (a las 00:00); al confirmar pasa a ser el día y el horario ganador
    public DateTime? FechaSalida { get; set; }
    // 0 = En votación, 1 = Confirmado, 2 = Cancelado
    public int Estado { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdRestauranteElegido { get; set; }
    // Quien creó el plan: es el único que lo confirma o lo cancela
    public int? IdUsuarioCreador { get; set; }
}
