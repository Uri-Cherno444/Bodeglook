using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Lugares = BD.ObtenerMejorPuntuados(4, 0);
        return View();
    }

    public IActionResult Inicio(int? restriccion)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;

        ViewBag.PrimerNombre = usuario.ObtenerPrimerNombre();
        ViewBag.PlanesEnVotacion = BD.ContarPlanesEnVotacion(usuario.Id);
        ViewBag.Amigos = BD.ContarAmigos(usuario.Id);

        List<Restricciones> restricciones = BD.ObtenerRestricciones();
        ViewBag.Restricciones = restricciones;
        CargarLugares(usuario.Id, Restricciones.ValidarElegida(restriccion, restricciones));
        return View();
    }

    public IActionResult LugaresInicio(int? restriccion)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios usuario = Sesion.Leer(HttpContext.Session)!;
        CargarLugares(usuario.Id, Restricciones.ValidarElegida(restriccion, BD.ObtenerRestricciones()));
        return PartialView("_LugaresInicio");
    }

    private void CargarLugares(int idUsuario, int? restriccion)
    {
        ViewBag.RestriccionElegida = restriccion;
        ViewBag.Recomendados = TarjetaRestaurante.Recomendados(4, idUsuario, BD.ObtenerSoloApto(idUsuario), restriccion);
        ViewBag.Favoritos = TarjetaRestaurante.FavoritosConRestriccion(idUsuario, restriccion);
    }

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
