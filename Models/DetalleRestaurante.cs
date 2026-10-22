using System.Globalization;

namespace Bodeglook.Models;

// No es una tabla: junta lo que muestra 04 · Detalle + Reseña
// (Restaurantes, Categorias, Barrios, el promedio de Reseñas y si es favorito del usuario).
public class DetalleRestaurante
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Foto { get; set; }
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public string? DireccionCalle { get; set; }
    public int DireccionNumero { get; set; }
    public int DireccionCodigoPostal { get; set; }
    public int Telefono { get; set; }
    public DateTime HorarioApertura { get; set; }
    public DateTime HorarioCerrado { get; set; }
    public double? Puntaje { get; set; }
    public int CantidadResenas { get; set; }
    public bool EsFavorito { get; set; }
    public List<string> Restricciones { get; set; } = new List<string>();

    // 4.666 -> "4.7"
    public string TextoPuntaje()
    {
        if (Puntaje == null) return "";
        return Puntaje.Value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    // "(1 reseña)" o "(128 reseñas)"
    public string TextoCantidadResenas()
    {
        return CantidadResenas == 1 ? "(1 reseña)" : "(" + CantidadResenas + " reseñas)";
    }

    // "12:00 - 23:00". Si apertura y cierre son iguales es que no hay dato.
    public string TextoHorario()
    {
        if (HorarioApertura.TimeOfDay == HorarioCerrado.TimeOfDay) return "";
        return HorarioApertura.ToString("HH:mm") + " - " + HorarioCerrado.ToString("HH:mm");
    }

    // 1155551234 -> "+54 11 5555-1234". 0 es que no hay dato.
    public string TextoTelefono()
    {
        if (Telefono <= 0) return "";
        string numero = Telefono.ToString();
        if (numero.Length != 10) return numero;
        return "+54 " + numero.Substring(0, 2) + " " + numero.Substring(2, 4) + "-" + numero.Substring(6, 4);
    }

    // "Av. Siempreviva 742". Si no hay calle cargada, no hay dato.
    public string TextoDireccion()
    {
        if (string.IsNullOrWhiteSpace(DireccionCalle)) return "";
        if (DireccionNumero <= 0) return DireccionCalle;
        return DireccionCalle + " " + DireccionNumero;
    }
}
