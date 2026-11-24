namespace Bodeglook.Models;

public class NuevaResena
{
    public int IdRestaurante { get; set; }
    public int Comida { get; set; }
    public int Ambiente { get; set; }
    public int Atencion { get; set; }
    public int Precio { get; set; }
    public string? Comentario { get; set; }
    public bool EsCanje { get; set; }
    public IFormFile? Foto { get; set; }

    public static readonly string[,] Dimensiones = {
        { "comida", "Comida", "Sabor, presentación, temperatura y porción" },
        { "ambiente", "Ambiente", "Ruido, luz, comodidad y limpieza" },
        { "atencion", "Atención", "Velocidad, amabilidad y manejo de restricciones dietarias" },
        { "precio", "Precio y valor", "¿Valió lo que costó?" }
    };

    public object Guardar(int idUsuario)
    {
        if (!BD.ExisteRestaurante(IdRestaurante)) return new { ok = false, mensaje = "Ese lugar no existe." };

        int[] puntajes = { Comida, Ambiente, Atencion, Precio };
        if (puntajes.Any(p => p < 1 || p > 5))
            return new { ok = false, mensaje = "Elegí un puntaje del 1 al 5 en Comida, Ambiente, Atención y Precio y valor." };

        if (EsCanje)
            return new { ok = false, mensaje = "Por ahora no se pueden publicar reseñas de invitaciones o canjes." };

        if (BD.YaReseno(idUsuario, IdRestaurante))
            return new { ok = false, mensaje = "Ya dejaste una reseña de este lugar." };

        if (BD.TieneLenguajeOfensivo(Comentario))
            return new { ok = false, campo = "comentario", mensaje = "Tu texto tiene palabras que no están permitidas en Bodeglook. Revisalo antes de publicar." };

        string extension = "";
        if (Foto != null)
        {
            if (Foto.Length == 0 || Foto.Length > Fotos.TamanoMaximo)
                return new { ok = false, campo = "foto", mensaje = "La foto no puede pesar más de 5 MB." };

            extension = Fotos.ExtensionDeImagen(Foto);
            if (extension == "")
                return new { ok = false, campo = "foto", mensaje = "La foto tiene que ser JPG, PNG o WEBP." };
        }

        Reseñas nueva = new Reseñas();
        nueva.Puntaje = (int)Math.Round(puntajes.Average(), MidpointRounding.AwayFromZero);
        nueva.Comentario = string.IsNullOrWhiteSpace(Comentario) ? null : Comentario.Trim();
        nueva.IdUsuario = idUsuario;
        nueva.IdRestaurante = IdRestaurante;

        if (Foto != null)
            nueva.Foto = Fotos.GuardarSubida(Foto, "resenas", Guid.NewGuid().ToString("N") + extension);

        int error;
        try
        {
            error = BD.GuardarResena(nueva);
        }
        catch
        {
            Fotos.BorrarSubida(nueva.Foto);
            throw;
        }

        if (error != 0) Fotos.BorrarSubida(nueva.Foto);
        if (error == 1) return new { ok = false, mensaje = "Elegí un puntaje del 1 al 5." };
        if (error == 2) return new { ok = false, mensaje = "Ya dejaste una reseña de este lugar." };

        return new { ok = true, publicada = false };
    }
}
