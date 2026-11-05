namespace Bodeglook.Models;

// No es una tabla: junta los datos de Restaurantes, Categorias, Barrios
// y el promedio de Reseñas que muestra cada tarjeta de lugar.
public class TarjetaRestaurante
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    // Siempre tiene una ruta: la portada del restaurante o la imagen por defecto (ver Fotos)
    private string? _foto;
    public string? Foto
    {
        get { return Fotos.Portada(Id, _foto); }
        set { _foto = value; }
    }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public double? Puntaje { get; set; }
    public bool EsFavorito { get; set; }

    // Para 05 · Descubrir: chip "Recomendado" y pin en el mapa
    public bool EsPro { get; set; }
    // "Manual" o "Geoapify": los de Geoapify llevan los créditos de OpenStreetMap
    public string? Origen { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    // Distancia en km desde el usuario: su ubicación del navegador o, si no la dio, el centro del barrio de su perfil
    // (fórmula de Haversine). null si falta alguna coordenada.
    public double? DistanciaKm { get; set; }
}
