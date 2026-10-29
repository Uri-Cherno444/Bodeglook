namespace Bodeglook.Models;

// No es una tabla: un horario propuesto en un plan, con sus votos.
// PlanesHorarios tiene una fila por lugar; IdHorario es la primera de las filas con esa hora.
public class OpcionHorario
{
    public int IdHorario { get; set; }
    public DateTime FechaHoraPropuesta { get; set; }
    public int Votos { get; set; }

    // "21:00"
    public string TextoHorario()
    {
        return FechaHoraPropuesta.ToString("HH:mm");
    }
}
