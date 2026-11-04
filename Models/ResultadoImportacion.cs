namespace Bodeglook.Models;

// No es una tabla: lo que trajo la importación de un barrio.
public class ResultadoImportacion
{
    public string? Barrio { get; set; }
    // Cuántos lugares devolvió Geoapify
    public int Traidos { get; set; }
    public int Nuevos { get; set; }
    public int Actualizados { get; set; }
    // Sin nombre en OpenStreetMap: se saltean
    public int SinNombre { get; set; }
    // No encajan en ninguna de nuestras categorías: se saltean
    public int SinCategoria { get; set; }
    // Si Geoapify no respondió o se pasó el tope diario
    public string? Aviso { get; set; }
}
