using System.Diagnostics;
using System.Text.Json;
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
    public IActionResult Inicio()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;

        // En el banner va solo el primer nombre ("Hola, Camila")
        ViewBag.PrimerNombre = usuario.ObtenerPrimerNombre();
        ViewBag.Recomendados = BD.ObtenerMejorPuntuados(4, usuario.Id);
        ViewBag.Favoritos = BD.ObtenerFavoritos(usuario.Id);
        return View();
    }

    // La llama el corazón de cada tarjeta con fetch. Recibe { idRestaurante } y devuelve { ok, esFavorito }
    [HttpPost]
    public IActionResult AlternarFavorito([FromBody] Favoritos datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();
        if (datos.IdRestaurante == null || !BD.ExisteRestaurante(datos.IdRestaurante.Value))
            return Json(new { ok = false });

        Usuarios usuario = JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
        bool esFavorito = BD.AlternarFavorito(usuario.Id, datos.IdRestaurante.Value);
        return Json(new { ok = true, esFavorito });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
