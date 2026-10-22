namespace Bodeglook.Models;

// No es una tabla: los datos que muestra cada tarjeta de plan en 08 · Mis Planes.
public class TarjetaPlan
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public string? Estado { get; set; }
    public int IdGrupo { get; set; }
    public int CantidadLugares { get; set; }
    public int? IdRestauranteElegido { get; set; }
    public string? LugarElegido { get; set; }
    public List<string> Iniciales { get; set; } = new List<string>();
}
