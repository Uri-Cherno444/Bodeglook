using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Bodeglook.Models;

public static class BD
{
    public static string CadenaConexion = "";

    public static bool ProbarConexion()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int resultado = conexion.QueryFirst<int>("ProbarConexion", commandType: CommandType.StoredProcedure);
            return resultado == 1;
        }
    }

    public static Usuarios? IniciarSesion(string email, string contraseña)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>("InicioSesionUsuario",
                new { Email = email, Contraseña = contraseña },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static int? RegistrarUsuario(Usuarios usuario, int idBarrio)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int?>("RegistrarUsuario",
                new { usuario.NombreCompleto, usuario.NombreUsuario, usuario.Email, usuario.Contraseña, idBarrio },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static Usuarios? ObtenerPerfilUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>("ObtenerPerfilUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static bool ActualizarPerfilUsuario(int idUsuario, string nombreCompleto, string? biografia, int idBarrio)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int error = conexion.QueryFirst<int>("ActualizarPerfilUsuario",
                new { idUsuario, NombreCompleto = nombreCompleto, Biografía = biografia, idBarrio },
                commandType: CommandType.StoredProcedure);
            return error == 0;
        }
    }

    public static int ContarResenasDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarResenasUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<Restricciones> ObtenerRestricciones()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Restricciones>("ListarRestricciones", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<int> ObtenerIdsRestricciones()
    {
        return ObtenerRestricciones().Select(r => r.Id).ToList();
    }

    public static List<int> ObtenerIdsRestriccionesPorNombre(List<string> nombres)
    {
        return ObtenerRestricciones().Where(r => nombres.Contains(r.Nombre!)).Select(r => r.Id).ToList();
    }

    public static List<Barrios> ObtenerBarrios()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Barrios>("ListarBarrios", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<int> ObtenerIdsBarrios()
    {
        return ObtenerBarrios().Select(b => b.Id).ToList();
    }

    public static Barrios? ObtenerBarrio(int? idBarrio)
    {
        return ObtenerBarrios().FirstOrDefault(b => b.Id == idBarrio);
    }

    public static List<Categorias> ObtenerCategorias()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Categorias>("ListarCategorias", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static bool ExisteCategoria(int idCategoria)
    {
        return ObtenerCategorias().Any(c => c.Id == idCategoria);
    }

    public static Dictionary<string, int> ObtenerIdsCategoriasPorNombre()
    {
        return ObtenerCategorias().ToDictionary(c => c.Nombre!, c => c.Id);
    }

    public static List<Barrios> ObtenerBarriosConCoordenadas()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Barrios>("ListarBarriosConCoordenadas", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<Barrios> ObtenerBarriosUbicados()
    {
        return ObtenerBarriosConCoordenadas().Where(b => b.Latitud != null && b.Longitud != null).ToList();
    }

    public static Barrios? ObtenerBarrioDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Barrios>("ObtenerBarrioUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static bool GuardarLugarGeoapify(LugarGeoapify lugar, int idBarrio, int idCategoria)
    {
        string nombre = lugar.Nombre!.Trim();
        if (nombre.Length > 100) nombre = nombre.Substring(0, 100);
        string calle = (lugar.Calle ?? "").Trim();
        if (calle.Length > 50) calle = calle.Substring(0, 50);

        var datos = new
        {
            IdGeoapify = lugar.IdGeoapify,
            Nombre = nombre,
            Calle = calle,
            Numero = lugar.NumeroEntero(),
            CodigoPostal = lugar.CodigoPostalEntero(),
            Latitud = lugar.Latitud,
            Longitud = lugar.Longitud,
            idBarrio,
            idCategoria
        };

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("GuardarLugarGeoapify", datos, commandType: CommandType.StoredProcedure);
        }
    }

    public static string? ObtenerOrigenRestaurante(int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<string>("ObtenerOrigenRestaurante",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static int RegistrarRestaurante(string nombre, string email, string contraseña, int idBarrio, int idCategoria, bool esPro)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("RegistrarRestaurante",
                new { Nombre = nombre, Email = email, Contraseña = contraseña, idBarrio, idCategoria, EsPro = esPro },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<Restricciones> ObtenerRestriccionesDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Restricciones>("ListarRestriccionesUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<int> ObtenerIdsRestriccionesDeUsuario(int idUsuario)
    {
        return ObtenerRestriccionesDeUsuario(idUsuario).Select(r => r.Id).ToList();
    }

    public static List<string> ObtenerNombresRestriccionesDeUsuario(int idUsuario)
    {
        return ObtenerRestriccionesDeUsuario(idUsuario).Select(r => r.Nombre!).ToList();
    }

    public static Usuarios? ObtenerDatosEdicion(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>("ObtenerDatosEdicionUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<int> ObtenerZonasDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<int>("ListarZonasUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<string> ObtenerCompaniasDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>("ListarCompaniasUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static bool ObtenerSoloApto(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<bool?>("ObtenerSoloAptoUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure) == true;
        }
    }

    public static void GuardarSoloApto(int idUsuario, bool soloApto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("GuardarSoloApto",
                new { idUsuario, soloApto },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static bool ExisteNombreUsuario(string nombreUsuario, int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("ExisteNombreUsuario",
                new { nombreUsuario, idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static void GuardarGustos(int idUsuario, DatosGustos gustos)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Open();
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                GuardarGustosEnTransaccion(conexion, transaccion, idUsuario, gustos);
                transaccion.Commit();
            }
        }
    }

    public static void GuardarPerfil(int idUsuario, string nombreCompleto, string nombreUsuario, int idBarrio,
                                     string? fotoNueva, DatosGustos gustos, bool soloApto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Open();
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                conexion.Execute("GuardarDatosPerfilUsuario",
                    new { idUsuario, nombreCompleto, nombreUsuario, idBarrio, fotoNueva, soloApto },
                    transaccion,
                    commandType: CommandType.StoredProcedure);
                GuardarGustosEnTransaccion(conexion, transaccion, idUsuario, gustos);
                transaccion.Commit();
            }
        }
    }

    private static void GuardarGustosEnTransaccion(SqlConnection conexion, SqlTransaction transaccion, int idUsuario, DatosGustos gustos)
    {
        conexion.Execute("GuardarFrecuenciaUsuario",
            new { idUsuario, frecuencia = gustos.Frecuencia }, transaccion,
            commandType: CommandType.StoredProcedure);

        conexion.Execute("BorrarRestriccionesUsuario", new { idUsuario }, transaccion,
            commandType: CommandType.StoredProcedure);
        foreach (int idRestriccion in gustos.Restricciones)
        {
            conexion.Execute("AgregarRestriccionUsuario",
                new { idUsuario, idRestriccion }, transaccion,
                commandType: CommandType.StoredProcedure);
        }

        conexion.Execute("BorrarZonasUsuario", new { idUsuario }, transaccion,
            commandType: CommandType.StoredProcedure);
        foreach (int idBarrio in gustos.Zonas)
        {
            conexion.Execute("AgregarZonaUsuario",
                new { idUsuario, idBarrio }, transaccion,
                commandType: CommandType.StoredProcedure);
        }

        conexion.Execute("BorrarCompaniasUsuario", new { idUsuario }, transaccion,
            commandType: CommandType.StoredProcedure);
        foreach (string compania in gustos.Companias)
        {
            conexion.Execute("AgregarCompaniaUsuario",
                new { idUsuario, compania }, transaccion,
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<Usuarios> ObtenerIntegrantesDeGrupo(int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            SqlMapper.GridReader resultado = conexion.QueryMultiple("ListarMiembrosGrupo",
                new { idGrupo },
                commandType: CommandType.StoredProcedure);
            List<Usuarios> integrantes = resultado.Read<Usuarios>().ToList();
            if (resultado.IsConsumed) return new List<Usuarios>();
            return integrantes;
        }
    }

    public static bool EsIntegranteDeGrupo(int idUsuario, int idGrupo)
    {
        return ObtenerIntegrantesDeGrupo(idGrupo).Any(integrante => integrante.Id == idUsuario);
    }

    public static List<ResumenChat> ObtenerChatsDeUsuario(int idUsuario)
    {
        List<ResumenChat> chats;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            chats = conexion.Query<ResumenChat>("ListarChatsUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }

        foreach (ResumenChat chat in chats)
        {
            List<string> nombres = new List<string>();
            foreach (Usuarios integrante in ObtenerIntegrantesDeGrupo(chat.IdGrupo))
            {
                if (integrante.Id != idUsuario) nombres.Add(integrante.ObtenerPrimerNombre());
            }
            nombres.Add("yo");
            chat.Integrantes = string.Join(", ", nombres);
        }
        return chats;
    }

    public static Grupos? ObtenerGrupo(int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Grupos>("ObtenerGrupo",
                new { idGrupo },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<MensajeChat> ObtenerMensajes(int idGrupo, int desdeId)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<MensajeChat>("ListarMensajesGrupo",
                new { idGrupo, desdeId },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static void GuardarMensaje(int idGrupo, int idUsuario, string contenido)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("GuardarMensaje",
                new { contenido, idGrupo, idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static void MarcarComoLeido(int idUsuario, int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("MarcarGrupoLeido",
                new { idUsuario, idGrupo },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static int CrearGrupo(string nombre, int idCreador, List<int> idsIntegrantes)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idGrupo = conexion.QueryFirst<int>("CrearGrupo",
                new { nombre },
                commandType: CommandType.StoredProcedure);

            conexion.Execute("AgregarUsuarioAGrupo",
                new { idUsuario = idCreador, idGrupo },
                commandType: CommandType.StoredProcedure);
            foreach (int idIntegrante in idsIntegrantes)
            {
                conexion.Execute("AgregarUsuarioAGrupo",
                    new { idUsuario = idIntegrante, idGrupo },
                    commandType: CommandType.StoredProcedure);
            }
            return idGrupo;
        }
    }

    public static void ActualizarFotoGrupo(int idGrupo, string foto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("ActualizarFotoGrupo",
                new { foto, idGrupo },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<Usuarios> ObtenerOtrosUsuarios(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Usuarios>("ListarOtrosUsuarios",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<int> ObtenerIdsOtrosUsuarios(int idUsuario)
    {
        return ObtenerOtrosUsuarios(idUsuario).Select(u => u.Id).ToList();
    }

    public static List<TarjetaPlan> ObtenerPlanesDeUsuario(int idUsuario, bool? confirmado)
    {
        List<TarjetaPlan> planes;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            planes = conexion.Query<TarjetaPlan>("ListarPlanesUsuario",
                new { idUsuario, confirmado },
                commandType: CommandType.StoredProcedure).ToList();
        }

        foreach (TarjetaPlan plan in planes)
        {
            foreach (Usuarios integrante in ObtenerIntegrantesDeGrupo(plan.IdGrupo))
            {
                if (integrante.Id != idUsuario) plan.Iniciales.Add(integrante.ObtenerIniciales());
            }
        }
        return planes;
    }

    public static int ContarPlanesEnVotacion(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarPlanesVotacionUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static int ContarAmigos(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarAmigosUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static Planes? ObtenerPlan(int idPlan)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Planes>("ObtenerPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static EncuestaPlan? ObtenerEncuesta(int idPlan, int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            EncuestaPlan? encuesta = conexion.QueryFirstOrDefault<EncuestaPlan>("ObtenerEncuestaPlan",
                new { idPlan, idUsuario },
                commandType: CommandType.StoredProcedure);
            if (encuesta == null) return null;
            encuesta.Opciones = conexion.Query<OpcionPlan>("ListarOpcionesPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure).ToList();
            encuesta.Horarios = conexion.Query<OpcionHorario>("ListarHorariosPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure).ToList();
            List<Participante> participantes = conexion.Query<Participante>("ListarParticipantesPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure).ToList();
            encuesta.Participantes = participantes.Where(p => p.IdUsuario == encuesta.IdCreador)
                .Concat(participantes.Where(p => p.IdUsuario != encuesta.IdCreador))
                .ToList();
            return encuesta;
        }
    }

    public static List<EncuestaPlan> ObtenerEncuestasDeGrupo(int idGrupo, int idUsuario)
    {
        List<int> idsPlanes;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            idsPlanes = conexion.Query<int>("ListarPlanesPublicadosGrupo",
                new { idGrupo },
                commandType: CommandType.StoredProcedure).ToList();
        }

        List<EncuestaPlan> encuestas = new List<EncuestaPlan>();
        foreach (int idPlan in idsPlanes)
        {
            EncuestaPlan? encuesta = ObtenerEncuesta(idPlan, idUsuario);
            if (encuesta != null) encuestas.Add(encuesta);
        }
        return encuestas;
    }

    public static List<LugarBuscado> BuscarLugaresParaGrupo(int idGrupo, string texto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<LugarBuscado>("BuscarLugaresParaGrupo",
                new { idGrupo, texto },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static LugarBuscado? ObtenerLugar(int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<LugarBuscado>("ObtenerLugar",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<string> ObtenerRestriccionesDeGrupo(int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>("ListarRestriccionesGrupo",
                new { idGrupo },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static int CrearPlan(string titulo, DateTime dia, int idGrupo, int idCreador, List<int> idsLugares, List<TimeSpan> horarios)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idPlan = conexion.QueryFirst<int>("CrearPlan",
                new { Titulo = titulo, FechaCierreVotacion = dia, FechaSalida = dia, idGrupo, idUsuarioCreador = idCreador },
                commandType: CommandType.StoredProcedure);

            foreach (int idRestaurante in idsLugares)
            {
                conexion.Execute("AgregarRestauranteAPlan",
                    new { idPlan, idRestaurante },
                    commandType: CommandType.StoredProcedure);
                foreach (TimeSpan horario in horarios)
                {
                    conexion.Execute("AgregarHorarioAPlan",
                        new { idPlan, idRestaurante, FechaHoraPropuesta = dia.Add(horario) },
                        commandType: CommandType.StoredProcedure);
                }
            }

            conexion.Execute("GuardarMensajePlan",
                new { titulo, idGrupo, idCreador, idPlan },
                commandType: CommandType.StoredProcedure);
            return idPlan;
        }
    }

    public static bool EsLugarDelPlan(int idPlan, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("EsLugarDelPlan",
                new { idPlan, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static int? ObtenerLugarVotado(int idPlan, int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<int?>("ObtenerLugarVotado",
                new { idPlan, idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static void Votar(int idPlan, int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int? idHorario = conexion.QueryFirstOrDefault<int?>("ObtenerHorarioVotadoEnLugar",
                new { idPlan, idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
            conexion.Execute("VotarPlanCompleto",
                new { idUsuario, idPlan, idRestaurante, idHorarioDisponibleVotado = idHorario },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }

    public static bool EsHorarioDelPlan(int idPlan, int idHorario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("EsHorarioDelPlan",
                new { idPlan, idHorario },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static void VotarHorario(int idPlan, int idUsuario, int idRestaurante, int idHorario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idFila = conexion.QueryFirst<int>("ObtenerHorarioDeLugar",
                new { idPlan, idRestaurante, idHorario },
                commandType: CommandType.StoredProcedure);
            conexion.Execute("VotarPlanCompleto",
                new { idUsuario, idPlan, idRestaurante, idHorarioDisponibleVotado = idFila },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }

    public static void ConfirmarPlan(int idPlan, int idRestaurante, DateTime fechaSalida)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("ConfirmarPlan",
                new { idPlan, idRestaurante, fechaSalida },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static void CancelarPlan(int idPlan)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("CancelarPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<TarjetaRestaurante> ObtenerMejorPuntuados(int cantidad, int idUsuario)
    {
        List<TarjetaRestaurante> tarjetas;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            tarjetas = conexion.Query<TarjetaRestaurante>("Mostrar4Restaurantes",
                new { cantidad },
                commandType: CommandType.StoredProcedure).ToList();
        }
        foreach (TarjetaRestaurante tarjeta in tarjetas)
        {
            CompletarTarjeta(tarjeta, idUsuario);
        }
        return tarjetas;
    }

    public static List<TarjetaRestaurante> ObtenerRestaurantesParaBuscar()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>("ListarRestaurantesBuscador",
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<Reseñas> ObtenerPuntajesDeResenas()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Reseñas>("ListarPuntajesResenas",
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<RestriccionesRestaurantes> ObtenerRestriccionesDeTodosLosRestaurantes()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<RestriccionesRestaurantes>("ListarTodasRestriccionesRestaurantes",
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Agrupa por restaurante: restricciones[idRestaurante] da los ids de las restricciones que cumple
    public static ILookup<int, int> ObtenerRestriccionesPorRestaurante()
    {
        return ObtenerRestriccionesDeTodosLosRestaurantes()
            .ToLookup(r => r.IdRestaurante ?? 0, r => r.IdRestriccion ?? 0);
    }

    public static HashSet<int> ObtenerIdsFavoritos(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<int>("ListarFavoritosUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToHashSet();
        }
    }

    public static void GuardarBusqueda(int idUsuario, string texto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("GuardarBusqueda",
                new { idUsuario, texto },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<string> ObtenerBusquedasRecientes(int idUsuario, int cantidad)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>("ListarBusquedasRecientes",
                new { idUsuario, cantidad },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static bool ExisteRestaurante(int idRestaurante)
    {
        return ObtenerRestaurante(idRestaurante) != null;
    }

    private static DetalleRestaurante? ObtenerRestaurante(int idRestaurante)
    {
        DetalleRestaurante? detalle;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            detalle = conexion.QueryFirstOrDefault<DetalleRestaurante>("ObtenerRestaurante",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure);
        }

        if (detalle != null) detalle.Puntaje = detalle.Promedio;
        return detalle;
    }

    public static bool EsFavorito(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("ComprobarEsFavorito",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    private static void CompletarTarjeta(TarjetaRestaurante tarjeta, int idUsuario)
    {
        DetalleRestaurante? restaurante = ObtenerRestaurante(tarjeta.Id);
        if (restaurante != null)
        {
            tarjeta.Categoria = restaurante.Categoria;
            tarjeta.Barrio = restaurante.Barrio;
            tarjeta.Puntaje = restaurante.Puntaje;
        }
        tarjeta.EsFavorito = idUsuario > 0 && EsFavorito(idUsuario, tarjeta.Id);
    }

    public static DetalleRestaurante? ObtenerDetalleRestaurante(int idRestaurante, int idUsuario)
    {
        DetalleRestaurante? detalle = ObtenerRestaurante(idRestaurante);
        if (detalle == null) return null;

        detalle.EsFavorito = EsFavorito(idUsuario, idRestaurante);
        detalle.Restricciones = ObtenerRestriccionesDeRestaurante(idRestaurante);
        return detalle;
    }

    public static List<string> ObtenerRestriccionesDeRestaurante(int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            List<Restricciones> restricciones = conexion.Query<Restricciones>("ListarRestriccionesRestaurante",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure).ToList();
            return restricciones.Select(restriccion => restriccion.Nombre!).ToList();
        }
    }

    public static DetalleRestaurante ObtenerPuntajeRestaurante(int idRestaurante)
    {
        return ObtenerRestaurante(idRestaurante) ?? new DetalleRestaurante();
    }

    public static bool YaReseno(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("ComprobarYaReseno",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static Planes? ObtenerVisitaVerificada(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Planes>("ObtenerVisitaVerificada",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<ResenaComunidad> ObtenerResenasDeRestaurante(int idRestaurante, int desde, int? cantidad)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<ResenaComunidad>("ListarResenasRestaurante",
                new { idRestaurante, desde, cantidad },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static int GuardarResena(Reseñas resena)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("CrearResena",
                new { resena.IdUsuario, resena.IdRestaurante, resena.Puntaje, resena.Comentario, resena.Foto },
                commandType: CommandType.StoredProcedure);
        }
    }

    public static List<int> ObtenerRestaurantesConRestriccion(int idRestriccion)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<int>("ListarRestaurantesConRestriccion",
                new { idRestriccion },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    public static List<TarjetaRestaurante> ObtenerFavoritos(int idUsuario)
    {
        List<TarjetaRestaurante> favoritos = new List<TarjetaRestaurante>();
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            using (IDataReader lector = conexion.ExecuteReader("MostrarFavoritos",
                new { idUsuario },
                commandType: CommandType.StoredProcedure))
            {
                int columnaId = lector.GetOrdinal("ID");
                int columnaNombre = lector.GetOrdinal("Nombre");
                int columnaFoto = lector.GetOrdinal("Foto");
                int columnaBarrio = lector.FieldCount - 2;
                int columnaCategoria = lector.FieldCount - 1;
                while (lector.Read())
                {
                    TarjetaRestaurante favorito = new TarjetaRestaurante();
                    favorito.Id = lector.GetInt32(columnaId);
                    favorito.Nombre = lector.GetString(columnaNombre);
                    favorito.Foto = lector.IsDBNull(columnaFoto) ? null : lector.GetString(columnaFoto);
                    favorito.Barrio = lector.GetString(columnaBarrio);
                    favorito.Categoria = lector.GetString(columnaCategoria);
                    favorito.EsFavorito = true;
                    favoritos.Add(favorito);
                }
            }
        }

        foreach (TarjetaRestaurante favorito in favoritos)
        {
            favorito.Puntaje = ObtenerRestaurante(favorito.Id)?.Puntaje;
        }
        return favoritos;
    }

    public static bool AlternarFavorito(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            if (EsFavorito(idUsuario, idRestaurante))
            {
                conexion.Execute("QuitarFavorito",
                    new { idUsuario, idRestaurante },
                    commandType: CommandType.StoredProcedure);
                return false;
            }

            conexion.Execute("AgregarFavorito",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
            return true;
        }
    }

    private static List<string>? palabrasProhibidas = null;

    public static bool TieneLenguajeOfensivo(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;

        if (palabrasProhibidas == null)
        {
            using (SqlConnection conexion = new SqlConnection(CadenaConexion))
            {
                palabrasProhibidas = conexion.Query<string>("ListarPalabrasProhibidas", commandType: CommandType.StoredProcedure)
                    .Select(palabra => NormalizarTexto(palabra).Trim())
                    .Where(palabra => palabra != "")
                    .ToList();
            }
        }

        string[] partes = NormalizarTexto(texto).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        List<string> palabras = new List<string>(partes);

        string juntas = "";
        foreach (string parte in partes.Append(""))
        {
            if (parte.Length == 1)
            {
                juntas += parte;
            }
            else
            {
                if (juntas.Length >= 2) palabras.Add(juntas);
                juntas = "";
            }
        }

        foreach (string palabra in palabras)
        {
            List<string> formas = new List<string> { palabra };
            if (palabra.Length > 2 && palabra.EndsWith("s")) formas.Add(palabra.Substring(0, palabra.Length - 1));
            if (palabra.Length > 3 && palabra.EndsWith("es")) formas.Add(palabra.Substring(0, palabra.Length - 2));

            foreach (string forma in formas)
            {
                if (palabrasProhibidas.Any(prohibida => EsLaMismaPalabra(forma, prohibida))) return true;
            }
        }
        return false;
    }

    private static string NormalizarTexto(string texto)
    {
        string resultado = "";
        foreach (char original in texto.ToLower())
        {
            char letra = original;
            if (letra == '4' || letra == '@') letra = 'a';
            else if (letra == '3') letra = 'e';
            else if (letra == '1') letra = 'i';
            else if (letra == '0') letra = 'o';
            else if (letra == '$') letra = 's';
            else if (letra != 'ñ') letra = letra.ToString().Normalize(System.Text.NormalizationForm.FormD)[0];

            resultado += char.IsLetter(letra) ? letra : ' ';
        }
        return resultado;
    }

    private static bool EsLaMismaPalabra(string palabra, string prohibida)
    {
        int i = 0;
        int j = 0;
        while (i < palabra.Length && j < prohibida.Length)
        {
            if (palabra[i] != prohibida[j]) return false;

            char letra = palabra[i];
            int vecesEnPalabra = 0;
            while (i < palabra.Length && palabra[i] == letra) { vecesEnPalabra++; i++; }
            int vecesEnProhibida = 0;
            while (j < prohibida.Length && prohibida[j] == letra) { vecesEnProhibida++; j++; }

            if (vecesEnPalabra < vecesEnProhibida) return false;
        }
        return i == palabra.Length && j == prohibida.Length;
    }
}
