using System.Globalization;

namespace Bodeglook.Models;

public class ResumenChat
{
    public int IdGrupo { get; set; }
    public string? Nombre { get; set; }
    public string? Foto { get; set; }
    public DateTime? UltimaFecha { get; set; }
    public int NoLeidos { get; set; }
    public string Integrantes { get; set; } = "";

    public DateTime? FechaOrden { get; set; }

    public string TextoHora()
    {
        if (UltimaFecha == null) return "";

        DateTime fecha = UltimaFecha.Value;
        if (fecha.Date == DateTime.Today) return fecha.ToString("HH:mm");
        if (fecha.Date == DateTime.Today.AddDays(-1)) return "Ayer";
        if (fecha.Date > DateTime.Today.AddDays(-7))
        {
            string dia = fecha.ToString("ddd", new CultureInfo("es-AR")).Replace(".", "");
            return char.ToUpper(dia[0]) + dia.Substring(1);
        }
        return fecha.ToString("dd/MM");
    }
}
