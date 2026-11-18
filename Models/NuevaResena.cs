namespace Bodeglook.Models;

// No es una tabla: lo que manda el modal "Dejar una reseña" (04b).
// Los cuatro puntajes van del 1 al 5. En la tabla Reseñas se guarda solo el promedio redondeado
// (no hay columnas para cada uno ni para marcar los canjes).
public class NuevaResena
{
    public int IdRestaurante { get; set; }
    public int Comida { get; set; }
    public int Ambiente { get; set; }
    public int Atencion { get; set; }
    public int Precio { get; set; }
    public string? Comentario { get; set; }
    public bool EsCanje { get; set; }
    // La foto es opcional (una sola): jpg, png o webp de hasta 5 MB
    public IFormFile? Foto { get; set; }

    // Los cuatro puntajes del modal 04b: nombre del campo, título y subtítulo
    public static readonly string[,] Dimensiones = {
        { "comida", "Comida", "Sabor, presentación, temperatura y porción" },
        { "ambiente", "Ambiente", "Ruido, luz, comodidad y limpieza" },
        { "atencion", "Atención", "Velocidad, amabilidad y manejo de restricciones dietarias" },
        { "precio", "Precio y valor", "¿Valió lo que costó?" }
    };

    // Revisa y guarda la reseña. Devuelve { ok, mensaje } si no se puede guardar
    // (campo = "foto" si el error es de la foto), o { ok, publicada } si se guardó.
    // La reseña se publica 24 horas después de dejarla.
    public object Guardar(int idUsuario)
    {
        if (!BD.ExisteRestaurante(IdRestaurante)) return new { ok = false, mensaje = "Ese lugar no existe." };

        int[] puntajes = { Comida, Ambiente, Atencion, Precio };
        if (puntajes.Any(p => p < 1 || p > 5))
            return new { ok = false, mensaje = "Elegí un puntaje del 1 al 5 en Comida, Ambiente, Atención y Precio y valor." };

        // La base no tiene dónde marcar los canjes: sin esa marca sumarían al puntaje, así que no se publican
        if (EsCanje)
            return new { ok = false, mensaje = "Por ahora no se pueden publicar reseñas de invitaciones o canjes." };

        if (BD.YaReseno(idUsuario, IdRestaurante))
            return new { ok = false, mensaje = "Ya dejaste una reseña de este lugar." };

        // Siempre se revisa acá, en el servidor, aunque alguien saltee el navegador.
        // No se dice cuál palabra fue: el navegador marca el campo y deja el texto para que lo corrija.
        if (BD.TieneLenguajeOfensivo(Comentario))
            return new { ok = false, campo = "comentario", mensaje = "Tu texto tiene palabras que no están permitidas en Bodeglook. Revisalo antes de publicar." };

        // La foto (opcional): se revisa el tamaño y el tipo real del archivo, no solo el nombre
        string extension = "";
        if (Foto != null)
        {
            if (Foto.Length == 0 || Foto.Length > Fotos.TamanoMaximo)
                return new { ok = false, campo = "foto", mensaje = "La foto no puede pesar más de 5 MB." };

            extension = Fotos.ExtensionDeImagen(Foto);
            if (extension == "")
                return new { ok = false, campo = "foto", mensaje = "La foto tiene que ser JPG, PNG o WEBP." };
        }

        // En la tabla entra un entero: se guarda el promedio de los cuatro, redondeado
        Reseñas nueva = new Reseñas();
        nueva.Puntaje = (int)Math.Round(puntajes.Average(), MidpointRounding.AwayFromZero);
        nueva.Comentario = string.IsNullOrWhiteSpace(Comentario) ? null : Comentario.Trim();
        nueva.IdUsuario = idUsuario;
        nueva.IdRestaurante = IdRestaurante;

        // La foto se guarda en wwwroot/img/resenas con un nombre generado (nunca el que trae el archivo)
        if (Foto != null)
            nueva.Foto = Fotos.GuardarSubida(Foto, "resenas", Guid.NewGuid().ToString("N") + extension);

        // El procedimiento CrearResena guarda la reseña con la fecha de ahora, así que
        // se publica 24 horas después y no se muestra enseguida (sus fotos tampoco)
        int error;
        try
        {
            error = BD.GuardarResena(nueva);
        }
        catch
        {
            // Si la base falla no queda una foto suelta
            Fotos.BorrarSubida(nueva.Foto);
            throw;
        }

        if (error != 0) Fotos.BorrarSubida(nueva.Foto);
        if (error == 1) return new { ok = false, mensaje = "Elegí un puntaje del 1 al 5." };
        if (error == 2) return new { ok = false, mensaje = "Ya dejaste una reseña de este lugar." };

        return new { ok = true, publicada = false };
    }
}
