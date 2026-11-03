using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Bodeglook.Models;

namespace Bodeglook.Controllers;

public class PerfilController : Controller
{
    // 08 · Perfil y Amigos (privada)
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("usuario") == null) return RedirectToAction("IniciarSesion", "Cuenta");

        Usuarios sesion = UsuarioDeSesion();
        // Se leen de la base (no de la sesión) para que la foto y los datos guardados siempre estén al día
        Usuarios? perfil = BD.ObtenerDatosEdicion(sesion.Id);
        if (perfil == null) return RedirectToAction("IniciarSesion", "Cuenta");

        List<Barrios> barrios = BD.ObtenerBarrios();
        ViewBag.Barrio = barrios.FirstOrDefault(b => b.Id == perfil.IdBarrio)?.Nombre;
        ViewBag.CantidadResenas = BD.ContarResenasDeUsuario(sesion.Id);
        // "Gustos y restricciones": las del onboarding
        ViewBag.Restricciones = BD.ObtenerRestriccionesDeUsuario(sesion.Id);
        // "Restaurantes en mi perfil": sus favoritos
        ViewBag.Restaurantes = BD.ObtenerFavoritos(sesion.Id);
        // "Mis Planes": los de todos sus grupos
        ViewBag.Planes = BD.ObtenerPlanesDeUsuario(sesion.Id, null);

        // 08c · Editar perfil (modal): las opciones y lo que el usuario ya eligió
        ViewBag.Barrios = barrios;
        ViewBag.TodasRestricciones = BD.ObtenerRestricciones();
        ViewBag.ZonasMarcadas = BD.ObtenerZonasDeUsuario(sesion.Id);
        ViewBag.CompaniasMarcadas = BD.ObtenerCompaniasDeUsuario(sesion.Id);
        return View(perfil);
    }

    // La llama "Guardar cambios" del modal Editar perfil con fetch. Recibe un formulario (por la foto):
    // nombreCompleto, nombreUsuario, idBarrio, restricciones (una por Id marcado), zonas, frecuencia,
    // companias, soloApto y foto (solo si la cambió).
    // Devuelve { ok, campo, mensaje } si hay un error ("campo" = nombre, usuario, ubicacion o aviso)
    // o { ok, perfil } con lo que hay que mostrar en la tarjeta de perfil.
    [HttpPost]
    public IActionResult GuardarPerfil(string? nombreCompleto, string? nombreUsuario, int? idBarrio,
        List<int>? restricciones, List<int>? zonas, string? frecuencia, List<string>? companias,
        bool soloApto, IFormFile? foto)
    {
        if (HttpContext.Session.GetString("usuario") == null) return Unauthorized();

        Usuarios sesion = UsuarioDeSesion();

        nombreCompleto = (nombreCompleto ?? "").Trim();
        nombreUsuario = (nombreUsuario ?? "").Trim();

        if (nombreCompleto == "")
            return Json(new { ok = false, campo = "nombre", mensaje = "Completá tu nombre y apellido." });
        if (nombreCompleto.Length > 100)
            return Json(new { ok = false, campo = "nombre", mensaje = "Puede tener hasta 100 caracteres." });

        if (nombreUsuario == "")
            return Json(new { ok = false, campo = "usuario", mensaje = "Elegí un nombre de usuario." });
        if (nombreUsuario.Length > 50)
            return Json(new { ok = false, campo = "usuario", mensaje = "Puede tener hasta 50 caracteres." });
        if (nombreUsuario.Any(char.IsWhiteSpace))
            return Json(new { ok = false, campo = "usuario", mensaje = "No puede tener espacios." });
        if (BD.ExisteNombreUsuario(nombreUsuario, sesion.Id))
            return Json(new { ok = false, campo = "usuario", mensaje = "Ese usuario ya existe. Probá con otro." });

        List<Barrios> barrios = BD.ObtenerBarrios();
        Barrios? barrio = barrios.FirstOrDefault(b => b.Id == idBarrio);
        if (barrio == null)
            return Json(new { ok = false, campo = "ubicacion", mensaje = "Elegí tu ubicación." });

        // La foto: se mira qué es de verdad (no lo que dice el nombre del archivo)
        string extension = "";
        if (foto != null)
        {
            if (foto.Length > 5 * 1024 * 1024)
                return Json(new { ok = false, campo = "aviso", mensaje = "La foto puede pesar hasta 5 MB." });
            extension = ExtensionDeImagen(foto);
            if (extension == "")
                return Json(new { ok = false, campo = "aviso", mensaje = "La foto tiene que ser JPG, PNG o WEBP." });
        }

        DatosGustos gustos = Gustos.Limpiar(new DatosGustos
        {
            Restricciones = restricciones ?? new List<int>(),
            Zonas = zonas ?? new List<int>(),
            Frecuencia = frecuencia,
            Companias = companias ?? new List<string>()
        });
        // Sin restricciones no hay nada que filtrar: queda "Ver todo"
        bool soloAptoFinal = soloApto && gustos.Restricciones.Count > 0;

        string? fotoAnterior = BD.ObtenerDatosEdicion(sesion.Id)?.FotoPerfil;
        string? fotoNueva = null;
        string rutaNueva = "";
        if (foto != null)
        {
            // La foto se guarda en wwwroot/img/usuarios con el Id y la hora (así el navegador no muestra la vieja)
            string archivo = "usuario-" + sesion.Id + "-" + DateTime.UtcNow.Ticks + extension;
            string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "usuarios");
            Directory.CreateDirectory(carpeta);
            rutaNueva = Path.Combine(carpeta, archivo);
            using (FileStream destino = new FileStream(rutaNueva, FileMode.Create))
            {
                foto.CopyTo(destino);
            }
            fotoNueva = "/img/usuarios/" + archivo;
        }

        try
        {
            BD.GuardarPerfil(sesion.Id, nombreCompleto, nombreUsuario, barrio.Id, fotoNueva, gustos, soloAptoFinal);
        }
        catch
        {
            // Si la base falla no queda una foto suelta
            if (rutaNueva != "") System.IO.File.Delete(rutaNueva);
            throw;
        }

        // La foto vieja ya no se usa: se borra (solo si es una de las subidas, no una foto de ejemplo)
        if (fotoNueva != null && fotoAnterior != null && fotoAnterior.StartsWith("/img/usuarios/"))
        {
            string rutaVieja = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "usuarios", Path.GetFileName(fotoAnterior));
            if (System.IO.File.Exists(rutaVieja)) System.IO.File.Delete(rutaVieja);
        }

        // El header y el menú del avatar leen la sesión: se actualiza con lo nuevo
        sesion.NombreCompleto = nombreCompleto;
        sesion.NombreUsuario = nombreUsuario;
        sesion.IdBarrio = barrio.Id;
        if (fotoNueva != null) sesion.FotoPerfil = fotoNueva;
        HttpContext.Session.SetString("usuario", JsonSerializer.Serialize(sesion));

        List<Restricciones> restriccionesGuardadas = BD.ObtenerRestriccionesDeUsuario(sesion.Id);
        return Json(new
        {
            ok = true,
            perfil = new
            {
                nombre = nombreCompleto,
                usuario = nombreUsuario,
                barrio = barrio.Nombre,
                foto = fotoNueva,
                iniciales = sesion.ObtenerIniciales(),
                restricciones = restriccionesGuardadas.Select(r => r.Nombre)
            }
        });
    }

    // Mira los primeros bytes del archivo: ".jpg", ".png" o ".webp". Si no es ninguna, "".
    private static string ExtensionDeImagen(IFormFile archivo)
    {
        byte[] inicio = new byte[12];
        int leidos;
        using (Stream lector = archivo.OpenReadStream())
        {
            leidos = lector.Read(inicio, 0, inicio.Length);
        }
        if (leidos < 12) return "";

        if (inicio[0] == 0xFF && inicio[1] == 0xD8 && inicio[2] == 0xFF) return ".jpg";
        if (inicio[0] == 0x89 && inicio[1] == 0x50 && inicio[2] == 0x4E && inicio[3] == 0x47) return ".png";
        if (inicio[0] == 'R' && inicio[1] == 'I' && inicio[2] == 'F' && inicio[3] == 'F'
            && inicio[8] == 'W' && inicio[9] == 'E' && inicio[10] == 'B' && inicio[11] == 'P') return ".webp";
        return "";
    }

    private Usuarios UsuarioDeSesion()
    {
        return JsonSerializer.Deserialize<Usuarios>(HttpContext.Session.GetString("usuario")!)!;
    }
}
