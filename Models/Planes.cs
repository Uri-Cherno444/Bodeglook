namespace Bodeglook.Models;

public class Planes
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public DateTime? FechaCierreVotacion { get; set; }
    public DateTime? FechaSalida { get; set; }
    public int Estado { get; set; }
    public int? IdGrupo { get; set; }
    public int? IdRestauranteElegido { get; set; }
    public int? IdUsuarioCreador { get; set; }

    public string TextoFechaSalida()
    {
        if (FechaSalida == null) return "";
        string[] dias = { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };
        return dias[(int)FechaSalida.Value.DayOfWeek] + " " + FechaSalida.Value.ToString("dd/MM");
    }
}
