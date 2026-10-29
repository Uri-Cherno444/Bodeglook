using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class GruposController : Controller
{
    // Mis Chats abre el chat más reciente. Si todavía no está en ningún grupo, muestra la lista vacía.
    // crear = 1 abre el modal Crear grupo (lo usa el ícono de "Chats" desde otras pantallas).
    // plan = 1 abre el modal Crear plan; lugar es el restaurante que viene ya cargado desde el Detalle.
    public IActionResult Index(int? crear, int? plan, int? lugar)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        List<ResumenChat> chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        if (chats.Count > 0) return RedirectToAction("Chat", new { id = chats[0].IdGrupo, crear, plan, lugar });

        ViewBag.Chats = chats;
        ViewBag.GrupoAbierto = 0;
        return View("Chat", null);
    }

    // 07 · Chat de grupo (privada). Solo lo ven los integrantes: el resto vuelve a Mis Chats.
    public IActionResult Chat(int id, int? lugar)
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios usuario = UsuarioDeSesion();
        if (!BD.EsIntegranteDeGrupo(usuario.Id, id)) return RedirectToAction("Index");

        // Se marca como leído antes de armar la lista, así el chat abierto aparece sin no leídos
        BD.MarcarComoLeido(usuario.Id, id);
        ViewBag.Chats = BD.ObtenerChatsDeUsuario(usuario.Id);
        ViewBag.GrupoAbierto = id;
        ViewBag.Integrantes = TextoIntegrantes(id, usuario.Id);
        // El primer lugar del modal Crear plan, si viene desde el Detalle de un restaurante
        ViewBag.LugarInicial = lugar == null ? null : BD.ObtenerLugar(lugar.Value);
        return View(BD.ObtenerGrupo(id));
    }

    // ---------- Acciones que llama el JavaScript con fetch ----------

    // Los mensajes con Id mayor a "desde". El chat la llama al abrir (desde = 0) y cada 3 segundos.
    // También devuelve cómo están todas las encuestas del grupo, así los votos se actualizan solos.
    public IActionResult Mensajes(int id, int desde)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        if (!BD.EsIntegranteDeGrupo(usuario.Id, id)) return StatusCode(403);

        // Si los está viendo, los nuevos no cuentan como no leídos
        BD.MarcarComoLeido(usuario.Id, id);
        List<MensajeChat> mensajes = BD.ObtenerMensajes(id, desde);
        List<EncuestaPlan> encuestas = BD.ObtenerEncuestasDeGrupo(id, usuario.Id);
        return Json(new
        {
            mensajes = mensajes.Select(m => ParaJson(m, usuario.Id)),
            encuestas = encuestas.Select(e => EncuestaParaJson(e, usuario.Id))
        });
    }

    // Recibe: { idGrupo: 3, contenido: "Hola" }
    [HttpPost]
    public IActionResult EnviarMensaje([FromBody] Mensajes datos)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        int idGrupo = datos.IdGrupo ?? 0;
        if (!BD.EsIntegranteDeGrupo(usuario.Id, idGrupo)) return StatusCode(403);

        string contenido = (datos.Contenido ?? "").Trim();
        if (contenido == "") return Json(new { ok = false, mensaje = "Escribí un mensaje." });
        if (contenido.Length > 1000) return Json(new { ok = false, mensaje = "El mensaje es demasiado largo." });

        BD.GuardarMensaje(idGrupo, usuario.Id, contenido);
        return Json(new { ok = true });
    }

    // La lista "Chats" ya armada, para refrescarla sin recargar (hora, orden, no leídos y grupos nuevos)
    public IActionResult ListaChats(int abierto)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        ViewBag.GrupoAbierto = abierto;
        return PartialView("_Chats", BD.ObtenerChatsDeUsuario(usuario.Id));
    }

    // "Sumá amigos" del modal Crear grupo
    public IActionResult Amigos()
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        List<Usuarios> amigos = BD.ObtenerOtrosUsuarios(usuario.Id);
        return Json(amigos.Select(a => new { id = a.Id, nombre = a.NombreCompleto, iniciales = a.ObtenerIniciales() }));
    }

    // Recibe un formulario (por la foto): nombre, integrantes (uno por amigo marcado) y foto (opcional).
    // Devuelve { ok, mensaje } si hay un error o { ok, destino } con el chat nuevo.
    [HttpPost]
    public IActionResult CrearGrupo(string? nombre, List<int> integrantes, IFormFile? foto)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios usuario = UsuarioDeSesion();
        nombre = (nombre ?? "").Trim();
        if (nombre == "") return Json(new { ok = false, mensaje = "Poné el nombre del grupo." });
        if (nombre.Length > 50) return Json(new { ok = false, mensaje = "El nombre del grupo puede tener hasta 50 caracteres." });

        // Solo se suman usuarios que existen (y no el creador, que entra solo)
        List<int> existentes = BD.ObtenerOtrosUsuarios(usuario.Id).Select(u => u.Id).ToList();
        List<int> validos = integrantes.Where(id => existentes.Contains(id)).Distinct().ToList();
        if (validos.Count == 0) return Json(new { ok = false, mensaje = "Sumá al menos un amigo." });

        string extension = "";
        if (foto != null)
        {
            extension = Path.GetExtension(foto.FileName).ToLower();
            string[] permitidas = { ".jpg", ".jpeg", ".png", ".webp" };
            if (!permitidas.Contains(extension) || !foto.ContentType.StartsWith("image/"))
                return Json(new { ok = false, mensaje = "La foto tiene que ser JPG, PNG o WEBP." });
            if (foto.Length > 5 * 1024 * 1024)
                return Json(new { ok = false, mensaje = "La foto puede pesar hasta 5 MB." });
        }

        int idGrupo = BD.CrearGrupo(nombre, usuario.Id, validos);

        // La foto se guarda en wwwroot/img con el Id del grupo: /img/grupo-7.jpg
        if (foto != null)
        {
            string archivo = "grupo-" + idGrupo + extension;
            string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", archivo);
            using (FileStream destino = new FileStream(ruta, FileMode.Create))
            {
                foto.CopyTo(destino);
            }
            BD.ActualizarFotoGrupo(idGrupo, "/img/" + archivo);
        }

        return Json(new { ok = true, destino = "/Grupos/Chat/" + idGrupo });
    }

    // ---------- Ayudas ----------

    // Lo que necesita chat.js para dibujar un mensaje
    private object ParaJson(MensajeChat mensaje, int idUsuario)
    {
        Usuarios autor = new Usuarios { NombreCompleto = mensaje.NombreCompleto };
        return new
        {
            id = mensaje.Id,
            contenido = mensaje.Contenido,
            hora = mensaje.FechaHora.ToString("HH:mm"),
            dia = mensaje.FechaHora.ToString("yyyy-MM-dd"),
            textoDia = mensaje.TextoDia(),
            esMio = mensaje.IdUsuario == idUsuario,
            nombre = autor.ObtenerPrimerNombre(),
            iniciales = autor.ObtenerIniciales(),
            idPlan = mensaje.IdPlan
        };
    }

    // Lo que necesita chat.js para dibujar una encuesta (07), un plan confirmado (07e) o cancelado.
    // estado: "votacion", "confirmado" o "cancelado". La barra de cada opción es votos / integrantes del grupo.
    private object EncuestaParaJson(EncuestaPlan encuesta, int idUsuario)
    {
        string estado = encuesta.Confirmado() ? "confirmado" : encuesta.Cancelado() ? "cancelado" : "votacion";
        return new
        {
            id = encuesta.Id,
            titulo = encuesta.Titulo,
            estado,
            textoDia = encuesta.TextoDia(),
            textoDiaYHora = encuesta.TextoDiaYHora(),
            creador = encuesta.PrimerNombreCreador(),
            integrantes = encuesta.CantidadIntegrantes,
            votaron = encuesta.CantidadVotaron(),
            miVoto = encuesta.MiVoto,
            miVotoHorario = encuesta.MiVotoHorario,
            idElegido = encuesta.IdRestauranteElegido,
            idHorarioElegido = encuesta.HorarioElegido()?.IdHorario,
            lugarElegido = encuesta.LugarElegido()?.Nombre,
            opciones = encuesta.Opciones.Select(o => new
            {
                idRestaurante = o.IdRestaurante,
                nombre = o.Nombre,
                detalle = o.TextoDetalle(),
                votos = o.Votos,
                porcentaje = encuesta.Porcentaje(o.Votos)
            }),
            horarios = encuesta.Horarios.Select(h => new
            {
                idHorario = h.IdHorario,
                horario = h.TextoHorario(),
                votos = h.Votos,
                porcentaje = encuesta.Porcentaje(h.Votos)
            })
        };
    }

    // Cabecera del chat: "Cami, Sofi y vos"
    private string TextoIntegrantes(int idGrupo, int idUsuario)
    {
        List<string> nombres = BD.ObtenerIntegrantesDeGrupo(idGrupo)
            .Where(i => i.Id != idUsuario)
            .Select(i => i.ObtenerPrimerNombre())
            .ToList();
        if (nombres.Count == 0) return "Solo vos";
        return string.Join(", ", nombres) + " y vos";
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
