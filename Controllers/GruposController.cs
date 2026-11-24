using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class GruposController : Controller
{
    public IActionResult Index(int? crear, int? plan, int? lugar)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        List<ResumenChat> chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        if (chats.Count > 0) return RedirectToAction("Chat", new { id = chats[0].IdGrupo, crear, plan, lugar });

        ViewBag.Chats = chats;
        ViewBag.GrupoAbierto = 0;
        return View("Chat", null);
    }

    public IActionResult Chat(int id, int? lugar)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        if (!BD.EsIntegranteDeGrupo(usuario.Id, id)) return RedirectToAction("Index");

        BD.MarcarComoLeido(usuario.Id, id);
        ViewBag.Chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        ViewBag.GrupoAbierto = id;
        ViewBag.Integrantes = Grupos.TextoIntegrantes(id, usuario.Id);
        ViewBag.LugarInicial = lugar == null ? null : BD.ObtenerLugar(lugar.Value);
        ViewBag.Horarios = NuevoPlan.HorariosPosibles;
        ViewBag.Hoy = DateTime.Today.ToString("yyyy-MM-dd");
        return View(BD.ObtenerGrupo(id));
    }

    public IActionResult Mensajes(int id, int desde)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        if (!BD.EsIntegranteDeGrupo(usuario.Id, id)) return StatusCode(403);

        return Json(MensajeChat.NuevosDelGrupo(id, desde, usuario.Id));
    }

    [HttpPost]
    public IActionResult EnviarMensaje([FromBody] Mensajes datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.Enviar(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }

    public IActionResult ListaChats(int abierto)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.GrupoAbierto = abierto;
        return PartialView("_Chats", BD.ObtenerChatsDeUsuario(usuario.Id));
    }

    public IActionResult Amigos()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(Grupos.AmigosParaJson(usuario.Id));
    }

    [HttpPost]
    public IActionResult CrearGrupo(string? nombre, List<int> integrantes, IFormFile? foto)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(Grupos.Crear(nombre, usuario.Id, integrantes, foto));
    }
}
