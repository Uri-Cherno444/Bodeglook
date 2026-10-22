using System.Text.Json;
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
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        DetalleRestaurante? detalle = BD.ObtenerDetalleRestaurante(id, usuario.Id);
        if (detalle == null) return NotFound();

        ViewBag.Resenas = BD.ObtenerResenasDeRestaurante(id, 0, ResenasIniciales);
        return View(detalle);
    }

    // La llama "Ver las N" con fetch. Devuelve las reseñas que todavía no se muestran.
    public IActionResult Resenas(int id, int desde)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        List<object> resenas = new List<object>();
        foreach (ResenaComunidad resena in BD.ObtenerResenasDeRestaurante(id, desde, null))
        {
            resenas.Add(ResenaParaJson(resena));
        }
        return Json(resenas);
    }

    // La llama "Enviar reseña" con fetch. Recibe { idRestaurante, puntaje, comentario }.
    // Devuelve la reseña nueva y el promedio y la cantidad actualizados.
    [HttpPost]
    public IActionResult DejarResena([FromBody] Reseñas datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();
        if (datos.IdRestaurante == null || !BD.ExisteRestaurante(datos.IdRestaurante.Value))
            return Json(new { ok = false });
        if (datos.Puntaje == null || datos.Puntaje < 1 || datos.Puntaje > 5)
            return Json(new { ok = false });

        Usuarios usuario = UsuarioDeSesion();

        Reseñas nueva = new Reseñas();
        nueva.Puntaje = datos.Puntaje;
        nueva.Comentario = string.IsNullOrWhiteSpace(datos.Comentario) ? null : datos.Comentario.Trim();
        nueva.Fecha = DateTime.Now;
        nueva.IdUsuario = usuario.Id;
        nueva.IdRestaurante = datos.IdRestaurante;
        nueva.Id = BD.GuardarResena(nueva);

        // Para la tarjeta de la lista: el nombre sale de la sesión
        ResenaComunidad tarjeta = new ResenaComunidad();
        tarjeta.Id = nueva.Id;
        tarjeta.Puntaje = nueva.Puntaje.Value;
        tarjeta.Comentario = nueva.Comentario;
        tarjeta.Fecha = nueva.Fecha.Value;
        tarjeta.NombreCompleto = usuario.NombreCompleto;

        DetalleRestaurante puntaje = BD.ObtenerPuntajeRestaurante(datos.IdRestaurante.Value);
        return Json(new
        {
            ok = true,
            resena = ResenaParaJson(tarjeta),
            puntaje = puntaje.TextoPuntaje(),
            cantidad = puntaje.CantidadResenas,
            textoCantidad = puntaje.TextoCantidadResenas()
        });
    }

    // Descubrir todavía no está hecho
    public IActionResult Descubrir()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewData["Title"] = "Descubrir";
        ViewData["Pestaña"] = "Descubrir";
        return View("NoDesarrollado");
    }

    // Lo que necesita el JavaScript para armar una tarjeta de reseña
    private static object ResenaParaJson(ResenaComunidad resena)
    {
        return new
        {
            id = resena.Id,
            puntaje = resena.Puntaje,
            comentario = resena.Comentario,
            nombre = resena.NombreCompleto,
            iniciales = resena.ObtenerIniciales(),
            hace = resena.TextoHace()
        };
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
