namespace Bodeglook.Models;

// No es una tabla: lo que manda el modal "Crear plan" (07b).
// Dia viene como "2026-10-24" y Horarios como ["20:30", "21:00"] (los chips marcados).
// Lugares son los 3 campos (null si quedó vacío).
public class NuevoPlan
{
    public int IdGrupo { get; set; }
    public string? Titulo { get; set; }
    public string? Dia { get; set; }
    public List<string> Horarios { get; set; } = new List<string>();
    public List<int?> Lugares { get; set; } = new List<int?>();
}
