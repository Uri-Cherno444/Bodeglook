namespace Bodeglook.Models;

// No es una tabla: un barrio y cuántos lugares de una búsqueda tiene.
// Lo usa 05 · Descubrir para "No encontramos sushi en Palermo. Hay 3 en Belgrano".
public class BarrioConResultados
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public int Cantidad { get; set; }
}
