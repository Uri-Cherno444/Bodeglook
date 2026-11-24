namespace Bodeglook.Models;

public class TarjetaPlan
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public int NumeroEstado { get; set; }
    public string Estado
    {
        get { return NumeroEstado == 1 ? "Confirmado" : "En votación"; }
    }
    public int IdGrupo { get; set; }
    public int CantidadLugares { get; set; }
    public int? IdRestauranteElegido { get; set; }
    public string? LugarElegido { get; set; }
    public List<string> Iniciales { get; set; } = new List<string>();

    public const int CantidadEnPerfil = 6;

    public bool EsConfirmado()
    {
        return Estado == "Confirmado";
    }

    public string TextoDetalle()
    {
        if (EsConfirmado()) return "Confirmado en " + LugarElegido;
        return CantidadLugares + (CantidadLugares == 1 ? " lugar propuesto" : " lugares propuestos");
    }

    public static bool? ConfirmadoSegunFiltro(string? filtro)
    {
        if (filtro == "votacion") return false;
        if (filtro == "confirmados") return true;
        return null;
    }
}
