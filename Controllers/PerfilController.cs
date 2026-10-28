using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class PerfilController : Controller
{
    // 08 · Perfil y Amigos (privada)
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        ViewBag.Barrio = BD.ObtenerNombreBarrio(usuario.IdBarrio);
        ViewBag.CantidadResenas = BD.ContarResenasDeUsuario(usuario.Id);
        // "Gustos y restricciones": las del onboarding
        ViewBag.Restricciones = BD.ObtenerRestriccionesDeUsuario(usuario.Id);
        // "Restaurantes en mi perfil": sus favoritos
        ViewBag.Restaurantes = BD.ObtenerFavoritos(usuario.Id);
        // "Mis Planes": los de todos sus grupos
        ViewBag.Planes = BD.ObtenerPlanesDeUsuario(usuario.Id, null);
        return View(usuario);
    }

    // "Editar perfil" todavía no tiene pantalla en el Figma
    public IActionResult Editar()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Editar perfil";
        ViewData["Pestaña"] = "Perfil";
        return View("NoDesarrollado");
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
