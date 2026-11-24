namespace Bodeglook.Models;

public class Restricciones
{
    public int Id { get; set; }
    public string? Nombre { get; set; }

    public static string UnirConY(List<string> nombres)
    {
        if (nombres.Count == 0) return "";
        if (nombres.Count == 1) return nombres[0];
        return string.Join(", ", nombres.Take(nombres.Count - 1)) + " y " + nombres[nombres.Count - 1];
    }

    public static int? ValidarElegida(int? idRestriccion, List<Restricciones> restricciones)
    {
        if (!restricciones.Any(r => r.Id == idRestriccion)) return null;
        return idRestriccion;
    }
}
