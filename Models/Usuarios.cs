namespace Bodeglook.Models;

public class Usuarios
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? NombreUsuario { get; set; }
    public string? Email { get; set; }
    public string? Contraseña { get; set; }
    public string? FotoPerfil { get; set; }
    public string? Bio { get; set; }
    public DateTime? FechaRegistro { get; set; }

    // "Camila Fernández" -> "CF". Si tiene una sola palabra: "Cami" -> "CA" (como en el Figma)
    public string ObtenerIniciales()
    {
        string[] palabras = (Nombre ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (palabras.Length == 0) return "";
        if (palabras.Length == 1) return palabras[0].Substring(0, Math.Min(2, palabras[0].Length)).ToUpper();
        return (palabras[0].Substring(0, 1) + palabras[1].Substring(0, 1)).ToUpper();
    }

    // "Camila Fernández" -> "Camila"
    public string ObtenerPrimerNombre()
    {
        return (Nombre ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
    }
}
