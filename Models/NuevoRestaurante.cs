namespace Bodeglook.Models;

// No es una tabla: lo que manda el formulario de 06 · Registro Restaurante.
// Plan es "gratis" o "pro".
public class NuevoRestaurante
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Contraseña { get; set; }
    public int? IdBarrio { get; set; }
    public int? IdCategoria { get; set; }
    public string? Plan { get; set; }
}
