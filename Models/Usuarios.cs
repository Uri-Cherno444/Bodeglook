namespace Bodeglook.Models;

public class Usuarios
{
    public int Id { get; set; }
    public string? NombreCompleto { get; set; }
    public string? NombreUsuario { get; set; }
    public string? Email { get; set; }
    public string? Contraseña { get; set; }
    public string? FotoPerfil { get; set; }
    public string? Biografía { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public int? IdBarrio { get; set; }

    public string? Barrio { get; set; }

    public string? Frecuencia { get; set; }
    public bool? SoloApto { get; set; }

    public string ObtenerIniciales()
    {
        string[] palabras = (NombreCompleto ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (palabras.Length == 0) return "";
        if (palabras.Length == 1) return palabras[0].Substring(0, Math.Min(2, palabras[0].Length)).ToUpper();
        return (palabras[0].Substring(0, 1) + palabras[1].Substring(0, 1)).ToUpper();
    }

    public string ObtenerPrimerNombre()
    {
        return (NombreCompleto ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
    }

    public static object Ingresar(Usuarios datos, out Usuarios? logueado)
    {
        logueado = null;
        if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
            return new { ok = false, campo = "usuario", mensaje = "Completá tu email." };
        if (string.IsNullOrWhiteSpace(datos.Contraseña))
            return new { ok = false, campo = "contrasena", mensaje = "Completá tu contraseña." };

        string email = datos.NombreUsuario.Trim();
        logueado = BD.IniciarSesion(email, Contraseñas.Hashear(datos.Contraseña, email));
        if (logueado == null)
            return new { ok = false, campo = "aviso", mensaje = "El email o la contraseña no son correctos." };

        return new { ok = true, destino = "/Home/Inicio" };
    }

    public static object Registrar(Usuarios datos, out Usuarios? logueado)
    {
        logueado = null;
        if (string.IsNullOrWhiteSpace(datos.NombreCompleto))
            return new { ok = false, campo = "nombre", mensaje = "Completá tu nombre y apellido." };
        if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
            return new { ok = false, campo = "usuario", mensaje = "Elegí un nombre de usuario." };
        if (string.IsNullOrWhiteSpace(datos.Email) || !datos.Email.Contains('@'))
            return new { ok = false, campo = "email", mensaje = "Ingresá un email válido." };
        if (string.IsNullOrWhiteSpace(datos.Contraseña))
            return new { ok = false, campo = "contrasena", mensaje = "Elegí una contraseña." };
        if (datos.Contraseña.Length < 8)
            return new { ok = false, campo = "contrasena", mensaje = "La contraseña tiene que tener al menos 8 caracteres." };

        string email = datos.Email.Trim();

        Usuarios nuevo = new Usuarios();
        nuevo.NombreCompleto = datos.NombreCompleto.Trim();
        nuevo.NombreUsuario = datos.NombreUsuario.Trim();
        nuevo.Email = email;
        nuevo.Contraseña = Contraseñas.Hashear(datos.Contraseña, email);

        int idBarrio = BD.ObtenerBarrios()[0].Id;
        int? error = BD.RegistrarUsuario(nuevo, idBarrio);
        if (error == 1)
            return new { ok = false, campo = "usuario", mensaje = "Ese usuario ya existe. Probá con otro." };
        if (error == 2)
            return new { ok = false, campo = "email", mensaje = "Ese email ya tiene una cuenta." };

        logueado = BD.IniciarSesion(email, nuevo.Contraseña);
        if (logueado == null)
            return new { ok = false, campo = "aviso", mensaje = "No se pudo crear la cuenta. Probá de nuevo." };

        return new { ok = true, destino = "/Cuenta/Gustos" };
    }

    public static object GuardarPerfil(Usuarios sesion, string? nombreCompleto, string? nombreUsuario, int? idBarrio,
        List<int>? restricciones, List<int>? zonas, string? frecuencia, List<string>? companias,
        bool soloApto, IFormFile? foto)
    {
        nombreCompleto = (nombreCompleto ?? "").Trim();
        nombreUsuario = (nombreUsuario ?? "").Trim();

        if (nombreCompleto == "")
            return new { ok = false, campo = "nombre", mensaje = "Completá tu nombre y apellido." };
        if (nombreCompleto.Length > 100)
            return new { ok = false, campo = "nombre", mensaje = "Puede tener hasta 100 caracteres." };

        if (nombreUsuario == "")
            return new { ok = false, campo = "usuario", mensaje = "Elegí un nombre de usuario." };
        if (nombreUsuario.Length > 50)
            return new { ok = false, campo = "usuario", mensaje = "Puede tener hasta 50 caracteres." };
        if (nombreUsuario.Any(char.IsWhiteSpace))
            return new { ok = false, campo = "usuario", mensaje = "No puede tener espacios." };
        if (BD.ExisteNombreUsuario(nombreUsuario, sesion.Id))
            return new { ok = false, campo = "usuario", mensaje = "Ese usuario ya existe. Probá con otro." };

        Barrios? barrio = BD.ObtenerBarrios().FirstOrDefault(b => b.Id == idBarrio);
        if (barrio == null)
            return new { ok = false, campo = "ubicacion", mensaje = "Elegí tu ubicación." };

        string extension = "";
        if (foto != null)
        {
            if (foto.Length > Fotos.TamanoMaximo)
                return new { ok = false, campo = "aviso", mensaje = "La foto puede pesar hasta 5 MB." };
            extension = Fotos.ExtensionDeImagen(foto);
            if (extension == "")
                return new { ok = false, campo = "aviso", mensaje = "La foto tiene que ser JPG, PNG o WEBP." };
        }

        DatosGustos gustos = Gustos.Limpiar(new DatosGustos
        {
            Restricciones = restricciones ?? new List<int>(),
            Zonas = zonas ?? new List<int>(),
            Frecuencia = frecuencia,
            Companias = companias ?? new List<string>()
        });
        bool soloAptoFinal = soloApto && gustos.Restricciones.Count > 0;

        string? fotoAnterior = BD.ObtenerDatosEdicion(sesion.Id)?.FotoPerfil;
        string? fotoNueva = null;
        if (foto != null)
            fotoNueva = Fotos.GuardarSubida(foto, "usuarios", "usuario-" + sesion.Id + "-" + DateTime.UtcNow.Ticks + extension);

        try
        {
            BD.GuardarPerfil(sesion.Id, nombreCompleto, nombreUsuario, barrio.Id, fotoNueva, gustos, soloAptoFinal);
        }
        catch
        {
            Fotos.BorrarSubida(fotoNueva);
            throw;
        }

        if (fotoNueva != null && fotoAnterior != null && fotoAnterior.StartsWith("/img/usuarios/"))
            Fotos.BorrarSubida("/img/usuarios/" + Path.GetFileName(fotoAnterior));

        sesion.NombreCompleto = nombreCompleto;
        sesion.NombreUsuario = nombreUsuario;
        sesion.IdBarrio = barrio.Id;
        if (fotoNueva != null) sesion.FotoPerfil = fotoNueva;

        List<Restricciones> restriccionesGuardadas = BD.ObtenerRestriccionesDeUsuario(sesion.Id);
        return new
        {
            ok = true,
            perfil = new
            {
                nombre = nombreCompleto,
                usuario = nombreUsuario,
                barrio = barrio.Nombre,
                foto = fotoNueva,
                iniciales = sesion.ObtenerIniciales(),
                restricciones = restriccionesGuardadas.Select(r => r.Nombre)
            }
        };
    }
}
