using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class PlanesController : Controller
{
    // 08 · Mis Planes
    public IActionResult MisPlanes()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        ViewBag.Chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        return View();
    }

    // La llaman las pestañas Todos / En votación / Confirmados con fetch.
    // filtro: "todos", "votacion" o "confirmados"
    public IActionResult ListaPlanes(string filtro)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        string? estado = null;
        if (filtro == "votacion") estado = "En votación";
        if (filtro == "confirmados") estado = "Confirmado";

        Usuarios usuario = UsuarioDeSesion();
        return Json(BD.ObtenerPlanesDeUsuario(usuario.Id, estado));
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
