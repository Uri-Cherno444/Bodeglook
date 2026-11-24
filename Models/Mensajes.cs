namespace Bodeglook.Models;

public class Mensajes
{
    public int Id { get; set; }
    public string? Contenido { get; set; }
    public DateTime? FechaHora { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdPlan { get; set; }

    public object? Enviar(int idUsuario)
    {
        int idGrupo = IdGrupo ?? 0;
        if (!BD.EsIntegranteDeGrupo(idUsuario, idGrupo)) return null;

        string contenido = (Contenido ?? "").Trim();
        if (contenido == "") return new { ok = false, mensaje = "Escribí un mensaje." };
        if (contenido.Length > 1000) return new { ok = false, mensaje = "El mensaje es demasiado largo." };

        BD.GuardarMensaje(idGrupo, idUsuario, contenido);
        return new { ok = true };
    }
}
