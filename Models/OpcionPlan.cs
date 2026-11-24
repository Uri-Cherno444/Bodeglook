namespace Bodeglook.Models;

public class OpcionPlan
{
    public int IdRestaurante { get; set; }
    public string? Nombre { get; set; }
    private string? _foto;
    public string? Foto
    {
        get { return Fotos.Portada(IdRestaurante, _foto); }
        set { _foto = value; }
    }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public int Votos { get; set; }
    public string? SitioWeb { get; set; }

    public string TextoDetalle()
    {
        string barrio = (Barrio ?? "").Split(',')[0];
        if (Categoria == null) return barrio;
        if (barrio == "") return Categoria;
        return Categoria + " · " + barrio;
    }
}
