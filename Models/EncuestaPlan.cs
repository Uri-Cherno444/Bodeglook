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

    // ---------- Textos del Detalle del plan (07h / 07i) ----------

    public bool EsCreador(int idUsuario)
    {
        return IdCreador == idUsuario;
    }

    public string TextoEtiqueta()
    {
        return EnVotacion() ? "PLAN EN VOTACIÓN" : Confirmado() ? "PLAN CONFIRMADO" : "PLAN CANCELADO";
    }

    public string TextoChip()
    {
        return EnVotacion() ? "En votación" : Confirmado() ? "Confirmado" : "Cancelado";
    }

    public string ClaseChip()
    {
        return EnVotacion() ? "chip-naranja" : Confirmado() ? "chip-confirmado" : "chip-cancelado";
    }

    // "Vinoteca · Cañuelas · Opciones sin TACC verificadas" (restricciones: las que cumple el lugar elegido)
    public string TextoLugarElegido(List<string> restricciones)
    {
        string texto = LugarElegido()?.TextoDetalle() ?? "";
        if (restricciones.Count > 0)
            texto += " · Opciones " + string.Join(" y ", restricciones.Select(r => char.ToLower(r[0]) + r.Substring(1))) + " verificadas";
        return texto;
    }

    // El texto de la tarjeta de acciones mientras se vota: cuántos faltan y si puede confirmar
    public string TextoAcciones(int idUsuario)
    {
        int faltan = CantidadIntegrantes - CantidadVotaron();
        bool esCreador = EsCreador(idUsuario);
        if (faltan <= 0)
            return esCreador ? "Todos votaron. Ya podés confirmar el lugar y el horario ganadores." : "Todos votaron. " + PrimerNombreCreador() + " confirma el lugar y el horario.";
        return (faltan == 1 ? "Falta que vote 1 persona." : "Faltan votar " + faltan + " personas.") + (esCreador ? " Podés confirmar igual." : "");
    }

    // Las opciones de lugar listas para dibujar. En votación se resalta lo que va ganando
    // (si hay empate, todos los empatados); confirmado, lo elegido.
    public List<OpcionDetalle> DetalleLugares(int idUsuario)
    {
        int maximo = Opciones.Count == 0 ? 0 : Opciones.Max(o => o.Votos);
        List<OpcionDetalle> lista = new List<OpcionDetalle>();
        foreach (OpcionPlan opcion in Opciones)
        {
            OpcionDetalle detalle = new OpcionDetalle();
            detalle.Nombre = opcion.Nombre ?? "";
            detalle.Detalle = opcion.TextoDetalle();
            detalle.Votos = opcion.Votos;
            detalle.Porcentaje = Porcentaje(opcion.Votos);
            detalle.Votantes = Participantes.Where(p => p.IdLugarVotado == opcion.IdRestaurante).ToList();
            detalle.Resaltada = Confirmado() ? opcion.IdRestaurante == IdRestauranteElegido : EnVotacion() && maximo > 0 && opcion.Votos == maximo;
            detalle.IdUsuario = idUsuario;
            lista.Add(detalle);
        }
        return lista;
    }

    // Igual que DetalleLugares, para los horarios
    public List<OpcionDetalle> DetalleHorarios(int idUsuario)
    {
        int maximo = Horarios.Count == 0 ? 0 : Horarios.Max(h => h.Votos);
        int? idElegido = HorarioElegido()?.IdHorario;
        List<OpcionDetalle> lista = new List<OpcionDetalle>();
        foreach (OpcionHorario horario in Horarios)
        {
            OpcionDetalle detalle = new OpcionDetalle();
            detalle.Nombre = horario.TextoHorario();
            detalle.Detalle = TextoDiaLargo();
            detalle.Votos = horario.Votos;
            detalle.Porcentaje = Porcentaje(horario.Votos);
            detalle.Votantes = Participantes.Where(p => p.IdHorarioVotado == horario.IdHorario).ToList();
            detalle.Resaltada = Confirmado() ? horario.IdHorario == idElegido : EnVotacion() && maximo > 0 && horario.Votos == maximo;
            detalle.IdUsuario = idUsuario;
            lista.Add(detalle);
        }
        return lista;
    }

    // ---------- Para el chat ----------

    // Lo que necesita chat.js para dibujar una encuesta (07), un plan confirmado (07e) o cancelado.
    // estado: "votacion", "confirmado" o "cancelado". La barra de cada opción es votos / integrantes del grupo.
    public object ParaJson()
    {
        string estado = Confirmado() ? "confirmado" : Cancelado() ? "cancelado" : "votacion";
        return new
        {
            id = Id,
            titulo = Titulo,
            estado,
            textoDia = TextoDia(),
            textoDiaYHora = TextoDiaYHora(),
            creador = PrimerNombreCreador(),
            integrantes = CantidadIntegrantes,
            votaron = CantidadVotaron(),
            miVoto = MiVoto,
            miVotoHorario = MiVotoHorario,
            idElegido = IdRestauranteElegido,
            idHorarioElegido = HorarioElegido()?.IdHorario,
            lugarElegido = LugarElegido()?.Nombre,
            opciones = Opciones.Select(o => new
            {
                idRestaurante = o.IdRestaurante,
                nombre = o.Nombre,
                foto = o.Foto,
                detalle = o.TextoDetalle(),
                votos = o.Votos,
                porcentaje = Porcentaje(o.Votos)
            }),
            horarios = Horarios.Select(h => new
            {
                idHorario = h.IdHorario,
                horario = h.TextoHorario(),
                votos = h.Votos,
                porcentaje = Porcentaje(h.Votos)
            })
        };
    }
}
