using Microsoft.AspNetCore.Mvc;

namespace Bodeglook.Controllers;

public class GruposController : Controller
{
    // El chat de un grupo todavía no está hecho
    public IActionResult Chat(int id)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Chat";
        ViewData["Pestaña"] = "MisPlanes";
        return View("NoDesarrollado");
    }
}
