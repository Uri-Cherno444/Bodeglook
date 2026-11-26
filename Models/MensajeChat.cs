using System.Globalization;

namespace Bodeglook.Models;

public class MensajeChat
{
    public int Id { get; set; }
    public string? Contenido { get; set; }
    public DateTime FechaHora { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreCompleto { get; set; }
    public string? FotoPerfil { get; set; }
    public int? IdPlan { get; set; }

    public string TextoDia()
    {
        if (FechaHora.Date == DateTime.Today) return "Hoy";
        if (FechaHora.Date == DateTime.Today.AddDays(-1)) return "Ayer";

        string dia = FechaHora.ToString("ddd", new CultureInfo("es-AR")).Replace(".", "");
        return char.ToUpper(dia[0]) + dia.Substring(1) + " " + FechaHora.ToString("dd/MM");
    }

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

    public static object NuevosDelGrupo(int idGrupo, int desde, int idUsuario)
    {
        BD.MarcarComoLeido(idUsuario, idGrupo);
        List<MensajeChat> mensajes = BD.ObtenerMensajes(idGrupo, desde);
        List<EncuestaPlan> encuestas = BD.ObtenerEncuestasDeGrupo(idGrupo, idUsuario);
        return new
        {
            grupo = Grupos.Cabecera(idGrupo, idUsuario),
            mensajes = mensajes.Select(m => m.ParaJson(idUsuario)),
            encuestas = encuestas.Select(e => e.ParaJson())
        };
    }
}
