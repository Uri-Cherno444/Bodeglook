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
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.Restricciones = BD.ObtenerRestricciones();
        ViewBag.Barrios = BD.ObtenerBarrios();
        // Si vuelve con "Atrás" desde el paso 2, quedan marcadas las que ya eligió
        ViewBag.Marcadas = BD.ObtenerRestriccionesDeUsuario(usuario.Id).Select(r => r.Id).ToList();
        ViewBag.ZonasMarcadas = BD.ObtenerZonasDeUsuario(usuario.Id);
        ViewBag.CompaniasMarcadas = BD.ObtenerCompaniasDeUsuario(usuario.Id);
        ViewBag.FrecuenciaMarcada = BD.ObtenerDatosEdicion(usuario.Id)?.Frecuencia;
        return View();
    }

    // 02d · Filtro dietario (paso 2 de 2). Si no marcó restricciones, no hay nada que filtrar.
    public IActionResult FiltroDietario()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        List<string> nombres = BD.ObtenerRestriccionesDeUsuario(usuario.Id).Select(r => r.Nombre!).ToList();
        if (nombres.Count == 0) return RedirectToAction("Inicio", "Home");

        ViewBag.TextoRestricciones = Restricciones.UnirConY(nombres);
        // Marcada la que eligió antes; si todavía no eligió, "Solo lo apto para mí" (como en el Figma)
        ViewBag.SoloApto = BD.ObtenerDatosEdicion(usuario.Id)?.SoloApto != false;
        return View();
    }

    // ---------- Acciones que llama el JavaScript con fetch ----------
    // Lo que devuelven está explicado en Usuarios.Ingresar y Usuarios.Registrar.

    [HttpPost]
    public IActionResult Ingresar([FromBody] Usuarios datos)
    {
        object respuesta = Usuarios.Ingresar(datos, out Usuarios? logueado);
        if (logueado != null) Sesion.Guardar(HttpContext.Session, logueado);
        return Json(respuesta);
    }

    [HttpPost]
    public IActionResult Registrar([FromBody] Usuarios datos)
    {
        object respuesta = Usuarios.Registrar(datos, out Usuarios? logueado);
        if (logueado != null) Sesion.Guardar(HttpContext.Session, logueado);
        return Json(respuesta);
    }

    // "Seguir" del paso 1. Recibe { restricciones: [1, 2], zonas: [3], frecuencia: "Cada tanto", companias: ["Amigos"] }.
    // "Ninguna" manda restricciones: [].
    [HttpPost]
    public IActionResult GuardarGustos([FromBody] DatosGustos datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(Models.Gustos.Guardar(usuario.Id, datos));
    }

    // "Entrar a Bodeglook" del paso 2. Recibe true (Solo lo apto para mí) o false (Ver todo).
    [HttpPost]
    public IActionResult GuardarFiltro([FromBody] bool soloApto)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        BD.GuardarSoloApto(usuario.Id, soloApto);
        return Json(new { ok = true, destino = "/Home/Inicio" });
    }

    public IActionResult CerrarSesion()
    {
        Sesion.Cerrar(HttpContext.Session);
        return RedirectToAction("Index", "Home");
    }
}
