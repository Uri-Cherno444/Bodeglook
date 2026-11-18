namespace Bodeglook.Models;

// No es una tabla: una opción de lugar o de horario del Detalle del plan, lista para dibujar
// (nombre, detalle, votos, barra e iniciales de quién votó). La dibuja Views/Planes/_OpcionDetalle.cshtml.
public class OpcionDetalle
{
    public string Nombre { get; set; } = "";
    public string Detalle { get; set; } = "";
    public int Votos { get; set; }
    // Ancho de la barra: votos / integrantes del grupo
    public int Porcentaje { get; set; }
    public List<Participante> Votantes { get; set; } = new List<Participante>();
    public bool Resaltada { get; set; }
    // El usuario que está mirando (para mostrar "YO" y "Vos")
    public int IdUsuario { get; set; }

    // "1 voto" o "3 votos"
    public string TextoVotos()
    {
        return Votos + (Votos == 1 ? " voto" : " votos");
    }
}
