namespace Bodeglook.Models;

// No es una tabla: lo que manda el creador al confirmar ({ idPlan, idRestaurante, idHorario })
// o al cancelar ({ idPlan }) desde el Detalle del plan.
public class DecisionPlan
{
    public int IdPlan { get; set; }
    public int IdRestaurante { get; set; }
    public int IdHorario { get; set; }
}
