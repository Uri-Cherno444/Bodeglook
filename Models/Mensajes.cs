namespace Bodeglook.Models;

public class Mensajes
{
    public int Id { get; set; }
    public string? Contenido { get; set; }
    public DateTime? FechaHora { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdPlan { get; set; }
}
