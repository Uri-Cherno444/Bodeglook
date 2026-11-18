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

    // Nombre del barrio: solo lo completa el procedimiento ObtenerPerfilUsuario
    public string? Barrio { get; set; }

    // Gustos del onboarding: con qué frecuencia sale y si quiere ver solo lo apto para sus restricciones
    // (null = todavía no eligió). Solo los completa BD.ObtenerDatosEdicion.
    public string? Frecuencia { get; set; }
    public bool? SoloApto { get; set; }

    // "Camila Fernández" -> "CF". Si tiene una sola palabra: "Cami" -> "CA" (como en el Figma)
    public string ObtenerIniciales()
    {
        string[] palabras = (NombreCompleto ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (palabras.Length == 0) return "";
        if (palabras.Length == 1) return palabras[0].Substring(0, Math.Min(2, palabras[0].Length)).ToUpper();
        return (palabras[0].Substring(0, 1) + palabras[1].Substring(0, 1)).ToUpper();
    }

    // "Camila Fernández" -> "Camila"
    public string ObtenerPrimerNombre()
    {
        return (NombreCompleto ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
    }

    // ---------- Cuenta (02 · Iniciar sesión y Registro) ----------
    // Devuelven { ok, campo, mensaje } si hay un error:
    //   campo = "usuario", "contrasena", etc. si el error es de un campo
    //   campo = "aviso" si el error va en el aviso de arriba del formulario
    // o { ok, destino } con la página a la que sigue. En "logueado" queda el usuario para guardar en la sesión (null si hubo error).

    // Recibe { nombreUsuario: "email", contraseña: "..." }. El procedimiento InicioSesionUsuario busca por email y contraseña hasheada.
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

    // Recibe { nombreCompleto, nombreUsuario, email, contraseña }. Con la cuenta nueva queda con sesión y sigue al onboarding.
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

        // La base pide un barrio y el formulario no lo pregunta: queda el primero de la lista (provisorio)
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

    // ---------- 08c · Editar perfil ----------

    // "Guardar cambios" del modal Editar perfil. sesion es el usuario logueado: si se guarda bien,
    // se le cargan los datos nuevos (el controller lo vuelve a guardar en la sesión).
    // Devuelve { ok, campo, mensaje } si hay un error ("campo" = nombre, usuario, ubicacion o aviso)
    // o { ok, perfil } con lo que hay que mostrar en la tarjeta de perfil.
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

        // La foto: se mira qué es de verdad (no lo que dice el nombre del archivo)
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
        // Sin restricciones no hay nada que filtrar: queda "Ver todo"
        bool soloAptoFinal = soloApto && gustos.Restricciones.Count > 0;

        string? fotoAnterior = BD.ObtenerDatosEdicion(sesion.Id)?.FotoPerfil;
        string? fotoNueva = null;
        // La foto se guarda en wwwroot/img/usuarios con el Id y la hora (así el navegador no muestra la vieja)
        if (foto != null)
            fotoNueva = Fotos.GuardarSubida(foto, "usuarios", "usuario-" + sesion.Id + "-" + DateTime.UtcNow.Ticks + extension);

        try
        {
            BD.GuardarPerfil(sesion.Id, nombreCompleto, nombreUsuario, barrio.Id, fotoNueva, gustos, soloAptoFinal);
        }
        catch
        {
            // Si la base falla no queda una foto suelta
            Fotos.BorrarSubida(fotoNueva);
            throw;
        }

        // La foto vieja ya no se usa: se borra (solo si es una de las subidas, no una foto de ejemplo)
        if (fotoNueva != null && fotoAnterior != null && fotoAnterior.StartsWith("/img/usuarios/"))
            Fotos.BorrarSubida("/img/usuarios/" + Path.GetFileName(fotoAnterior));

        // El header y el menú del avatar leen la sesión: se actualiza con lo nuevo
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
