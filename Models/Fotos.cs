namespace Bodeglook.Models;

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

    private static bool Existe(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta)) return false;
        string archivo = Path.Combine(CarpetaWeb(), ruta.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(archivo);
    }

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

    public static string Portada(int idRestaurante, string? foto)
    {
        if (Existe(foto)) return foto!;

        string? portada = ImagenesDeCarpeta(idRestaurante)
            .FirstOrDefault(ruta => Path.GetFileNameWithoutExtension(ruta) == "portada");
        return portada ?? PorDefecto;
    }

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

    public static string? Avatar(string? foto)
    {
        return Existe(foto) ? foto : null;
    }

    public const long TamanoMaximo = 5 * 1024 * 1024;

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

    public static void BorrarSubida(string? rutaWeb)
    {
        if (string.IsNullOrWhiteSpace(rutaWeb)) return;
        string archivo = Path.Combine(CarpetaWeb(), rutaWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(archivo)) File.Delete(archivo);
    }
}
