namespace Bodeglook.Models;

// No es una tabla: junta los datos de Restaurantes, Categorias, Barrios
// y el promedio de Reseñas que muestra cada tarjeta de lugar.
public class TarjetaRestaurante
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Foto { get; set; }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public double? Puntaje { get; set; }
    public bool EsFavorito { get; set; }

    // Para 05 · Descubrir: chip "Recomendado" y pin en el mapa
    public bool EsPro { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}
