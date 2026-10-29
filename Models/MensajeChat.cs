using System.Globalization;

namespace Bodeglook.Models;

// No es una tabla: un mensaje del chat con el nombre de quien lo escribió.
public class MensajeChat
{
    public int Id { get; set; }
    public string? Contenido { get; set; }
    public DateTime FechaHora { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreCompleto { get; set; }
    // Si no es null, el mensaje es la encuesta de ese plan
    public int? IdPlan { get; set; }

    // Separador por día: "Hoy", "Ayer" o "Lun 26/10"
    public string TextoDia()
    {
        if (FechaHora.Date == DateTime.Today) return "Hoy";
        if (FechaHora.Date == DateTime.Today.AddDays(-1)) return "Ayer";

        string dia = FechaHora.ToString("ddd", new CultureInfo("es-AR")).Replace(".", "");
        return char.ToUpper(dia[0]) + dia.Substring(1) + " " + FechaHora.ToString("dd/MM");
    }
}
