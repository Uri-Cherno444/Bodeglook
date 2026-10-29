using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class RestaurantesController : Controller
{
    // Reseñas que se muestran al entrar; "Ver las N" carga el resto
    private const int ResenasIniciales = 3;

    // 05 · Descubrir: 5 resultados por página y las últimas 3 búsquedas
    private const int ResultadosPorPagina = 5;
    private const int BusquedasRecientes = 3;

    // 04 · Detalle + Reseña (privada)
    public IActionResult Detalle(int id)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        DetalleRestaurante? detalle = BD.ObtenerDetalleRestaurante(id, usuario.Id);
        if (detalle == null) return NotFound();

        ViewBag.Resenas = BD.ObtenerResenasDeRestaurante(id, 0, ResenasIniciales);
        ViewBag.YaReseno = BD.YaReseno(usuario.Id, id);
        ViewBag.Visita = BD.ObtenerVisitaVerificada(usuario.Id, id);
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

    // La llama "Publicar reseña" del modal 04b con fetch. Recibe NuevaResena.
    // Devuelve { ok, mensaje } si no se puede guardar, o { ok, publicada, ... } si se guardó.
    // La reseña se publica 24 horas después de dejarla.
    [HttpPost]
    public IActionResult DejarResena([FromBody] NuevaResena datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();
        if (!BD.ExisteRestaurante(datos.IdRestaurante)) return Json(new { ok = false, mensaje = "Ese lugar no existe." });

        int[] puntajes = { datos.Comida, datos.Ambiente, datos.Atencion, datos.Precio };
        if (puntajes.Any(p => p < 1 || p > 5))
            return Json(new { ok = false, mensaje = "Elegí un puntaje del 1 al 5 en Comida, Ambiente, Atención y Precio y valor." });

        // La base no tiene dónde marcar los canjes: sin esa marca sumarían al puntaje, así que no se publican
        if (datos.EsCanje)
            return Json(new { ok = false, mensaje = "Por ahora no se pueden publicar reseñas de invitaciones o canjes." });

        Usuarios usuario = UsuarioDeSesion();
        if (BD.YaReseno(usuario.Id, datos.IdRestaurante))
            return Json(new { ok = false, mensaje = "Ya dejaste una reseña de este lugar." });

        // En la tabla entra un entero: se guarda el promedio de los cuatro, redondeado
        Reseñas nueva = new Reseñas();
        nueva.Puntaje = (int)Math.Round(puntajes.Average(), MidpointRounding.AwayFromZero);
        nueva.Comentario = string.IsNullOrWhiteSpace(datos.Comentario) ? null : datos.Comentario.Trim();
        nueva.IdUsuario = usuario.Id;
        nueva.IdRestaurante = datos.IdRestaurante;

        // El procedimiento CrearResena guarda la reseña con la fecha de ahora, así que
        // se publica 24 horas después y no se muestra enseguida
        int error = BD.GuardarResena(nueva);
        if (error == 1) return Json(new { ok = false, mensaje = "Elegí un puntaje del 1 al 5." });
        if (error == 2) return Json(new { ok = false, mensaje = "Ya dejaste una reseña de este lugar." });

        return Json(new { ok = true, publicada = false });
    }

    // 05 · Descubrir Restaurantes (privada). q viene del buscador del header o de un link.
    // Los resultados los pide descubrir.js con fetch a /Restaurantes/Resultados.
    public IActionResult Descubrir(string? q)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        ViewBag.Texto = (q ?? "").Trim();
        ViewBag.Recientes = BD.ObtenerBusquedasRecientes(UsuarioDeSesion().Id, BusquedasRecientes);
        return View();
    }

    // La llama descubrir.js con fetch al buscar y al cambiar de página.
    // Devuelve { pagina, totalPaginas, resultados, recientes }. Cada búsqueda nueva (pagina 1 con texto) se guarda.
    public IActionResult Resultados(string? texto, int pagina)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        texto = (texto ?? "").Trim();
        if (texto.Length > 100) texto = texto.Substring(0, 100);

        // "Solo lo apto para mí" del onboarding (queda en la sesión)
        bool soloApto = HttpContext.Session.GetString("soloApto") == "1";

        int total = BD.ContarRestaurantes(texto, usuario.Id, soloApto);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)ResultadosPorPagina));
        if (pagina < 1) pagina = 1;
        if (pagina > totalPaginas) pagina = totalPaginas;

        if (pagina == 1 && texto != "") BD.GuardarBusqueda(usuario.Id, texto);

        List<object> resultados = new List<object>();
        foreach (TarjetaRestaurante lugar in BD.BuscarRestaurantes(texto, usuario.Id, soloApto, (pagina - 1) * ResultadosPorPagina, ResultadosPorPagina))
        {
            resultados.Add(new
            {
                id = lugar.Id,
                nombre = lugar.Nombre,
                foto = lugar.Foto,
                categoria = lugar.Categoria,
                barrio = lugar.Barrio,
                puntaje = lugar.Puntaje == null ? null : lugar.Puntaje.Value.ToString("0.0", CultureInfo.InvariantCulture),
                esFavorito = lugar.EsFavorito,
                esPro = lugar.EsPro,
                latitud = lugar.Latitud,
                longitud = lugar.Longitud,
                distanciaKm = lugar.DistanciaKm == null ? (double?)null : Math.Round(lugar.DistanciaKm.Value, 1)
            });
        }

        return Json(new
        {
            pagina,
            totalPaginas,
            resultados,
            recientes = BD.ObtenerBusquedasRecientes(usuario.Id, BusquedasRecientes)
        });
    }

    // 06 · Registro Restaurante (pública). Llegan desde la Landing y desde Iniciar sesión.
    // plan = "pro" viene de "Pasarme a Pro" de la Landing y deja elegida la tarjeta Pro; con cualquier otro valor queda Gratis.
    public IActionResult Registro(string? plan)
    {
        ViewBag.Barrios = BD.ObtenerBarrios();
        ViewBag.Categorias = BD.ObtenerCategorias();
        ViewBag.Plan = plan == "pro" ? "pro" : "gratis";
        return View();
    }

    // La llama "Crear cuenta" de 06 con fetch. Recibe NuevoRestaurante y devuelve { ok, campo, mensaje }
    // (campo = "nombre", "email", "contrasena", "barrio", "categoria" o "aviso") o { ok, destino }.
    // Si elige Pro solo se guarda el plan: todavía no hay cobro ni pantalla de pago.
    [HttpPost]
    public IActionResult RegistrarRestaurante([FromBody] NuevoRestaurante datos)
    {
        string nombre = (datos.Nombre ?? "").Trim();
        string email = (datos.Email ?? "").Trim();
        string contraseña = datos.Contraseña ?? "";

        if (nombre == "")
            return Json(new { ok = false, campo = "nombre", mensaje = "Completá el nombre del restaurante." });
        if (nombre.Length > 100)
            return Json(new { ok = false, campo = "nombre", mensaje = "El nombre puede tener hasta 100 caracteres." });
        if (email == "" || !email.Contains('@') || email.Length > 100)
            return Json(new { ok = false, campo = "email", mensaje = "Ingresá un email válido." });
        if (contraseña == "")
            return Json(new { ok = false, campo = "contrasena", mensaje = "Elegí una contraseña." });
        if (contraseña.Length < 8)
            return Json(new { ok = false, campo = "contrasena", mensaje = "La contraseña tiene que tener al menos 8 caracteres." });
        if (datos.IdBarrio == null)
            return Json(new { ok = false, campo = "barrio", mensaje = "Elegí un barrio." });
        if (datos.IdCategoria == null)
            return Json(new { ok = false, campo = "categoria", mensaje = "Elegí una categoría." });
        if (datos.Plan != "gratis" && datos.Plan != "pro")
            return Json(new { ok = false, campo = "aviso", mensaje = "Elegí un plan." });

        int error = BD.RegistrarRestaurante(nombre, email, Contraseñas.Hashear(contraseña, email),
            datos.IdBarrio.Value, datos.IdCategoria.Value, datos.Plan == "pro");
        if (error == 1) return Json(new { ok = false, campo = "email", mensaje = "Ese email ya tiene una cuenta." });
        if (error == 2) return Json(new { ok = false, campo = "barrio", mensaje = "Elegí un barrio de la lista." });
        if (error == 3) return Json(new { ok = false, campo = "categoria", mensaje = "Elegí una categoría de la lista." });

        // No hay pantalla para el restaurante: vuelve a Iniciar sesión con un aviso
        return Json(new { ok = true, destino = "/Cuenta/IniciarSesion?restaurante=1" });
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
