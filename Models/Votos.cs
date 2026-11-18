namespace Bodeglook.Models;

// Un voto por persona y por plan: el lugar y el horario van en la misma fila
public class Votos
{
    public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdPlan { get; set; }
    public int? IdRestaurante { get; set; }
    // La fila de PlanesHorarios del lugar votado. null si todavía no votó horario.
    public int? IdHorarioVotado { get; set; }

    // Votar el lugar desde la encuesta ({ idPlan, idRestaurante }). Se puede cambiar mientras esté En votación.
    // Devuelve { ok, mensaje }, o null si quien vota no es del grupo (el controller responde 403).
    public object? VotarLugar(int idUsuario)
    {
        Planes? plan = BD.ObtenerPlan(IdPlan ?? 0);
        if (plan == null || !BD.EsIntegranteDeGrupo(idUsuario, plan.IdGrupo ?? 0)) return null;
        if (plan.Estado != 0) return new { ok = false, mensaje = "La votación ya terminó." };

        int idRestaurante = IdRestaurante ?? 0;
        if (!BD.EsLugarDelPlan(plan.Id, idRestaurante)) return new { ok = false, mensaje = "Ese lugar no está en el plan." };

        BD.Votar(plan.Id, idUsuario, idRestaurante);
        return new { ok = true };
    }

    // Votar el horario ({ idPlan, idHorarioVotado }). Igual que el lugar: se puede cambiar mientras esté En votación.
    // Votos guarda lugar y horario juntos, así que primero hay que votar un lugar.
    // Devuelve { ok, mensaje }, o null si quien vota no es del grupo (el controller responde 403).
    public object? VotarHorario(int idUsuario)
    {
        Planes? plan = BD.ObtenerPlan(IdPlan ?? 0);
        if (plan == null || !BD.EsIntegranteDeGrupo(idUsuario, plan.IdGrupo ?? 0)) return null;
        if (plan.Estado != 0) return new { ok = false, mensaje = "La votación ya terminó." };

        int idHorario = IdHorarioVotado ?? 0;
        if (!BD.EsHorarioDelPlan(plan.Id, idHorario)) return new { ok = false, mensaje = "Ese horario no está en el plan." };

        int? idLugar = BD.ObtenerLugarVotado(plan.Id, idUsuario);
        if (idLugar == null) return new { ok = false, mensaje = "Votá un lugar para elegir el horario." };

        BD.VotarHorario(plan.Id, idUsuario, idLugar.Value, idHorario);
        return new { ok = true };
    }
}
