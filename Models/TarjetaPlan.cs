namespace Bodeglook.Models;

// No es una tabla: los datos que muestra cada tarjeta de plan en 08 · Mis Planes.
public class TarjetaPlan
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    // Planes.Estado: 0 = En votación, 1 = Confirmado (ListarPlanesUsuario no trae los cancelados)
    public int NumeroEstado { get; set; }
    // El texto del chip: "Confirmado" o "En votación"
    public string Estado
    {
        get { return NumeroEstado == 1 ? "Confirmado" : "En votación"; }
    }
    public int IdGrupo { get; set; }
    public int CantidadLugares { get; set; }
    public int? IdRestauranteElegido { get; set; }
    public string? LugarElegido { get; set; }
    public List<string> Iniciales { get; set; } = new List<string>();

    // "Mis Planes" del Perfil muestra 6 (tres filas); "Ver todos" muestra el resto
    public const int CantidadEnPerfil = 6;

    public bool EsConfirmado()
    {
        return Estado == "Confirmado";
    }

    // "Confirmado en Bodega Los Alamos" o "3 lugares propuestos"
    public string TextoDetalle()
    {
        if (EsConfirmado()) return "Confirmado en " + LugarElegido;
        return CantidadLugares + (CantidadLugares == 1 ? " lugar propuesto" : " lugares propuestos");
    }

    // Las pestañas de Mis Planes: "votacion" (false), "confirmados" (true) o "todos" (null)
    public static bool? ConfirmadoSegunFiltro(string? filtro)
    {
        if (filtro == "votacion") return false;
        if (filtro == "confirmados") return true;
        return null;
    }
}
