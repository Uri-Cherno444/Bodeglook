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

    // La base no guarda quién creó el grupo: es el primer integrante que se le sumó (ver BD.CrearGrupo)
    public static bool EsCreador(int idGrupo, int idUsuario)
    {
        return BD.ObtenerIdCreadorGrupo(idGrupo) == idUsuario;
    }

    public static object Cabecera(int idGrupo, int idUsuario)
    {
        Grupos? grupo = BD.ObtenerGrupo(idGrupo);
        return new
        {
            nombre = grupo?.Nombre,
            foto = Fotos.Avatar(grupo?.Foto),
            integrantes = TextoIntegrantes(idGrupo, idUsuario)
        };
    }

    // Devuelve null si el usuario no puede editar el grupo
    public static object? DatosParaEditar(int idGrupo, int idUsuario)
    {
        if (!EsCreador(idGrupo, idUsuario)) return null;

        Grupos grupo = BD.ObtenerGrupo(idGrupo)!;
        return new
        {
            nombre = grupo.Nombre,
            foto = Fotos.Avatar(grupo.Foto),
            integrantes = BD.ObtenerIntegrantesDeGrupo(idGrupo).Where(i => i.Id != idUsuario).Select(i => i.Id)
        };
    }

    // Las reglas del formulario, iguales al crear y al editar. Devuelve el mensaje de error o null.
    private static string? Validar(string nombre, List<int> integrantes, IFormFile? foto)
    {
        if (nombre == "") return "Poné el nombre del grupo.";
        if (nombre.Length > 50) return "El nombre del grupo puede tener hasta 50 caracteres.";
        if (integrantes.Count == 0) return "Sumá al menos un amigo.";

        if (foto != null)
        {
            string[] permitidas = { ".jpg", ".jpeg", ".png", ".webp" };
            if (!permitidas.Contains(Extension(foto)) || !foto.ContentType.StartsWith("image/"))
                return "La foto tiene que ser JPG, PNG o WEBP.";
            if (foto.Length > Fotos.TamanoMaximo)
                return "La foto puede pesar hasta 5 MB.";
        }
        return null;
    }

    private static string Extension(IFormFile foto)
    {
        return Path.GetExtension(foto.FileName).ToLower();
    }

    private static List<int> IntegrantesValidos(int idCreador, List<int> integrantes)
    {
        List<int> existentes = BD.ObtenerIdsOtrosUsuarios(idCreador);
        return integrantes.Where(id => existentes.Contains(id)).Distinct().ToList();
    }

    public static object Crear(string? nombre, int idCreador, List<int> integrantes, IFormFile? foto)
    {
        nombre = (nombre ?? "").Trim();
        List<int> validos = IntegrantesValidos(idCreador, integrantes);
        string? error = Validar(nombre, validos, foto);
        if (error != null) return new { ok = false, mensaje = error };

        int idGrupo = BD.CrearGrupo(nombre, idCreador, validos);

        if (foto != null)
            BD.ActualizarFotoGrupo(idGrupo, Fotos.GuardarSubida(foto, "", "grupo-" + idGrupo + Extension(foto)));

        return new { ok = true, destino = "/Grupos/Chat/" + idGrupo };
    }

    // Devuelve null si el usuario no puede editar el grupo
    public static object? Editar(int idGrupo, string? nombre, int idUsuario, List<int> integrantes, IFormFile? foto)
    {
        if (!EsCreador(idGrupo, idUsuario)) return null;

        nombre = (nombre ?? "").Trim();
        List<int> validos = IntegrantesValidos(idUsuario, integrantes);
        string? error = Validar(nombre, validos, foto);
        if (error != null) return new { ok = false, mensaje = error };

        // El creador no entra en ninguna de las dos listas: nunca se puede quitar
        List<int> actuales = BD.ObtenerIntegrantesDeGrupo(idGrupo).Where(i => i.Id != idUsuario).Select(i => i.Id).ToList();
        string? fotoAnterior = BD.ObtenerGrupo(idGrupo)!.Foto;

        // Nombre de archivo nuevo en cada cambio, para que el navegador no muestre la foto vieja
        string? fotoNueva = null;
        if (foto != null)
            fotoNueva = Fotos.GuardarSubida(foto, "", "grupo-" + idGrupo + "-" + DateTime.UtcNow.Ticks + Extension(foto));

        try
        {
            BD.ActualizarGrupo(idGrupo, nombre, fotoNueva, validos.Except(actuales).ToList(), actuales.Except(validos).ToList());
        }
        catch
        {
            Fotos.BorrarSubida(fotoNueva);
            throw;
        }

        if (fotoNueva != null && fotoAnterior != null && fotoAnterior.StartsWith("/img/grupo-"))
            Fotos.BorrarSubida("/img/" + Path.GetFileName(fotoAnterior));

        return new { ok = true };
    }
}
