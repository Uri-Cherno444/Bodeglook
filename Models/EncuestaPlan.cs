namespace Bodeglook.Models;

// No es una tabla: todo lo que muestra un plan en la encuesta del chat (07 y 07e) y en su Detalle (07h y 07i).
public class EncuestaPlan
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    // 0 = En votación, 1 = Confirmado, 2 = Cancelado
    public int Estado { get; set; }
    // Mientras se vota es solo el día; confirmado lleva también el horario ganador
    public DateTime FechaSalida { get; set; }
    public int IdGrupo { get; set; }
    public string? NombreGrupo { get; set; }
    // Planes.idUsuarioCreador
    public int? IdCreador { get; set; }
    public string? NombreCreador { get; set; }
    public int? IdRestauranteElegido { get; set; }

    public List<OpcionPlan> Opciones { get; set; } = new List<OpcionPlan>();
    public List<OpcionHorario> Horarios { get; set; } = new List<OpcionHorario>();
    // Todos los integrantes del grupo con lo que votó cada uno
    public List<Participante> Participantes { get; set; } = new List<Participante>();
    public int CantidadIntegrantes { get; set; }
    // Lo que votó el usuario que está mirando (null si todavía no votó)
    public int? MiVoto { get; set; }
    public int? MiVotoHorario { get; set; }

    public bool EnVotacion() { return Estado == 0; }
    public bool Confirmado() { return Estado == 1; }
    public bool Cancelado() { return Estado == 2; }

    // "X de N votaron" cuenta solo a quien votó lugar y horario
    public int CantidadVotaron()
    {
        return Participantes.Count(p => p.VotoTodo());
    }

    // "Sáb 24/10"
    public string TextoDia()
    {
        string[] dias = { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };
        return dias[(int)FechaSalida.DayOfWeek] + " " + FechaSalida.ToString("dd/MM");
    }

    // "Sábado 24/10" (debajo de cada horario en el Detalle)
    public string TextoDiaLargo()
    {
        string[] dias = { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
        return dias[(int)FechaSalida.DayOfWeek] + " " + FechaSalida.ToString("dd/MM");
    }

    // "Sáb 24/10 a las 21:00" (cuando ya está confirmado)
    public string TextoDiaYHora()
    {
        return TextoDia() + " a las " + FechaSalida.ToString("HH:mm");
    }

    // "Cami" (el primer nombre de quien lo creó)
    public string PrimerNombreCreador()
    {
        return new Usuarios { NombreCompleto = NombreCreador }.ObtenerPrimerNombre();
    }

    // Los lugares con más votos. Si hay uno solo, ese gana; si hay varios, hay empate y elige el creador.
    public List<OpcionPlan> LugaresMasVotados()
    {
        int maximo = Opciones.Count == 0 ? 0 : Opciones.Max(o => o.Votos);
        return Opciones.Where(o => o.Votos == maximo).ToList();
    }

    public List<OpcionHorario> HorariosMasVotados()
    {
        int maximo = Horarios.Count == 0 ? 0 : Horarios.Max(h => h.Votos);
        return Horarios.Where(h => h.Votos == maximo).ToList();
    }

    public OpcionPlan? LugarElegido()
    {
        return Opciones.FirstOrDefault(o => o.IdRestaurante == IdRestauranteElegido);
    }

    // Planes no guarda el horario elegido: al confirmar, FechaSalida queda con el día y la hora ganadora
    public OpcionHorario? HorarioElegido()
    {
        if (!Confirmado()) return null;
        return Horarios.FirstOrDefault(h => h.FechaHoraPropuesta == FechaSalida);
    }

    // Barra de cada opción: votos / integrantes del grupo
    public int Porcentaje(int votos)
    {
        return CantidadIntegrantes == 0 ? 0 : votos * 100 / CantidadIntegrantes;
    }
}
