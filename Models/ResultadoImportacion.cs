namespace Bodeglook.Models;

public class ResultadoImportacion
{
    public string? Barrio { get; set; }
    public int Traidos { get; set; }
    public int Nuevos { get; set; }
    public int Actualizados { get; set; }
    public int SinNombre { get; set; }
    public int SinCategoria { get; set; }
    public int ConError { get; set; }
    public string? PrimerError { get; set; }
    public string? Aviso { get; set; }
}
