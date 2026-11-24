namespace Bodeglook.Models;

public class OpcionHorario
{
    public int IdHorario { get; set; }
    public DateTime FechaHoraPropuesta { get; set; }
    public int Votos { get; set; }

    public string TextoHorario()
    {
        return FechaHoraPropuesta.ToString("HH:mm");
    }
}
