using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class GruposController : Controller
{
    // Mis Chats abre el chat más reciente. Si todavía no está en ningún grupo, muestra la lista vacía.
    // crear = 1 abre el modal Crear grupo (lo usa el ícono de "Chats" desde otras pantallas).
    // plan = 1 abre el modal Crear plan; lugar es el restaurante que viene ya cargado desde el Detalle.
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

    // 07 · Chat de grupo (privada). Solo lo ven los integrantes: el resto vuelve a Mis Chats.
    public IActionResult Chat(int id, int? lugar)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        if (!BD.EsIntegranteDeGrupo(usuario.Id, id)) return RedirectToAction("Index");

        // Se marca como leído antes de armar la lista, así el chat abierto aparece sin no leídos
        BD.MarcarComoLeido(usuario.Id, id);
        ViewBag.Chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        ViewBag.GrupoAbierto = id;
        ViewBag.Integrantes = Grupos.TextoIntegrantes(id, usuario.Id);
        // El primer lugar del modal Crear plan, si viene desde el Detalle de un restaurante
        ViewBag.LugarInicial = lugar == null ? null : BD.ObtenerLugar(lugar.Value);
        // Los chips de horario y el primer día que se puede elegir en el modal Crear plan
        ViewBag.Horarios = NuevoPlan.HorariosPosibles;
        ViewBag.Hoy = DateTime.Today.ToString("yyyy-MM-dd");
        return View(BD.ObtenerGrupo(id));
    }

    // ---------- Acciones que llama el JavaScript con fetch ----------

    // Los mensajes con Id mayor a "desde". El chat la llama al abrir (desde = 0) y cada 3 segundos.
    // También devuelve cómo están todas las encuestas del grupo, así los votos se actualizan solos.
    public IActionResult Mensajes(int id, int desde)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        if (!BD.EsIntegranteDeGrupo(usuario.Id, id)) return StatusCode(403);

        return Json(MensajeChat.NuevosDelGrupo(id, desde, usuario.Id));
    }

    // Recibe: { idGrupo: 3, contenido: "Hola" }
    [HttpPost]
    public IActionResult EnviarMensaje([FromBody] Mensajes datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.Enviar(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }

    // La lista "Chats" ya armada, para refrescarla sin recargar (hora, orden, no leídos y grupos nuevos)
    public IActionResult ListaChats(int abierto)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.GrupoAbierto = abierto;
        return PartialView("_Chats", BD.ObtenerChatsDeUsuario(usuario.Id));
    }

    // "Sumá amigos" del modal Crear grupo
    public IActionResult Amigos()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(Grupos.AmigosParaJson(usuario.Id));
    }

    // Recibe un formulario (por la foto): nombre, integrantes (uno por amigo marcado) y foto (opcional).
    // Lo que devuelve está explicado en Grupos.Crear.
    [HttpPost]
    public IActionResult CrearGrupo(string? nombre, List<int> integrantes, IFormFile? foto)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(Grupos.Crear(nombre, usuario.Id, integrantes, foto));
    }
}
