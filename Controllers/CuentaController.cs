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

    // ---------- Acciones que llama el JavaScript con fetch ----------
    // Todas devuelven { ok, campo, mensaje }:
    //   campo = "usuario", "contrasena", etc. si el error es de un campo
    //   campo = "aviso" si el error va en el aviso de arriba del formulario

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
        return Json(new { ok = true });
    }

    // Recibe: { nombre, nombreUsuario, email, contraseña }
    [HttpPost]
    public IActionResult Registrar([FromBody] Usuarios datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return Json(new { ok = false, campo = "nombre", mensaje = "Completá tu nombre." });
        if (string.IsNullOrWhiteSpace(datos.NombreUsuario))
            return Json(new { ok = false, campo = "usuario", mensaje = "Elegí un nombre de usuario." });
        if (string.IsNullOrWhiteSpace(datos.Email) || !datos.Email.Contains('@'))
            return Json(new { ok = false, campo = "email", mensaje = "Ingresá un email válido." });
        if (string.IsNullOrWhiteSpace(datos.Contraseña))
            return Json(new { ok = false, campo = "contrasena", mensaje = "Elegí una contraseña." });

        string nombreUsuario = datos.NombreUsuario.Trim();
        string email = datos.Email.Trim();

        if (BD.ExisteNombreUsuario(nombreUsuario))
            return Json(new { ok = false, campo = "usuario", mensaje = "Ese usuario ya existe. Probá con otro." });
        if (BD.ExisteEmail(email))
            return Json(new { ok = false, campo = "email", mensaje = "Ese email ya tiene una cuenta." });

        Usuarios nuevo = new Usuarios();
        nuevo.Nombre = datos.Nombre.Trim();
        nuevo.NombreUsuario = nombreUsuario;
        nuevo.Email = email;
        nuevo.Contraseña = HashearContraseña(datos.Contraseña);
        nuevo.FechaRegistro = DateTime.Today;
        nuevo.Id = BD.RegistrarUsuario(nuevo);

        GuardarEnSesion(nuevo);
        return Json(new { ok = true });
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

    // ---------- Contraseñas ----------
    // Se guarda "sal:hash" en Base64. La sal es aleatoria para cada usuario,
    // así dos personas con la misma contraseña no tienen el mismo hash.

    private static string HashearContraseña(string contraseña)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, 100000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(sal) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerificarContraseña(string contraseña, string guardada)
    {
        string[] partes = guardada.Split(':');
        if (partes.Length != 2) return false;

        byte[] sal = Convert.FromBase64String(partes[0]);
        byte[] hashGuardado = Convert.FromBase64String(partes[1]);
        byte[] hashNuevo = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hashNuevo, hashGuardado);
    }
}
