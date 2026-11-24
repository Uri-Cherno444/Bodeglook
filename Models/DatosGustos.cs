namespace Bodeglook.Models;

public class DatosGustos
{
    public List<int> Restricciones { get; set; } = new List<int>();
    public List<int> Zonas { get; set; } = new List<int>();
    public string? Frecuencia { get; set; }
    public List<string> Companias { get; set; } = new List<string>();
}
