namespace Bodeglook.Models;

public class DetalleRestaurante
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    private string? _foto;
    public string? Foto
    {
        get { return Fotos.Portada(Id, _foto); }
        set { _foto = value; }
    }
    public List<string> Galeria()
    {
        return Fotos.Galeria(Id, _foto);
    }
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public string? DireccionCalle { get; set; }
    public int DireccionNumero { get; set; }
    public int DireccionCodigoPostal { get; set; }
    public string? Telefono { get; set; }
    public DateTime HorarioApertura { get; set; }
    public DateTime HorarioCerrado { get; set; }
    public double? Puntaje { get; set; }
    public double? Promedio { get; set; }
    public int CantidadResenas { get; set; }
    public bool EsFavorito { get; set; }
    public List<string> Restricciones { get; set; } = new List<string>();

    public string TextoCantidadResenas()
    {
        if (CantidadResenas == 0) return "Sin reseñas todavía";
        return CantidadResenas == 1 ? "(1 reseña)" : "(" + CantidadResenas + " reseñas)";
    }

    public string TextoHorario()
    {
        if (HorarioApertura.TimeOfDay == HorarioCerrado.TimeOfDay) return "";
        return HorarioApertura.ToString("HH:mm") + " - " + HorarioCerrado.ToString("HH:mm");
    }

    public string TextoTelefono()
    {
        string numero = (Telefono ?? "").Trim();
        if (numero == "" || numero == "0") return "";
        if (numero.Length != 10 || !numero.All(char.IsDigit)) return numero;
        return "+54 " + numero.Substring(0, 2) + " " + numero.Substring(2, 4) + "-" + numero.Substring(6, 4);
    }

    public string TextoDireccion()
    {
        if (string.IsNullOrWhiteSpace(DireccionCalle)) return "";
        if (DireccionNumero <= 0) return DireccionCalle;
        return DireccionCalle + " " + DireccionNumero;
    }

    public string TextoLugarModal()
    {
        if (Barrio == null) return Nombre ?? "";
        return Nombre + " · " + Barrio.Split(',')[0];
    }
}
