using System.Globalization;

namespace Bodeglook.Models;

// No es una tabla: un mensaje del chat con el nombre de quien lo escribió.
public class MensajeChat
{
    public int Id { get; set; }
    public string? Contenido { get; set; }
    public DateTime FechaHora { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreCompleto { get; set; }
    // La foto de perfil de quien lo escribió (null: se muestran las iniciales)
    public string? FotoPerfil { get; set; }
    // Si no es null, el mensaje es la encuesta de ese plan
    public int? IdPlan { get; set; }

    // Separador por día: "Hoy", "Ayer" o "Lun 26/10"
    public string TextoDia()
    {
        if (FechaHora.Date == DateTime.Today) return "Hoy";
        if (FechaHora.Date == DateTime.Today.AddDays(-1)) return "Ayer";

        string dia = FechaHora.ToString("ddd", new CultureInfo("es-AR")).Replace(".", "");
        return char.ToUpper(dia[0]) + dia.Substring(1) + " " + FechaHora.ToString("dd/MM");
    }

    // Lo que necesita chat.js para dibujar un mensaje. idUsuario: quien está mirando (sus mensajes van a la derecha).
    public object ParaJson(int idUsuario)
    {
        Usuarios autor = new Usuarios { NombreCompleto = NombreCompleto };
        return new
        {
            id = Id,
            contenido = Contenido,
            hora = FechaHora.ToString("HH:mm"),
            dia = FechaHora.ToString("yyyy-MM-dd"),
            textoDia = TextoDia(),
            esMio = IdUsuario == idUsuario,
            nombre = autor.ObtenerPrimerNombre(),
            iniciales = autor.ObtenerIniciales(),
            foto = Fotos.Avatar(FotoPerfil),
            idPlan = IdPlan
        };
    }

    // Lo que devuelve /Grupos/Mensajes: los mensajes con Id mayor a "desde" y cómo están todas las encuestas del grupo.
    // Si los está viendo, los nuevos no cuentan como no leídos.
    public static object NuevosDelGrupo(int idGrupo, int desde, int idUsuario)
    {
        BD.MarcarComoLeido(idUsuario, idGrupo);
        List<MensajeChat> mensajes = BD.ObtenerMensajes(idGrupo, desde);
        List<EncuestaPlan> encuestas = BD.ObtenerEncuestasDeGrupo(idGrupo, idUsuario);
        return new
        {
            mensajes = mensajes.Select(m => m.ParaJson(idUsuario)),
            encuestas = encuestas.Select(e => e.ParaJson())
        };
    }
}
