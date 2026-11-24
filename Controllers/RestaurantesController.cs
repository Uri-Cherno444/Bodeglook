using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class RestaurantesController : Controller
{
    private const int ResenasIniciales = 3;

    public IActionResult Detalle(int id)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        DetalleRestaurante? detalle = BD.ObtenerDetalleRestaurante(id, usuario.Id);
        if (detalle == null) return NotFound();

        ViewBag.Resenas = BD.ObtenerResenasDeRestaurante(id, 0, ResenasIniciales);
        ViewBag.YaReseno = BD.YaReseno(usuario.Id, id);
        ViewBag.Visita = BD.ObtenerVisitaVerificada(usuario.Id, id);
        ViewBag.Origen = BD.ObtenerOrigenRestaurante(id);
        ViewBag.Dimensiones = NuevaResena.Dimensiones;
        return View(detalle);
    }

    public IActionResult Resenas(int id, int desde)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        return Json(ResenaComunidad.ListaParaJson(id, desde));
    }

    [HttpPost]
    public IActionResult DejarResena([FromForm] NuevaResena datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(datos.Guardar(usuario.Id));
    }

    public IActionResult Descubrir(string? q)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.Texto = (q ?? "").Trim();
        ViewBag.Recientes = BD.ObtenerBusquedasRecientes(usuario.Id, BuscadorRestaurantes.BusquedasRecientes);
        ViewBag.Barrios = BD.ObtenerBarrios();
        ViewBag.Categorias = BD.ObtenerCategorias();
        ViewBag.CentroEsBarrio = BuscadorRestaurantes.CentroDelMapa(usuario.Id, out double latitud, out double longitud);
        ViewBag.CentroLatitud = latitud;
        ViewBag.CentroLongitud = longitud;
        return View();
    }

    public async Task<IActionResult> Resultados(string? texto, int pagina, int? idBarrio, int? idCategoria, string? orden,
        double? latitud, double? longitud, double? norte, double? sur, double? este, double? oeste, int? zoom, bool pines = false)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(await BuscadorRestaurantes.Buscar(usuario.Id, texto, pagina, idBarrio, idCategoria, orden,
            latitud, longitud, norte, sur, este, oeste, zoom, pines));
    }

    public IActionResult Registro(string? plan)
    {
        ViewBag.Barrios = BD.ObtenerBarrios();
        ViewBag.Categorias = BD.ObtenerCategorias();
        ViewBag.Plan = NuevoRestaurante.PlanInicial(plan);
        return View();
    }

    [HttpPost]
    public IActionResult RegistrarRestaurante([FromBody] NuevoRestaurante datos)
    {
        return Json(datos.Registrar());
    }
}
