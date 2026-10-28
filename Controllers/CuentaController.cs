using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class CuentaController : Controller
{
    // ---------- Vistas (públicas) ----------

    public IActionResult IniciarSesion()
    {
        return View();
    }

    public IActionResult Registro()
    {
        return View();
    }

    // 02c · Onboarding · Gustos (paso 1 de 2)
    public IActionResult Gustos()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion");

        Usuarios usuario = UsuarioDeSesion();
        ViewBag.Restricciones = BD.ObtenerRestricciones();
        ViewBag.Barrios = BD.ObtenerBarrios();
        // Si vuelve con "Atrás" desde el paso 2, quedan marcadas las que ya eligió
        ViewBag.Marcadas = BD.ObtenerRestriccionesDeUsuario(usuario.Id).Select(r => r.Id).ToList();
        return View();
    }

    // 02d · Filtro dietario (paso 2 de 2). Si no marcó restricciones, no hay nada que filtrar.
    public IActionResult FiltroDietario()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion");

        Usuarios usuario = UsuarioDeSesion();
        List<string> nombres = BD.ObtenerRestriccionesDeUsuario(usuario.Id).Select(r => r.Nombre!).ToList();
        if (nombres.Count == 0) return RedirectToAction("Inicio", "Home");

        ViewBag.TextoRestricciones = UnirConY(nombres);
        // Marcada la que eligió antes; si todavía no eligió, "Solo lo apto para mí" (como en el Figma)
        ViewBag.SoloApto = HttpContext.Session.GetString("soloApto") != "0";
        return View();
    }

    // ---------- Acciones que llama el JavaScript con fetch ----------
    // Todas devuelven { ok, campo, mensaje }:
    //   campo = "usuario", "contrasena", etc. si el error es de un campo
    //   campo = "aviso" si el error va en el aviso de arriba del formulario
    // Si sale bien devuelven { ok, destino }: la página a la que sigue.

    // Recibe: { nombreUsuario: "usuario o email", contraseña: "..." }
    [HttpPost]
    public IActionResult Ingresar([FromBody] Usuarios datos)
    {
        if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
            return Json(new { ok = false, campo = "usuario", mensaje = "Completá tu usuario o email." });
        if (string.IsNullOrWhiteSpace(datos.Contraseña))
            return Json(new { ok = false, campo = "contrasena", mensaje = "Completá tu contraseña." });

        Usuarios? usuario = BD.ObtenerUsuarioPorUsuarioOEmail(datos.NombreUsuario.Trim());
        if (usuario == null)
            return Json(new { ok = false, campo = "aviso", mensaje = "No hay ninguna cuenta con ese usuario o email. Registrate para empezar." });

        if (!VerificarContraseña(datos.Contraseña, usuario.Contraseña!))
            return Json(new { ok = false, campo = "contrasena", mensaje = "La contraseña no es correcta." });

        GuardarEnSesion(usuario);
        return Json(new { ok = true, destino = "/Home/Inicio" });
    }

    // Recibe: { nombreCompleto, nombreUsuario, email, contraseña }
    [HttpPost]
    public IActionResult Registrar([FromBody] Usuarios datos)
    {
        if (string.IsNullOrWhiteSpace(datos.NombreCompleto))
            return Json(new { ok = false, campo = "nombre", mensaje = "Completá tu nombre y apellido." });
        if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
            return Json(new { ok = false, campo = "usuario", mensaje = "Elegí un nombre de usuario." });
        if (string.IsNullOrWhiteSpace(datos.Email) || !datos.Email.Contains('@'))
            return Json(new { ok = false, campo = "email", mensaje = "Ingresá un email válido." });
        if (string.IsNullOrWhiteSpace(datos.Contraseña))
            return Json(new { ok = false, campo = "contrasena", mensaje = "Elegí una contraseña." });
        if (datos.Contraseña.Length < 8)
            return Json(new { ok = false, campo = "contrasena", mensaje = "La contraseña tiene que tener al menos 8 caracteres." });

        string nombreUsuario = datos.NombreUsuario.Trim();
        string email = datos.Email.Trim();

        if (BD.ExisteNombreUsuario(nombreUsuario))
            return Json(new { ok = false, campo = "usuario", mensaje = "Ese usuario ya existe. Probá con otro." });
        if (BD.ExisteEmail(email))
            return Json(new { ok = false, campo = "email", mensaje = "Ese email ya tiene una cuenta." });

        Usuarios nuevo = new Usuarios();
        nuevo.NombreCompleto = datos.NombreCompleto.Trim();
        nuevo.NombreUsuario = nombreUsuario;
        nuevo.Email = email;
        nuevo.Contraseña = HashearContraseña(datos.Contraseña);
        nuevo.FechaRegistro = DateTime.Today;
        nuevo.Id = BD.RegistrarUsuario(nuevo);

        // Con la cuenta nueva queda con sesión y sigue al onboarding
        GuardarEnSesion(nuevo);
        return Json(new { ok = true, destino = "/Cuenta/Gustos" });
    }

    // "Seguir" del paso 1. Recibe los Id de las restricciones marcadas: [1, 2]. "Ninguna" manda [].
    // Zonas, frecuencia y con quién sale no se guardan: la base no tiene dónde.
    // Si no marcó restricciones se saltea el paso 2.
    [HttpPost]
    public IActionResult GuardarGustos([FromBody] List<int> restricciones)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        // Solo se guardan Id que existen en la tabla Restricciones
        List<int> existentes = BD.ObtenerRestricciones().Select(r => r.Id).ToList();
        List<int> validas = restricciones.Where(id => existentes.Contains(id)).Distinct().ToList();

        Usuarios usuario = UsuarioDeSesion();
        BD.GuardarRestriccionesDeUsuario(usuario.Id, validas);

        if (validas.Count == 0)
        {
            HttpContext.Session.Remove("soloApto");
            return Json(new { ok = true, destino = "/Home/Inicio" });
        }
        return Json(new { ok = true, destino = "/Cuenta/FiltroDietario" });
    }

    // "Entrar a Bodeglook" del paso 2. Recibe true (Solo lo apto para mí) o false (Ver todo).
    // Se guarda en la sesión porque la base no tiene columna para esto.
    [HttpPost]
    public IActionResult GuardarFiltro([FromBody] bool soloApto)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        HttpContext.Session.SetString("soloApto", soloApto ? "1" : "0");
        return Json(new { ok = true, destino = "/Home/Inicio" });
    }

    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // ---------- Sesión ----------

    // Guarda el usuario como JSON en la sesión, sin la contraseña
    private void GuardarEnSesion(Usuarios usuario)
    {
        usuario.Contraseña = null;
        HttpContext.Session.SetString("usuario", JsonSerializer.Serialize(usuario));
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }

    // ["Sin TACC"] -> "Sin TACC"; ["Sin TACC", "Vegano", "Kosher"] -> "Sin TACC, Vegano y Kosher"
    private static string UnirConY(List<string> nombres)
    {
        if (nombres.Count == 1) return nombres[0];
        return string.Join(", ", nombres.Take(nombres.Count - 1)) + " y " + nombres[nombres.Count - 1];
    }

    // ---------- Contraseñas ----------
    // Se guarda "sal:hash" en Base64. La sal es aleatoria para cada usuario,
    // así dos personas con la misma contraseña no tienen el mismo hash.
    // Sal de 12 bytes (16 caracteres) + ":" + hash de 24 bytes (32 caracteres) = 49 caracteres,
    // para que entre en Usuarios.Contraseña varchar(50).

    private static string HashearContraseña(string contraseña)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(12);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, 100000, HashAlgorithmName.SHA256, 24);
        return Convert.ToBase64String(sal) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerificarContraseña(string contraseña, string guardada)
    {
        string[] partes = guardada.Split(':');
        if (partes.Length != 2) return false;

        byte[] sal = Convert.FromBase64String(partes[0]);
        byte[] hashGuardado = Convert.FromBase64String(partes[1]);
        byte[] hashNuevo = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, 100000, HashAlgorithmName.SHA256, hashGuardado.Length);
        return CryptographicOperations.FixedTimeEquals(hashNuevo, hashGuardado);
    }
}
