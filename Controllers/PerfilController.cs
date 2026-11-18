using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class PerfilController : Controller
{
    // 08 · Perfil y Amigos (privada)
    public IActionResult Index()
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios sesion = Sesion.Leer(HttpContext.Session)!;
        // Se leen de la base (no de la sesión) para que la foto y los datos guardados siempre estén al día
        Usuarios? perfil = BD.ObtenerDatosEdicion(sesion.Id);
        if (perfil == null) return RedirectToAction("IniciarSesion", "Cuenta");

        List<Barrios> barrios = BD.ObtenerBarrios();
        List<Restricciones> restricciones = BD.ObtenerRestriccionesDeUsuario(sesion.Id);
        ViewBag.Barrio = barrios.FirstOrDefault(b => b.Id == perfil.IdBarrio)?.Nombre;
        ViewBag.CantidadResenas = BD.ContarResenasDeUsuario(sesion.Id);
        // "Gustos y restricciones": las del onboarding
        ViewBag.Restricciones = restricciones;
        // "Restaurantes en mi perfil": sus favoritos
        ViewBag.Restaurantes = BD.ObtenerFavoritos(sesion.Id);
        // "Mis Planes": los de todos sus grupos
        ViewBag.Planes = BD.ObtenerPlanesDeUsuario(sesion.Id, null);
        ViewBag.PlanesIniciales = TarjetaPlan.CantidadEnPerfil;

        // 08c · Editar perfil (modal): las opciones y lo que el usuario ya eligió
        ViewBag.Barrios = barrios;
        ViewBag.TodasRestricciones = BD.ObtenerRestricciones();
        ViewBag.RestriccionesMarcadas = restricciones.Select(r => r.Id).ToList();
        ViewBag.ZonasMarcadas = BD.ObtenerZonasDeUsuario(sesion.Id);
        ViewBag.CompaniasMarcadas = BD.ObtenerCompaniasDeUsuario(sesion.Id);
        // "Solo lo apto" solo tiene sentido si hay restricciones marcadas
        ViewBag.SoloApto = perfil.SoloApto == true && restricciones.Count > 0;
        return View(perfil);
    }

    // La llama "Guardar cambios" del modal Editar perfil con fetch. Recibe un formulario (por la foto):
    // nombreCompleto, nombreUsuario, idBarrio, restricciones (una por Id marcado), zonas, frecuencia,
    // companias, soloApto y foto (solo si la cambió). Lo que devuelve está explicado en Usuarios.GuardarPerfil.
    [HttpPost]
    public IActionResult GuardarPerfil(string? nombreCompleto, string? nombreUsuario, int? idBarrio,
        List<int>? restricciones, List<int>? zonas, string? frecuencia, List<string>? companias,
        bool soloApto, IFormFile? foto)
    {
        if (!Sesion.HayUsuario(HttpContext.Session)) return Unauthorized();

        Usuarios sesion = Sesion.Leer(HttpContext.Session)!;
        object respuesta = Usuarios.GuardarPerfil(sesion, nombreCompleto, nombreUsuario, idBarrio,
            restricciones, zonas, frecuencia, companias, soloApto, foto);
        // Si se guardó, sesion ya tiene los datos nuevos (si hubo un error quedó igual)
        Sesion.Guardar(HttpContext.Session, sesion);
        return Json(respuesta);
    }
}
