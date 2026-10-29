namespace Bodeglook.Models;

// No es una tabla: un integrante del grupo y lo que votó en el plan ("Participantes" de 07h).
public class Participante
{
    public int IdUsuario { get; set; }
    public string? NombreCompleto { get; set; }
    public int? IdLugarVotado { get; set; }
    public string? LugarVotado { get; set; }
    // Igual que OpcionHorario.IdHorario: la primera fila de PlanesHorarios con esa hora
    public int? IdHorarioVotado { get; set; }
    public DateTime? HorarioVotado { get; set; }

    public bool VotoTodo()
    {
        return IdLugarVotado != null && IdHorarioVotado != null;
    }

    // Para el usuario de la sesión va "YO" y "Vos", como en el Figma
    public string Iniciales(int idUsuarioSesion)
    {
        if (IdUsuario == idUsuarioSesion) return "YO";
        return new Usuarios { NombreCompleto = NombreCompleto }.ObtenerIniciales();
    }

    public string Nombre(int idUsuarioSesion)
    {
        if (IdUsuario == idUsuarioSesion) return "Vos";
        return new Usuarios { NombreCompleto = NombreCompleto }.ObtenerPrimerNombre();
    }

    // "Votó · Bodega Los Alamos · 21:00", "Votaste · ..." o "Todavía no votó"
    public string TextoVoto(int idUsuarioSesion)
    {
        if (IdLugarVotado == null && IdHorarioVotado == null)
            return IdUsuario == idUsuarioSesion ? "Todavía no votaste" : "Todavía no votó";

        string texto = IdUsuario == idUsuarioSesion ? "Votaste" : "Votó";
        if (LugarVotado != null) texto += " · " + LugarVotado;
        if (HorarioVotado != null) texto += " · " + HorarioVotado.Value.ToString("HH:mm");
        return texto;
    }
}
