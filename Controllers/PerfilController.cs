using Microsoft.AspNetCore.Mvc;

namespace Bodeglook.Controllers;

public class PerfilController : Controller
{
    // 05 · Perfil y Amigos todavía no está hecho
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Perfil";
        ViewData["Pestaña"] = "Perfil";
        return View("NoDesarrollado");
    }
}
