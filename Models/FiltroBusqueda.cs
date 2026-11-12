namespace Bodeglook.Models;

// No es una tabla: lo que se le pide a la base en 05 · Descubrir (el buscador, el barrio, la categoría y lo que se ve del mapa).
public class FiltroBusqueda
{
    public string Texto { get; set; } = "";
    public int IdUsuario { get; set; }
    // "Solo lo apto para mí": deja los que cumplen al menos una restricción del usuario
    public bool SoloApto { get; set; }
    public int? IdBarrio { get; set; }
    public int? IdCategoria { get; set; }
    // La zona que se ve en el mapa (null = no se limita por zona)
    public double? Norte { get; set; }
    public double? Sur { get; set; }
    public double? Este { get; set; }
    public double? Oeste { get; set; }
    // Cuántos lugares como máximo se muestran: los mejores de la zona (null = todos)
    public int? Limite { get; set; }

    public bool TieneZona
    {
        get { return Norte != null && Sur != null && Este != null && Oeste != null; }
    }
}
