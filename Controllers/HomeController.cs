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
        ViewBag.Lugares = BD.ObtenerMejorPuntuados(4, 0, false);
        return View();
    }

    // 03 · Inicio (privada)
    // restriccion: el chip elegido (/Home/Inicio?restriccion=2). Sin él se muestra "Todos".
    public IActionResult Inicio(int? restriccion)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;

        // En el banner va solo el primer nombre ("Hola, Camila")
        ViewBag.PrimerNombre = usuario.ObtenerPrimerNombre();
        ViewBag.PlanesEnVotacion = BD.ContarPlanesEnVotacion(usuario.Id);
        ViewBag.Amigos = BD.ContarAmigos(usuario.Id);

        // Los chips salen de la tabla Restricciones. Un id que no existe se toma como "Todos".
        List<Restricciones> restricciones = BD.ObtenerRestricciones();
        if (!restricciones.Any(r => r.Id == restriccion)) restriccion = null;
        ViewBag.Restricciones = restricciones;
        ViewBag.RestriccionElegida = restriccion;

        // "Solo lo apto para mí": se elige en el onboarding o en Editar perfil y queda en Usuarios.SoloApto
        bool soloApto = BD.ObtenerSoloApto(usuario.Id);
        ViewBag.Recomendados = BD.ObtenerMejorPuntuados(4, usuario.Id, soloApto, restriccion);

        List<TarjetaRestaurante> favoritos = BD.ObtenerFavoritos(usuario.Id);
        if (restriccion != null)
        {
            List<int> conRestriccion = BD.ObtenerRestaurantesConRestriccion(restriccion.Value);
            favoritos = favoritos.Where(f => conRestriccion.Contains(f.Id)).ToList();
        }
        ViewBag.Favoritos = favoritos;
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
