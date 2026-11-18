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

    // ---------- Fotos que sube la gente (reseñas, perfil y grupos) ----------

    // Las fotos subidas pesan hasta 5 MB
    public const long TamanoMaximo = 5 * 1024 * 1024;

    // Mira los primeros bytes del archivo: ".jpg", ".png" o ".webp". Si no es ninguna, "".
    // Así se sabe qué es de verdad, no lo que dice el nombre del archivo.
    public static string ExtensionDeImagen(IFormFile archivo)
    {
        byte[] inicio = new byte[12];
        int leidos;
        using (Stream lector = archivo.OpenReadStream())
        {
            leidos = lector.Read(inicio, 0, inicio.Length);
        }
        if (leidos < 12) return "";

        if (inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF) return ".jpg";
        if (inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47) return ".png";
        if (inicio[0] == 'R' && inicio[1] == 'I' && inicio[2] == 'F' && inicio[3] == 'F'
            && inicio[8] == 'W' && inicio[9] == 'E' && inicio[10] == 'B' && inicio[11] == 'P') return ".webp";
        return "";
    }

    // Guarda el archivo en wwwroot/img/{carpeta}/{nombre} y devuelve la ruta web ("/img/resenas/abc.jpg").
    // carpeta puede ser "" (queda directo en wwwroot/img).
    public static string GuardarSubida(IFormFile archivo, string carpeta, string nombre)
    {
        string carpetaFisica = Path.Combine(CarpetaWeb(), "img", carpeta);
        Directory.CreateDirectory(carpetaFisica);
        using (FileStream destino = new FileStream(Path.Combine(carpetaFisica, nombre), FileMode.Create))
        {
            archivo.CopyTo(destino);
        }
        return carpeta == "" ? "/img/" + nombre : "/img/" + carpeta + "/" + nombre;
    }

    // Borra una foto subida a partir de su ruta web, si existe
    public static void BorrarSubida(string? rutaWeb)
    {
        if (string.IsNullOrWhiteSpace(rutaWeb)) return;
        string archivo = Path.Combine(CarpetaWeb(), rutaWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(archivo)) File.Delete(archivo);
    }
}
