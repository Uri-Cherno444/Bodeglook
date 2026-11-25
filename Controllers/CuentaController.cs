using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class CuentaController : Controller
{

    public IActionResult IniciarSesion()
    {
        return View();
    }

    public IActionResult Registro()
    {
        return View();
    }

    public IActionResult Gustos()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.Restricciones = BD.ObtenerRestricciones();
        ViewBag.Barrios = BD.ObtenerBarrios();
        ViewBag.Marcadas = BD.ObtenerIdsRestriccionesDeUsuario(usuario.Id);
        ViewBag.ZonasMarcadas = BD.ObtenerZonasDeUsuario(usuario.Id);
        ViewBag.CompaniasMarcadas = BD.ObtenerCompaniasDeUsuario(usuario.Id);
        ViewBag.FrecuenciaMarcada = BD.ObtenerDatosEdicion(usuario.Id)?.Frecuencia;
        return View();
    }

    public IActionResult FiltroDietario()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        List<string> nombres = BD.ObtenerNombresRestriccionesDeUsuario(usuario.Id);
        if (nombres.Count == 0) return RedirectToAction("Inicio", "Home");

        ViewBag.TextoRestricciones = Restricciones.UnirConY(nombres);
        ViewBag.SoloApto = BD.ObtenerDatosEdicion(usuario.Id)?.SoloApto != false;
        return View();
    }

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

    [HttpPost]
    public IActionResult GuardarGustos([FromBody] DatosGustos datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(Models.Gustos.Guardar(usuario.Id, datos));
    }

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
