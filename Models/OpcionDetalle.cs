namespace Bodeglook.Models;

public class OpcionDetalle
{
    public string Nombre { get; set; } = "";
    public string Detalle { get; set; } = "";
    public int Votos { get; set; }
    public int Porcentaje { get; set; }
    public List<Participante> Votantes { get; set; } = new List<Participante>();
    public bool Resaltada { get; set; }
    public int IdUsuario { get; set; }

    public string TextoVotos()
    {
        return Votos + (Votos == 1 ? " voto" : " votos");
    }
}
