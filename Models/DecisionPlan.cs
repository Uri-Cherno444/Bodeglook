namespace Bodeglook.Models;

// No es una tabla: lo que manda el creador al confirmar ({ idPlan, idRestaurante, idHorario })
// o al cancelar ({ idPlan }) desde el Detalle del plan.
public class DecisionPlan
{
    public int IdPlan { get; set; }
    public int IdRestaurante { get; set; }
    public int IdHorario { get; set; }

    // "Confirmar lugar y horario": solo el creador, aunque no hayan votado todos.
    // Gana el más votado de cada uno; si hay empate, elige el creador entre los empatados.
    // Devuelve { ok, mensaje }, o null si no es el creador o no es del grupo (el controller responde 403).
    public object? Confirmar(int idUsuario)
    {
        EncuestaPlan? encuesta = BD.ObtenerEncuesta(IdPlan, idUsuario);
        if (encuesta == null || !BD.EsIntegranteDeGrupo(idUsuario, encuesta.IdGrupo)) return null;
        if (encuesta.IdCreador != idUsuario) return null;
        if (!encuesta.EnVotacion()) return new { ok = false, mensaje = "La votación ya terminó." };

        if (!encuesta.LugaresMasVotados().Any(o => o.IdRestaurante == IdRestaurante))
            return new { ok = false, mensaje = "Solo podés confirmar el lugar con más votos." };
        OpcionHorario? horario = encuesta.HorariosMasVotados().FirstOrDefault(h => h.IdHorario == IdHorario);
        if (horario == null)
            return new { ok = false, mensaje = "Solo podés confirmar el horario con más votos." };

        BD.ConfirmarPlan(encuesta.Id, IdRestaurante, horario.FechaHoraPropuesta);
        return new { ok = true };
    }

    // "Cancelar plan": solo el creador y solo mientras esté En votación.
    // Devuelve { ok, mensaje }, o null si no es el creador o no es del grupo (el controller responde 403).
    public object? Cancelar(int idUsuario)
    {
        Planes? plan = BD.ObtenerPlan(IdPlan);
        if (plan == null || !BD.EsIntegranteDeGrupo(idUsuario, plan.IdGrupo ?? 0)) return null;
        if (plan.IdUsuarioCreador != idUsuario) return null;
        if (plan.Estado != 0) return new { ok = false, mensaje = "La votación ya terminó." };

        BD.CancelarPlan(plan.Id);
        return new { ok = true };
    }
}
