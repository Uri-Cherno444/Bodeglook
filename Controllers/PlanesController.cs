using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class PlanesController : Controller
{
    // 08 · Mis Planes
    public IActionResult MisPlanes()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        ViewBag.Chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        return View();
    }

    // La llaman las pestañas Todos / En votación / Confirmados con fetch.
    // filtro: "todos", "votacion" o "confirmados"
    public IActionResult ListaPlanes(string filtro)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        bool? confirmado = null;
        if (filtro == "votacion") confirmado = false;
        if (filtro == "confirmados") confirmado = true;

        Usuarios usuario = UsuarioDeSesion();
        return Json(BD.ObtenerPlanesDeUsuario(usuario.Id, confirmado));
    }

    // Un plan se crea desde el chat de un grupo (07b). Los "Crear plan" de otras pantallas llevan
    // al chat más reciente con el modal abierto; idRestaurante es el lugar que viene del Detalle.
    public IActionResult NuevoPlan(int? idRestaurante)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        return RedirectToAction("Index", "Grupos", new { plan = 1, lugar = idRestaurante });
    }

    // La dirección vieja de "Ver detalle"
    public IActionResult Votacion(int id)
    {
        return RedirectToAction("Detalle", new { id });
    }

    // 07h · Detalle del plan en votación / 07i · confirmado (privada).
    // Solo lo ven los integrantes del grupo; el creador confirma o cancela desde acá.
    public IActionResult Detalle(int id)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        EncuestaPlan? encuesta = BD.ObtenerEncuesta(id, usuario.Id);
        if (encuesta == null || !BD.EsIntegranteDeGrupo(usuario.Id, encuesta.IdGrupo)) return RedirectToAction("Index", "Grupos");

        PrepararDetalle(encuesta, usuario.Id);
        return View(encuesta);
    }

    // La parte de la página que cambia (votos, participantes y estado). detalle-plan.js la pide cada 3 segundos.
    public IActionResult DetalleParcial(int id)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        EncuestaPlan? encuesta = BD.ObtenerEncuesta(id, usuario.Id);
        if (encuesta == null || !BD.EsIntegranteDeGrupo(usuario.Id, encuesta.IdGrupo)) return StatusCode(403);

        PrepararDetalle(encuesta, usuario.Id);
        return PartialView("_DetallePlan", encuesta);
    }

    private void PrepararDetalle(EncuestaPlan encuesta, int idUsuario)
    {
        ViewBag.IdUsuario = idUsuario;
        // "Opciones sin TACC verificadas" del lugar elegido
        ViewBag.Restricciones = encuesta.IdRestauranteElegido == null
            ? new List<string>()
            : BD.ObtenerRestriccionesDeRestaurante(encuesta.IdRestauranteElegido.Value);
    }

    // ---------- Acciones que llama el JavaScript con fetch ----------

    // Buscador de cada campo de lugar. Devuelve las restricciones del grupo (para "Aptos para todo el grupo")
    // y hasta 6 lugares, primero los aptos para todos.
    public IActionResult BuscarLugares(int idGrupo, string? texto)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        if (!BD.EsIntegranteDeGrupo(usuario.Id, idGrupo)) return StatusCode(403);

        texto = (texto ?? "").Trim();
        List<LugarBuscado> lugares = texto == "" ? new List<LugarBuscado>() : BD.BuscarLugaresParaGrupo(idGrupo, texto);
        return Json(new
        {
            restriccionesGrupo = BD.ObtenerRestriccionesDeGrupo(idGrupo),
            lugares = lugares.Select(l => new
            {
                id = l.Id,
                nombre = l.Nombre,
                detalle = l.Categoria == null ? l.BarrioCorto() : l.Categoria + " · " + l.BarrioCorto(),
                textoCampo = l.TextoCampo(),
                apto = l.AptoParaTodos,
                restricciones = l.Restricciones
            })
        });
    }

    // "Crear plan" del modal. Devuelve { ok, campo, mensaje } con el primer error (campo = "nombre",
    // "lugar1", "lugar2", "lugar3", "dia" u "horarios") o { ok } si se publicó en el chat.
    [HttpPost]
    public IActionResult CrearPlan([FromBody] NuevoPlan datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        if (!BD.EsIntegranteDeGrupo(usuario.Id, datos.IdGrupo)) return StatusCode(403);

        string titulo = (datos.Titulo ?? "").Trim();
        if (titulo == "") return Json(new { ok = false, campo = "nombre", mensaje = "Poné el nombre del plan." });
        if (titulo.Length > 50) return Json(new { ok = false, campo = "nombre", mensaje = "El nombre puede tener hasta 50 caracteres." });

        // Los 3 campos de lugar: se guardan los que tienen un lugar elegido, sin repetir
        List<int> lugares = new List<int>();
        for (int i = 0; i < datos.Lugares.Count && i < 3; i++)
        {
            int? idLugar = datos.Lugares[i];
            if (idLugar == null) continue;
            if (!BD.ExisteRestaurante(idLugar.Value))
                return Json(new { ok = false, campo = "lugar" + (i + 1), mensaje = "Elegí un lugar de la lista." });
            if (lugares.Contains(idLugar.Value))
                return Json(new { ok = false, campo = "lugar" + (i + 1), mensaje = "Ese lugar ya está en otra opción." });
            lugares.Add(idLugar.Value);
        }
        if (lugares.Count < 2)
        {
            // El error va en el primer campo vacío
            int vacio = 1;
            while (vacio <= datos.Lugares.Count && datos.Lugares[vacio - 1] != null) vacio++;
            return Json(new { ok = false, campo = "lugar" + Math.Min(vacio, 3), mensaje = "Elegí al menos 2 lugares para votar." });
        }

        if (!DateTime.TryParseExact(datos.Dia, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dia))
            return Json(new { ok = false, campo = "dia", mensaje = "Elegí un día." });
        if (dia < DateTime.Today)
            return Json(new { ok = false, campo = "dia", mensaje = "Elegí un día que todavía no pasó." });

        // Los chips de horario: entre 2 y 3, sin repetir
        List<TimeSpan> horarios = new List<TimeSpan>();
        foreach (string texto in datos.Horarios)
        {
            if (TimeSpan.TryParseExact(texto, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan horario) && !horarios.Contains(horario))
                horarios.Add(horario);
        }
        if (horarios.Count < 2) return Json(new { ok = false, campo = "horarios", mensaje = "Marcá al menos 2 horarios para votar." });
        if (horarios.Count > 3) return Json(new { ok = false, campo = "horarios", mensaje = "Podés marcar hasta 3 horarios." });
        if (horarios.Any(h => dia.Add(h) <= DateTime.Now))
            return Json(new { ok = false, campo = "horarios", mensaje = "Hay horarios que ya pasaron. Elegí otros u otro día." });

        BD.CrearPlan(titulo, dia, datos.IdGrupo, usuario.Id, lugares, horarios.OrderBy(h => h).ToList());
        return Json(new { ok = true });
    }

    // Votar el lugar desde la encuesta. Recibe: { idPlan: 1, idRestaurante: 4 }. Se puede cambiar mientras esté En votación.
    [HttpPost]
    public IActionResult Votar([FromBody] Votos datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        Planes? plan = BD.ObtenerPlan(datos.IdPlan ?? 0);
        if (plan == null || !BD.EsIntegranteDeGrupo(usuario.Id, plan.IdGrupo ?? 0)) return StatusCode(403);
        if (plan.Estado != 0) return Json(new { ok = false, mensaje = "La votación ya terminó." });

        int idRestaurante = datos.IdRestaurante ?? 0;
        if (!BD.EsLugarDelPlan(plan.Id, idRestaurante)) return Json(new { ok = false, mensaje = "Ese lugar no está en el plan." });

        BD.Votar(plan.Id, usuario.Id, idRestaurante);
        return Json(new { ok = true });
    }

    // Votar el horario. Recibe: { idPlan: 1, idHorarioVotado: 2 }. Igual que el lugar: se puede cambiar mientras esté En votación.
    // Votos guarda lugar y horario juntos, así que primero hay que votar un lugar.
    [HttpPost]
    public IActionResult VotarHorario([FromBody] Votos datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        Planes? plan = BD.ObtenerPlan(datos.IdPlan ?? 0);
        if (plan == null || !BD.EsIntegranteDeGrupo(usuario.Id, plan.IdGrupo ?? 0)) return StatusCode(403);
        if (plan.Estado != 0) return Json(new { ok = false, mensaje = "La votación ya terminó." });

        int idHorario = datos.IdHorarioVotado ?? 0;
        if (!BD.EsHorarioDelPlan(plan.Id, idHorario)) return Json(new { ok = false, mensaje = "Ese horario no está en el plan." });

        int? idLugar = BD.ObtenerLugarVotado(plan.Id, usuario.Id);
        if (idLugar == null) return Json(new { ok = false, mensaje = "Votá un lugar para elegir el horario." });

        BD.VotarHorario(plan.Id, usuario.Id, idLugar.Value, idHorario);
        return Json(new { ok = true });
    }

    // "Confirmar lugar y horario": solo el creador, aunque no hayan votado todos.
    // Gana el más votado de cada uno; si hay empate, elige el creador entre los empatados.
    // Recibe: { idPlan: 1, idRestaurante: 4, idHorario: 2 }
    [HttpPost]
    public IActionResult Confirmar([FromBody] DecisionPlan datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        EncuestaPlan? encuesta = BD.ObtenerEncuesta(datos.IdPlan, usuario.Id);
        if (encuesta == null || !BD.EsIntegranteDeGrupo(usuario.Id, encuesta.IdGrupo)) return StatusCode(403);
        if (encuesta.IdCreador != usuario.Id) return StatusCode(403);
        if (!encuesta.EnVotacion()) return Json(new { ok = false, mensaje = "La votación ya terminó." });

        if (!encuesta.LugaresMasVotados().Any(o => o.IdRestaurante == datos.IdRestaurante))
            return Json(new { ok = false, mensaje = "Solo podés confirmar el lugar con más votos." });
        OpcionHorario? horario = encuesta.HorariosMasVotados().FirstOrDefault(h => h.IdHorario == datos.IdHorario);
        if (horario == null)
            return Json(new { ok = false, mensaje = "Solo podés confirmar el horario con más votos." });

        BD.ConfirmarPlan(encuesta.Id, datos.IdRestaurante, horario.FechaHoraPropuesta);
        return Json(new { ok = true });
    }

    // "Cancelar plan": solo el creador y solo mientras esté En votación. Recibe: { idPlan: 1 }
    [HttpPost]
    public IActionResult Cancelar([FromBody] DecisionPlan datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        Planes? plan = BD.ObtenerPlan(datos.IdPlan);
        if (plan == null || !BD.EsIntegranteDeGrupo(usuario.Id, plan.IdGrupo ?? 0)) return StatusCode(403);
        if (plan.IdUsuarioCreador != usuario.Id) return StatusCode(403);
        if (plan.Estado != 0) return Json(new { ok = false, mensaje = "La votación ya terminó." });

        BD.CancelarPlan(plan.Id);
        return Json(new { ok = true });
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
