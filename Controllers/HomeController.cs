using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class HomeController : Controller
{
    // 01 · Index (pública)
    public IActionResult Index()
    {
        // Las tarjetas de "Lugares que recomienda la comunidad" (sin usuario: ninguna es favorita)
        ViewBag.Lugares = BD.ObtenerMejorPuntuados(4, 0);
        return View();
    }

    // 03 · Inicio (privada)
    // restriccion: el chip elegido (/Home/Inicio?restriccion=2). Sin él se muestra "Todos".
    public IActionResult Inicio(int? restriccion)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;

        // En el banner va solo el primer nombre ("Hola, Camila")
        ViewBag.PrimerNombre = usuario.ObtenerPrimerNombre();
        ViewBag.PlanesEnVotacion = BD.ContarPlanesEnVotacion(usuario.Id);
        ViewBag.Amigos = BD.ContarAmigos(usuario.Id);

        // Los chips salen de la tabla Restricciones
        List<Restricciones> restricciones = BD.ObtenerRestricciones();
        ViewBag.Restricciones = restricciones;
        CargarLugares(usuario.Id, Restricciones.ValidarElegida(restriccion, restricciones));
        return View();
    }

    // La llaman los chips de restricción de Inicio con fetch: devuelve "Recomendado para vos" y "Tus favoritos"
    // ya armados con ese filtro, así se cambian sin recargar la página.
    public IActionResult LugaresInicio(int? restriccion)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        CargarLugares(usuario.Id, Restricciones.ValidarElegida(restriccion, BD.ObtenerRestricciones()));
        return PartialView("_LugaresInicio");
    }

    // Las dos grillas de Inicio. "Solo lo apto para mí" se elige en el onboarding o en Editar perfil (Usuarios.SoloApto).
    private void CargarLugares(int idUsuario, int? restriccion)
    {
        ViewBag.RestriccionElegida = restriccion;
        ViewBag.Recomendados = TarjetaRestaurante.Recomendados(4, idUsuario, BD.ObtenerSoloApto(idUsuario), restriccion);
        ViewBag.Favoritos = TarjetaRestaurante.FavoritosConRestriccion(idUsuario, restriccion);
    }

    // La llama el corazón de cada tarjeta con fetch. Recibe { idRestaurante } y devuelve { ok, esFavorito }
    [HttpPost]
    public IActionResult AlternarFavorito([FromBody] Favoritos datos)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        return Json(datos.Alternar(usuario.Id));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
