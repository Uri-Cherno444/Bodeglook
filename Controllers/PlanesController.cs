using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class PlanesController : Controller
{
    public IActionResult MisPlanes()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.Chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        return View();
    }

    public IActionResult ListaPlanes(string filtro)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(BD.ObtenerPlanesDeUsuario(usuario.Id, TarjetaPlan.ConfirmadoSegunFiltro(filtro)));
    }

    public IActionResult NuevoPlan(int? idRestaurante)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        return RedirectToAction("Index", "Grupos", new { plan = 1, lugar = idRestaurante });
    }

    public IActionResult Votacion(int id)
    {
        return RedirectToAction("Detalle", new { id });
    }

    public IActionResult Detalle(int id)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        EncuestaPlan? encuesta = BD.ObtenerEncuesta(id, usuario.Id);
        if (encuesta == null || !BD.EsIntegranteDeGrupo(usuario.Id, encuesta.IdGrupo)) return RedirectToAction("Index", "Grupos");

        PrepararDetalle(encuesta, usuario.Id);
        return View(encuesta);
    }

    public IActionResult DetalleParcial(int id)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        EncuestaPlan? encuesta = BD.ObtenerEncuesta(id, usuario.Id);
        if (encuesta == null || !BD.EsIntegranteDeGrupo(usuario.Id, encuesta.IdGrupo)) return StatusCode(403);

        PrepararDetalle(encuesta, usuario.Id);
        return PartialView("_DetallePlan", encuesta);
    }

    private void PrepararDetalle(EncuestaPlan encuesta, int idUsuario)
    {
        ViewBag.IdUsuario = idUsuario;
        List<string> restricciones = encuesta.IdRestauranteElegido == null
            ? new List<string>()
            : BD.ObtenerRestriccionesDeRestaurante(encuesta.IdRestauranteElegido.Value);
        ViewBag.TextoLugarElegido = encuesta.TextoLugarElegido(restricciones);
    }

    public IActionResult BuscarLugares(int idGrupo, string? texto, List<int> excluir)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        if (!BD.EsIntegranteDeGrupo(usuario.Id, idGrupo)) return StatusCode(403);

        return Json(LugarBuscado.BuscarParaGrupo(idGrupo, texto, excluir));
    }

    [HttpPost]
    public IActionResult CrearPlan([FromBody] NuevoPlan datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.Crear(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }

    [HttpPost]
    public IActionResult Votar([FromBody] Votos datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.VotarLugar(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }

    [HttpPost]
    public IActionResult VotarHorario([FromBody] Votos datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.VotarHorario(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }

    [HttpPost]
    public IActionResult Confirmar([FromBody] DecisionPlan datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.Confirmar(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }

    [HttpPost]
    public IActionResult Cancelar([FromBody] DecisionPlan datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        object? respuesta = datos.Cancelar(usuario.Id);
        if (respuesta == null) return StatusCode(403);
        return Json(respuesta);
    }
}
