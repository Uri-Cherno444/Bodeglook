namespace Bodeglook.Models;

// No es una tabla: un resultado del buscador de lugares del modal Crear plan (07c).
public class LugarBuscado
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    // Cumple todas las restricciones de los integrantes del grupo
    public bool AptoParaTodos { get; set; }
    // Las restricciones que cumple el lugar: "Sin TACC", "Vegetariano"
    public List<string> Restricciones { get; set; } = new List<string>();

    public string BarrioCorto()
    {
        return (Barrio ?? "").Split(',')[0];
    }

    // Lo que queda escrito en el campo al elegirlo: "Bodega Los Alamos · Cañuelas"
    public string TextoCampo()
    {
        return BarrioCorto() == "" ? Nombre ?? "" : Nombre + " · " + BarrioCorto();
    }
}
