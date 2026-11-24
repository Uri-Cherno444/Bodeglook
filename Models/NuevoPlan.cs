using System.Globalization;

namespace Bodeglook.Models;

public class NuevoPlan
{
    public int IdGrupo { get; set; }
    public string? Titulo { get; set; }
    public string? Dia { get; set; }
    public List<string> Horarios { get; set; } = new List<string>();
    public List<int?> Lugares { get; set; } = new List<int?>();

    public static readonly string[] HorariosPosibles = { "20:00", "20:30", "21:00", "21:30", "22:00" };

    public object? Crear(int idUsuario)
    {
        if (!BD.EsIntegranteDeGrupo(idUsuario, IdGrupo)) return null;

        string titulo = (Titulo ?? "").Trim();
        if (titulo == "") return new { ok = false, campo = "nombre", mensaje = "Poné el nombre del plan." };
        if (titulo.Length > 50) return new { ok = false, campo = "nombre", mensaje = "El nombre puede tener hasta 50 caracteres." };

        List<int> lugares = new List<int>();
        for (int i = 0; i < Lugares.Count && i < 3; i++)
        {
            int? idLugar = Lugares[i];
            if (idLugar == null) continue;
            if (!BD.ExisteRestaurante(idLugar.Value))
                return new { ok = false, campo = "lugar" + (i + 1), mensaje = "Elegí un lugar de la lista." };
            if (lugares.Contains(idLugar.Value))
                return new { ok = false, campo = "lugar" + (i + 1), mensaje = "Ese lugar ya está en otra opción." };
            lugares.Add(idLugar.Value);
        }
        if (lugares.Count < 2)
        {
            int vacio = 1;
            while (vacio <= Lugares.Count && Lugares[vacio - 1] != null) vacio++;
            return new { ok = false, campo = "lugar" + Math.Min(vacio, 3), mensaje = "Elegí al menos 2 lugares para votar." };
        }

        if (!DateTime.TryParseExact(Dia, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dia))
            return new { ok = false, campo = "dia", mensaje = "Elegí un día." };
        if (dia < DateTime.Today)
            return new { ok = false, campo = "dia", mensaje = "Elegí un día que todavía no pasó." };

        List<TimeSpan> horarios = new List<TimeSpan>();
        foreach (string texto in Horarios)
        {
            if (TimeSpan.TryParseExact(texto, "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan horario) && !horarios.Contains(horario))
                horarios.Add(horario);
        }
        if (horarios.Count < 2) return new { ok = false, campo = "horarios", mensaje = "Marcá al menos 2 horarios para votar." };
        if (horarios.Count > 3) return new { ok = false, campo = "horarios", mensaje = "Podés marcar hasta 3 horarios." };
        if (horarios.Any(h => dia.Add(h) <= DateTime.Now))
            return new { ok = false, campo = "horarios", mensaje = "Hay horarios que ya pasaron. Elegí otros u otro día." };

        BD.CrearPlan(titulo, dia, IdGrupo, idUsuario, lugares, horarios.OrderBy(h => h).ToList());
        return new { ok = true };
    }
}
