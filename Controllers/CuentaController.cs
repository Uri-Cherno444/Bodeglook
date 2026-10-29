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

        // El procedimiento InicioSesionUsuario busca por email y contraseña hasheada
        string email = datos.NombreUsuario.Trim();
        Usuarios? usuario = BD.IniciarSesion(email, HashearContraseña(datos.Contraseña, email));
        if (usuario == null)
            return Json(new { ok = false, campo = "aviso", mensaje = "El email o la contraseña no son correctos." });

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

        Usuarios nuevo = new Usuarios();
        nuevo.NombreCompleto = datos.NombreCompleto.Trim();
        nuevo.NombreUsuario = nombreUsuario;
        nuevo.Email = email;
        nuevo.Contraseña = HashearContraseña(datos.Contraseña, email);

        // La base pide un barrio y el formulario no lo pregunta: queda el primero de la lista (provisorio)
        int idBarrio = BD.ObtenerBarrios()[0].Id;
        int? error = BD.RegistrarUsuario(nuevo, idBarrio);
        if (error == 1)
            return Json(new { ok = false, campo = "usuario", mensaje = "Ese usuario ya existe. Probá con otro." });
        if (error == 2)
            return Json(new { ok = false, campo = "email", mensaje = "Ese email ya tiene una cuenta." });

        // Con la cuenta nueva queda con sesión y sigue al onboarding
        Usuarios? creado = BD.IniciarSesion(email, nuevo.Contraseña);
        if (creado == null)
            return Json(new { ok = false, campo = "aviso", mensaje = "No se pudo crear la cuenta. Probá de nuevo." });

        GuardarEnSesion(creado);
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
    // El hash lo calcula Contraseñas.Hashear (la misma sal por email que usa el registro de restaurantes)
    private static string HashearContraseña(string contraseña, string email)
    {
        return Contraseñas.Hashear(contraseña, email);
    }
}
