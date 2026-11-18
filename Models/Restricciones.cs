namespace Bodeglook.Models;

public class Restricciones
{
    public int Id { get; set; }
    public string? Nombre { get; set; }

    // ["Sin TACC"] -> "Sin TACC"; ["Sin TACC", "Vegano", "Kosher"] -> "Sin TACC, Vegano y Kosher"
    public static string UnirConY(List<string> nombres)
    {
        if (nombres.Count == 0) return "";
        if (nombres.Count == 1) return nombres[0];
        return string.Join(", ", nombres.Take(nombres.Count - 1)) + " y " + nombres[nombres.Count - 1];
    }

    // El chip elegido en Inicio: si ese Id no existe se toma como "Todos" (null)
    public static int? ValidarElegida(int? idRestriccion, List<Restricciones> restricciones)
    {
        if (!restricciones.Any(r => r.Id == idRestriccion)) return null;
        return idRestriccion;
    }
}
