namespace Bodeglook.Models;

// No es una tabla: un lugar propuesto en un plan, con sus votos.
public class OpcionPlan
{
    public int IdRestaurante { get; set; }
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public int Votos { get; set; }
    // Para "Reservar en la web del restaurante" del lugar elegido
    public string? SitioWeb { get; set; }

    // "Vinoteca · Cañuelas" (del barrio va solo lo de antes de la coma: "Cañuelas, Bs. As." -> "Cañuelas")
    public string TextoDetalle()
    {
        string barrio = (Barrio ?? "").Split(',')[0];
        if (Categoria == null) return barrio;
        if (barrio == "") return Categoria;
        return Categoria + " · " + barrio;
    }
}
