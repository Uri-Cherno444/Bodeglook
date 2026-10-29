namespace Bodeglook.Models;

// Un voto por persona y por plan: el lugar y el horario van en la misma fila
public class Votos
{
    public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdPlan { get; set; }
    public int? IdRestaurante { get; set; }
    // La fila de PlanesHorarios del lugar votado. null si todavía no votó horario.
    public int? IdHorarioVotado { get; set; }
}
