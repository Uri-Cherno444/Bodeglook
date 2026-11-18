using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class RestaurantesController : Controller
{
    // Reseñas que se muestran al entrar; "Ver las N" carga el resto
    private const int ResenasIniciales = 3;

    // 04 · Detalle + Reseña (privada)
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

    // La llama "Ver las N" con fetch. Devuelve las reseñas que todavía no se muestran.
    public IActionResult Resenas(int id, int desde)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        return Json(ResenaComunidad.ListaParaJson(id, desde));
    }

    // La llama "Publicar reseña" del modal 04b con fetch. Recibe NuevaResena como formulario (por la foto).
    // Lo que devuelve está explicado en NuevaResena.Guardar.
    [HttpPost]
    public IActionResult DejarResena([FromForm] NuevaResena datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(datos.Guardar(usuario.Id));
    }

    // 05 · Descubrir Restaurantes (privada). q viene del buscador del header o de un link.
    // Los resultados los pide descubrir.js con fetch a /Restaurantes/Resultados.
    public IActionResult Descubrir(string? q)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        ViewBag.Texto = (q ?? "").Trim();
        ViewBag.Recientes = BD.ObtenerBusquedasRecientes(usuario.Id, BuscadorRestaurantes.BusquedasRecientes);
        // Las opciones del desplegable "Barrio" y los chips de categoría
        ViewBag.Barrios = BD.ObtenerBarrios();
        ViewBag.Categorias = BD.ObtenerCategorias();
        // Donde arranca el mapa si el navegador no da la ubicación
        ViewBag.CentroEsBarrio = BuscadorRestaurantes.CentroDelMapa(usuario.Id, out double latitud, out double longitud);
        ViewBag.CentroLatitud = latitud;
        ViewBag.CentroLongitud = longitud;
        return View();
    }

    // La llama descubrir.js con fetch al buscar, al cambiar de página, de barrio, de categoría, de orden o al mover el mapa.
    // Qué recibe y qué devuelve está explicado en BuscadorRestaurantes.Buscar.
    public async Task<IActionResult> Resultados(string? texto, int pagina, int? idBarrio, int? idCategoria, string? orden,
        double? latitud, double? longitud, double? norte, double? sur, double? este, double? oeste, int? zoom, bool pines = false)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(await BuscadorRestaurantes.Buscar(usuario.Id, texto, pagina, idBarrio, idCategoria, orden,
            latitud, longitud, norte, sur, este, oeste, zoom, pines));
    }

    // 06 · Registro Restaurante (pública). Llegan desde la Landing y desde Iniciar sesión.
    // plan = "pro" viene de "Pasarme a Pro" de la Landing y deja elegida la tarjeta Pro.
    public IActionResult Registro(string? plan)
    {
        ViewBag.Barrios = BD.ObtenerBarrios();
        ViewBag.Categorias = BD.ObtenerCategorias();
        ViewBag.Plan = NuevoRestaurante.PlanInicial(plan);
        return View();
    }

    // La llama "Crear cuenta" de 06 con fetch. Lo que devuelve está explicado en NuevoRestaurante.Registrar.
    [HttpPost]
    public IActionResult RegistrarRestaurante([FromBody] NuevoRestaurante datos)
    {
        return Json(datos.Registrar());
    }
}
