namespace Bodeglook.Models;

// Los horarios para votar de un plan (hasta 3). La tabla va por lugar:
// cada horario se guarda una vez por cada lugar del plan, con el día y la hora.
public class PlanesHorarios
{
    public int Id { get; set; }
    public int? IdPlan { get; set; }
    public int? IdRestaurante { get; set; }
    public DateTime FechaHoraPropuesta { get; set; }
}
