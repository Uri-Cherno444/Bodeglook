namespace Bodeglook.Models;

// No es una tabla: resuelve qué foto mostrar de un restaurante.
// Las fotos de cada restaurante están en wwwroot/img/restaurantes/{id}/:
//   portada.jpg (o .png)  -> la foto de portada
//   cualquier otra imagen -> la galería (en orden alfabético: galeria-1.jpg, galeria-2.jpg...)
// Si Restaurantes.Foto tiene una ruta y el archivo existe, esa es la portada.
// Si no hay ninguna foto, se usa la imagen por defecto (logo de Bodeglook sobre fondo crema).
public static class Fotos
{
    public const string PorDefecto = "/img/restaurante-sin-foto.svg";

    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png" };

    private static string CarpetaWeb()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    private static string CarpetaRestaurante(int idRestaurante)
    {
        return Path.Combine(CarpetaWeb(), "img", "restaurantes", idRestaurante.ToString());
    }

    // "/img/x.jpg" existe en wwwroot
    private static bool Existe(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta)) return false;
        string archivo = Path.Combine(CarpetaWeb(), ruta.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(archivo);
    }

    // Las imágenes de la carpeta del restaurante como rutas web, en orden alfabético
    private static List<string> ImagenesDeCarpeta(int idRestaurante)
    {
        string carpeta = CarpetaRestaurante(idRestaurante);
        if (!Directory.Exists(carpeta)) return new List<string>();

        return Directory.GetFiles(carpeta)
            .Where(archivo => Extensiones.Contains(Path.GetExtension(archivo).ToLower()))
            .Select(archivo => "/img/restaurantes/" + idRestaurante + "/" + Path.GetFileName(archivo))
            .OrderBy(ruta => ruta)
            .ToList();
    }

    // La foto de portada: Restaurantes.Foto, o portada.* de la carpeta, o la imagen por defecto
    public static string Portada(int idRestaurante, string? foto)
    {
        if (Existe(foto)) return foto!;

        string? portada = ImagenesDeCarpeta(idRestaurante)
            .FirstOrDefault(ruta => Path.GetFileNameWithoutExtension(ruta) == "portada");
        return portada ?? PorDefecto;
    }

    // La galería: la portada primero y después el resto de la carpeta. Vacía si el restaurante no tiene fotos.
    public static List<string> Galeria(int idRestaurante, string? foto)
    {
        string portada = Portada(idRestaurante, foto);
        List<string> galeria = new List<string>();
        if (portada != PorDefecto) galeria.Add(portada);

        foreach (string ruta in ImagenesDeCarpeta(idRestaurante))
        {
            if (!galeria.Contains(ruta)) galeria.Add(ruta);
        }
        return galeria;
    }

    // Foto de usuario o de grupo: la ruta si el archivo existe, si no null (se muestran las iniciales)
    public static string? Avatar(string? foto)
    {
        return Existe(foto) ? foto : null;
    }
}
