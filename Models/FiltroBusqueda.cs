namespace Bodeglook.Models;

public class FiltroBusqueda
{
    public string Texto { get; set; } = "";
    public int IdUsuario { get; set; }
    public bool SoloApto { get; set; }
    public int? IdBarrio { get; set; }
    public int? IdCategoria { get; set; }
    public double? Norte { get; set; }
    public double? Sur { get; set; }
    public double? Este { get; set; }
    public double? Oeste { get; set; }
    public int? Limite { get; set; }

    public bool TieneZona
    {
        get { return Norte != null && Sur != null && Este != null && Oeste != null; }
    }
}
