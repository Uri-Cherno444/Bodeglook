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

        bool? confirmado = null;
        if (filtro == "votacion") confirmado = false;
        if (filtro == "confirmados") confirmado = true;

        Usuarios usuario = UsuarioDeSesion();
        return Json(BD.ObtenerPlanesDeUsuario(usuario.Id, confirmado));
    }

    // 06 · Crear Plan todavía no está hecho. idRestaurante es el lugar que viene del Detalle.
    public IActionResult NuevoPlan(int? idRestaurante)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Crear plan";
        ViewData["Pestaña"] = "MisChats";
        return View("NoDesarrollado");
    }

    // La votación de un plan todavía no está hecha
    public IActionResult Votacion(int id)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Votación";
        ViewData["Pestaña"] = "MisChats";
        return View("NoDesarrollado");
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
