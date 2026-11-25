namespace Bodeglook.Models;

public class Grupos
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Foto { get; set; }
    public DateTime? FechaCreacion { get; set; }

    public static string TextoIntegrantes(int idGrupo, int idUsuario)
    {
        List<string> nombres = BD.ObtenerIntegrantesDeGrupo(idGrupo)
            .Where(i => i.Id != idUsuario)
            .Select(i => i.ObtenerPrimerNombre())
            .ToList();
        if (nombres.Count == 0) return "Solo vos";
        return string.Join(", ", nombres) + " y vos";
    }

    public static object AmigosParaJson(int idUsuario)
    {
        List<Usuarios> amigos = BD.ObtenerOtrosUsuarios(idUsuario);
        return amigos.Select(a => new { id = a.Id, nombre = a.NombreCompleto, iniciales = a.ObtenerIniciales() });
    }

    public static object Crear(string? nombre, int idCreador, List<int> integrantes, IFormFile? foto)
    {
        nombre = (nombre ?? "").Trim();
        if (nombre == "") return new { ok = false, mensaje = "Poné el nombre del grupo." };
        if (nombre.Length > 50) return new { ok = false, mensaje = "El nombre del grupo puede tener hasta 50 caracteres." };

        List<int> existentes = BD.ObtenerIdsOtrosUsuarios(idCreador);
        List<int> validos = integrantes.Where(id => existentes.Contains(id)).Distinct().ToList();
        if (validos.Count == 0) return new { ok = false, mensaje = "Sumá al menos un amigo." };

        string extension = "";
        if (foto != null)
        {
            extension = Path.GetExtension(foto.FileName).ToLower();
            string[] permitidas = { ".jpg", ".jpeg", ".png", ".webp" };
            if (!permitidas.Contains(extension) || !foto.ContentType.StartsWith("image/"))
                return new { ok = false, mensaje = "La foto tiene que ser JPG, PNG o WEBP." };
            if (foto.Length > Fotos.TamanoMaximo)
                return new { ok = false, mensaje = "La foto puede pesar hasta 5 MB." };
        }

        int idGrupo = BD.CrearGrupo(nombre, idCreador, validos);

        if (foto != null)
            BD.ActualizarFotoGrupo(idGrupo, Fotos.GuardarSubida(foto, "", "grupo-" + idGrupo + extension));

        return new { ok = true, destino = "/Grupos/Chat/" + idGrupo };
    }
}
