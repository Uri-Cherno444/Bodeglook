namespace Bodeglook.Models;

public class PlanesHorarios
{
    public int Id { get; set; }
    public int? IdPlan { get; set; }
    public int? IdRestaurante { get; set; }
    public DateTime FechaHoraPropuesta { get; set; }
}
