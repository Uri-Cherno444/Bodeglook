using Microsoft.AspNetCore.Mvc;

namespace Bodeglook.Controllers;

public class GruposController : Controller
{
    // Mis Chats todavía no está hecho
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Mis Chats";
        ViewData["Pestaña"] = "MisChats";
        return View("NoDesarrollado");
    }

    // El chat de un grupo todavía no está hecho
    public IActionResult Chat(int id)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Chat";
        ViewData["Pestaña"] = "MisChats";
        return View("NoDesarrollado");
    }
}
