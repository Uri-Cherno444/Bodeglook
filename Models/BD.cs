using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Bodeglook.Models;

public static class BD
{
    // Se carga en Program.cs desde appsettings.json (ConnectionStrings:Bodeglook)
    public static string CadenaConexion = "";

    public static bool ProbarConexion()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int resultado = conexion.QueryFirst<int>("ProbarConexion", commandType: CommandType.StoredProcedure);
            return resultado == 1;
        }
    }

    // ---------- Usuarios ----------

    // Procedimiento InicioSesionUsuario. Compara el email y la contraseña ya hasheada.
    // Devuelve null si no coinciden.
    public static Usuarios? IniciarSesion(string email, string contraseña)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>("InicioSesionUsuario",
                new { Email = email, Contraseña = contraseña },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento RegistrarUsuario. La contraseña ya tiene que venir hasheada.
    // Devuelve null si se guardó, 1 si el nombre de usuario ya existe o 2 si el email ya se usó.
    public static int? RegistrarUsuario(Usuarios usuario, int idBarrio)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int?>("RegistrarUsuario",
                new { usuario.NombreCompleto, usuario.NombreUsuario, usuario.Email, usuario.Contraseña, idBarrio },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ---------- 08 · Perfil ----------

    // Procedimiento ObtenerPerfilUsuario. Trae el usuario con el nombre de su barrio (campo Barrio).
    public static Usuarios? ObtenerPerfilUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>("ObtenerPerfilUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ActualizarPerfilUsuario. Devuelve true si se guardó, false si el barrio no existe.
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

    // Procedimiento ContarResenasUsuario: todas las reseñas que escribió, publicadas o no
    public static int ContarResenasDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarResenasUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ---------- Onboarding (gustos y filtro dietario) ----------

    // Procedimiento ListarRestricciones: son los chips de "Restricciones dietarias"
    public static List<Restricciones> ObtenerRestricciones()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Restricciones>("ListarRestricciones", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ListarBarrios: son los chips de "Zonas que te interesan"
    public static List<Barrios> ObtenerBarrios()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Barrios>("ListarBarrios", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ListarCategorias: las opciones del desplegable "Categoría"
    public static List<Categorias> ObtenerCategorias()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Categorias>("ListarCategorias", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ListarBarriosConCoordenadas: los barrios con el centro (Latitud y Longitud),
    // para ubicar los lugares nuevos y armar los límites de la importación
    public static List<Barrios> ObtenerBarriosConCoordenadas()
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Barrios>("ListarBarriosConCoordenadas", commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ObtenerBarrioUsuario: el barrio del perfil del usuario, con su centro. En Descubrir se usa para
    // medir las distancias cuando el navegador no da la ubicación. null si el usuario no tiene barrio.
    public static Barrios? ObtenerBarrioDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Barrios>("ObtenerBarrioUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ---------- Lugares de Geoapify ----------

    // Procedimiento GuardarLugarGeoapify. Guarda un lugar traído de Geoapify. Si ya existe (mismo IdGeoapify) lo actualiza en vez de duplicarlo.
    // Al actualizar no se pisan los datos que se pudieron completar a mano (teléfono, web, foto, descripción, plan Pro).
    // Las columnas que no aceptan NULL y que Geoapify no trae van vacías: '' y 0, y los horarios iguales (= "sin dato").
    // Devuelve true si el lugar era nuevo y false si se actualizó.
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

    // Procedimiento ObtenerOrigenRestaurante: de dónde salió el lugar: "Manual" o "Geoapify"
    // (decide si se muestran los créditos). null si no existe.
    public static string? ObtenerOrigenRestaurante(int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<string>("ObtenerOrigenRestaurante",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento RegistrarRestaurante. La contraseña ya tiene que venir hasheada.
    // Devuelve 0 si se guardó, 1 si el email ya se usó, 2 si el barrio no existe o 3 si la categoría no existe.
    public static int RegistrarRestaurante(string nombre, string email, string contraseña, int idBarrio, int idCategoria, bool esPro)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("RegistrarRestaurante",
                new { Nombre = nombre, Email = email, Contraseña = contraseña, idBarrio, idCategoria, EsPro = esPro },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ListarRestriccionesUsuario: las restricciones que marcó el usuario
    public static List<Restricciones> ObtenerRestriccionesDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Restricciones>("ListarRestriccionesUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // ---------- 08c · Editar perfil y gustos ----------

    // Procedimiento ObtenerDatosEdicionUsuario: lo que carga el modal Editar perfil:
    // nombre, usuario, foto, barrio, frecuencia y "solo lo apto"
    public static Usuarios? ObtenerDatosEdicion(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>("ObtenerDatosEdicionUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ListarZonasUsuario: los Id de los barrios que le interesan al usuario
    public static List<int> ObtenerZonasDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<int>("ListarZonasUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ListarCompaniasUsuario: con quién sale: "Amigos", "Pareja", etc.
    public static List<string> ObtenerCompaniasDeUsuario(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>("ListarCompaniasUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ObtenerSoloAptoUsuario: "Solo lo apto para mí". Si todavía no eligió (null) se muestra todo.
    public static bool ObtenerSoloApto(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<bool?>("ObtenerSoloAptoUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure) == true;
        }
    }

    // Procedimiento GuardarSoloApto. Paso 2 del onboarding (02d): guarda "Solo lo apto para mí" (true) o "Ver todo" (false)
    public static void GuardarSoloApto(int idUsuario, bool soloApto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("GuardarSoloApto",
                new { idUsuario, soloApto },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ExisteNombreUsuario: true si otro usuario (no el idUsuario) ya tiene ese nombre de usuario
    public static bool ExisteNombreUsuario(string nombreUsuario, int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("ExisteNombreUsuario",
                new { nombreUsuario, idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Paso 1 del onboarding (02c): frecuencia, zonas, con quién sale y restricciones.
    // No toca "solo lo apto" (eso es el paso 2).
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

    // Guarda el modal Editar perfil entero o nada: datos, foto (solo si cambió), gustos y "solo lo apto".
    public static void GuardarPerfil(int idUsuario, string nombreCompleto, string nombreUsuario, int idBarrio,
                                     string? fotoNueva, DatosGustos gustos, bool soloApto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Open();
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                // Procedimiento GuardarDatosPerfilUsuario
                conexion.Execute("GuardarDatosPerfilUsuario",
                    new { idUsuario, nombreCompleto, nombreUsuario, idBarrio, fotoNueva, soloApto },
                    transaccion,
                    commandType: CommandType.StoredProcedure);
                GuardarGustosEnTransaccion(conexion, transaccion, idUsuario, gustos);
                transaccion.Commit();
            }
        }
    }

    // Borra lo que tenía el usuario y guarda lo nuevo (las 4 listas de gustos)
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

    // ---------- Grupos y chats ----------

    // Procedimiento ListarMiembrosGrupo (sin la contraseña). Si el grupo no existe el procedimiento
    // solo devuelve el código de error, y acá queda una lista vacía.
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

    // Los grupos del usuario con la fecha del último mensaje
    // y cuántos mensajes no leyó (usa UsuariosGrupos.UltimaLectura).
    public static List<ResumenChat> ObtenerChatsDeUsuario(int idUsuario)
    {
        // Procedimiento ListarChatsUsuario: trae cada grupo con la fecha de su último mensaje y los no leídos
        // (mensajes de otros posteriores a su UltimaLectura). El chat con el mensaje más nuevo va primero.
        List<ResumenChat> chats;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            chats = conexion.Query<ResumenChat>("ListarChatsUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }

        // Debajo del nombre del grupo van los integrantes: "Cami, Sofi, yo"
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

    // Procedimiento ObtenerGrupo
    public static Grupos? ObtenerGrupo(int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Grupos>("ObtenerGrupo",
                new { idGrupo },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ListarMensajesGrupo: los mensajes del grupo con Id mayor a desdeId, del más viejo al más nuevo.
    // Con desdeId = 0 trae todos; el chat después pide solo los nuevos.
    public static List<MensajeChat> ObtenerMensajes(int idGrupo, int desdeId)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<MensajeChat>("ListarMensajesGrupo",
                new { idGrupo, desdeId },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento GuardarMensaje
    public static void GuardarMensaje(int idGrupo, int idUsuario, string contenido)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("GuardarMensaje",
                new { contenido, idGrupo, idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento MarcarGrupoLeido: al abrir el chat (y cada vez que llegan mensajes con el chat abierto)
    // sus no leídos quedan en cero
    public static void MarcarComoLeido(int idUsuario, int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("MarcarGrupoLeido",
                new { idUsuario, idGrupo },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento CrearGrupo: crea el grupo con el creador y los integrantes que sumó. Devuelve el Id nuevo.
    public static int CrearGrupo(string nombre, int idCreador, List<int> idsIntegrantes)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idGrupo = Convert.ToInt32(conexion.QueryFirst<decimal>("CrearGrupo",
                new { nombre },
                commandType: CommandType.StoredProcedure));

            // Procedimiento AgregarUsuarioAGrupo: el creador y cada integrante
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

    // Procedimiento ListarOtrosUsuarios. "Sumá amigos": las amistades todavía no tienen tabla en la base,
    // así que por ahora se puede sumar a cualquier otro usuario registrado.
    public static List<Usuarios> ObtenerOtrosUsuarios(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Usuarios>("ListarOtrosUsuarios",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // ---------- Planes ----------

    // Los planes de todos los grupos del usuario, sin los cancelados.
    // Planes.Estado: 0 = En votación, 1 = Confirmado, 2 = Cancelado. confirmado = null trae votación y confirmados.
    // La tarjeta muestra el texto del chip, por eso la consulta lo devuelve ya escrito.
    public static List<TarjetaPlan> ObtenerPlanesDeUsuario(int idUsuario, bool? confirmado)
    {
        // Procedimiento ListarPlanesUsuario
        List<TarjetaPlan> planes;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            planes = conexion.Query<TarjetaPlan>("ListarPlanesUsuario",
                new { idUsuario, confirmado },
                commandType: CommandType.StoredProcedure).ToList();
        }

        // Abajo a la derecha de la tarjeta van las iniciales de los otros integrantes del grupo
        foreach (TarjetaPlan plan in planes)
        {
            foreach (Usuarios integrante in ObtenerIntegrantesDeGrupo(plan.IdGrupo))
            {
                if (integrante.Id != idUsuario) plan.Iniciales.Add(integrante.ObtenerIniciales());
            }
        }
        return planes;
    }

    // El contador "planes en votación" del banner de Inicio: los de todos los grupos del usuario
    public static int ContarPlanesEnVotacion(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarPlanesVotacionUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ContarAmigosUsuario: el contador "amigos" del banner de Inicio. Las amistades no tienen tabla
    // en la base, así que cuenta las personas distintas que comparten algún grupo con el usuario.
    public static int ContarAmigos(int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarAmigosUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ObtenerPlan
    public static Planes? ObtenerPlan(int idPlan)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Planes>("ObtenerPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure);
        }
    }

    // La encuesta de un plan como la ve idUsuario (con sus votos marcados):
    // lugares, horarios y lo que votó cada integrante del grupo.
    // PlanesHorarios tiene una fila por lugar y por hora; cada horario de la encuesta se identifica
    // con la primera fila de esa hora (MIN(Id)) y sus votos se cuentan por hora.
    public static EncuestaPlan? ObtenerEncuesta(int idPlan, int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            // Procedimientos ObtenerEncuestaPlan (el plan), ListarOpcionesPlan (los lugares, en el orden en que se
            // propusieron), ListarHorariosPlan (de más temprano a más tarde) y ListarParticipantesPlan
            // (todos los integrantes del grupo con su voto de lugar y de horario; primero el que organiza)
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
            encuesta.Participantes = conexion.Query<Participante>("ListarParticipantesPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure).ToList();
            return encuesta;
        }
    }

    // Las encuestas que se publicaron en el chat del grupo
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

    // Busca lugares por nombre para el modal Crear plan.
    // Primero van los aptos para todo el grupo: los que cumplen todas las restricciones de sus integrantes.
    public static List<LugarBuscado> BuscarLugaresParaGrupo(int idGrupo, string texto)
    {
        // Procedimiento BuscarLugaresParaGrupo
        List<LugarBuscado> lugares;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            lugares = conexion.Query<LugarBuscado>("BuscarLugaresParaGrupo",
                new { idGrupo, texto },
                commandType: CommandType.StoredProcedure).ToList();
        }

        foreach (LugarBuscado lugar in lugares)
        {
            lugar.Restricciones = ObtenerRestriccionesDeRestaurante(lugar.Id);
        }
        return lugares;
    }

    // Procedimiento ObtenerLugar: un lugar suelto, el que viene ya cargado desde el Detalle del restaurante
    public static LugarBuscado? ObtenerLugar(int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<LugarBuscado>("ObtenerLugar",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ListarRestriccionesGrupo: las restricciones de los integrantes del grupo,
    // sin repetir ("Sin TACC", "Vegetariano")
    public static List<string> ObtenerRestriccionesDeGrupo(int idGrupo)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>("ListarRestriccionesGrupo",
                new { idGrupo },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Guarda el plan En votación con sus lugares y horarios y lo publica en el chat como mensaje encuesta.
    // Mientras se vota, FechaSalida es solo el día (FechaCierreVotacion es la misma). Devuelve el Id nuevo.
    public static int CrearPlan(string titulo, DateTime dia, int idGrupo, int idCreador, List<int> idsLugares, List<TimeSpan> horarios)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            // Procedimiento CrearPlan: queda En votación (Estado 0) y devuelve el Id nuevo
            int idPlan = conexion.QueryFirst<int>("CrearPlan",
                new { Titulo = titulo, FechaCierreVotacion = dia, FechaSalida = dia, idGrupo, idUsuarioCreador = idCreador },
                commandType: CommandType.StoredProcedure);

            // PlanesHorarios va por lugar: cada horario se guarda una vez por cada lugar, con el día y la hora
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

            // Procedimiento GuardarMensajePlan: publica el plan en el chat
            conexion.Execute("GuardarMensajePlan",
                new { titulo, idGrupo, idCreador, idPlan },
                commandType: CommandType.StoredProcedure);
            return idPlan;
        }
    }

    // Procedimiento EsLugarDelPlan
    public static bool EsLugarDelPlan(int idPlan, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("EsLugarDelPlan",
                new { idPlan, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ObtenerLugarVotado: el lugar que votó el usuario en el plan (null si todavía no votó)
    public static int? ObtenerLugarVotado(int idPlan, int idUsuario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<int?>("ObtenerLugarVotado",
                new { idPlan, idUsuario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Votos guarda lugar y horario en la misma fila y se graba con el procedimiento VotarPlanCompleto.
    // El horario es la fila de PlanesHorarios del lugar votado: al cambiar de lugar se busca la misma hora
    // en el lugar nuevo, así el horario que ya había votado no se pierde.
    public static void Votar(int idPlan, int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            // Procedimiento ObtenerHorarioVotadoEnLugar
            int? idHorario = conexion.QueryFirstOrDefault<int?>("ObtenerHorarioVotadoEnLugar",
                new { idPlan, idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
            conexion.Execute("VotarPlanCompleto",
                new { idUsuario, idPlan, idRestaurante, idHorarioDisponibleVotado = idHorario },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }

    // Procedimiento EsHorarioDelPlan
    public static bool EsHorarioDelPlan(int idPlan, int idHorario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("EsHorarioDelPlan",
                new { idPlan, idHorario },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Cambia el horario del voto. Hay que haber votado un lugar antes (Votos.idRestaurante es obligatorio).
    // idHorario es cualquier fila de esa hora; se guarda la fila del lugar que votó.
    public static void VotarHorario(int idPlan, int idUsuario, int idRestaurante, int idHorario)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            // Procedimiento ObtenerHorarioDeLugar
            int idFila = conexion.QueryFirst<int>("ObtenerHorarioDeLugar",
                new { idPlan, idRestaurante, idHorario },
                commandType: CommandType.StoredProcedure);
            conexion.Execute("VotarPlanCompleto",
                new { idUsuario, idPlan, idRestaurante, idHorarioDisponibleVotado = idFila },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }

    // Procedimiento ConfirmarPlan: queda Confirmado con el lugar ganador;
    // FechaSalida pasa a ser el día y la hora ganadora
    public static void ConfirmarPlan(int idPlan, int idRestaurante, DateTime fechaSalida)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("ConfirmarPlan",
                new { idPlan, idRestaurante, fechaSalida },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento CancelarPlan
    public static void CancelarPlan(int idPlan)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("CancelarPlan",
                new { idPlan },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ---------- Restaurantes ----------

    // Los restaurantes de las tarjetas "Recomendados". EsFavorito dice si el usuario ya lo guardó
    // (para pintar el corazón; con idUsuario 0 ninguno es favorito).
    // Sin soloApto se usa el procedimiento Mostrar4Restaurantes (los 4 primeros) y se completan
    // categoría, barrio, puntaje y favorito con ObtenerRestaurante y ComprobarEsFavorito.
    // soloApto = true deja solo los que cumplen al menos una restricción del usuario
    // (si el usuario no marcó ninguna, trae todos): para eso no hay procedimiento, queda la consulta.
    // idRestriccion (el chip elegido en Inicio) deja solo los lugares que ofrecen esa opción.
    public static List<TarjetaRestaurante> ObtenerMejorPuntuados(int cantidad, int idUsuario, bool soloApto, int? idRestriccion = null)
    {
        if (!soloApto && idRestriccion == null)
        {
            List<TarjetaRestaurante> tarjetas;
            using (SqlConnection conexion = new SqlConnection(CadenaConexion))
            {
                tarjetas = conexion.Query<TarjetaRestaurante>("Mostrar4Restaurantes",
                    commandType: CommandType.StoredProcedure).Take(cantidad).ToList();
            }
            foreach (TarjetaRestaurante tarjeta in tarjetas)
            {
                CompletarTarjeta(tarjeta, idUsuario);
            }
            return tarjetas;
        }

        // Procedimiento ListarMejorPuntuados
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>("ListarMejorPuntuados",
                new { cantidad, idUsuario, soloApto, idRestriccion },
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Las tarjetas "Recomendado para vos" de Inicio: lugares que se parecen a los gustos del usuario y que cambian en cada visita.
    // El procedimiento ListarRecomendables trae los lugares con sus puntos de afinidad:
    //   +2 si está en un barrio que le interesa (ZonasUsuarios o su barrio del perfil)
    //   +2 si es de una categoría que ya marcó como favorita o reseñó con 4 o 5
    //   +1 si tiene buen puntaje (4 o más)
    // Acá se le suma un número al azar (0 a 3) a cada uno y se toman los de arriba: los que más le gustan salen seguido,
    // pero no son siempre los mismos. No muestra lo que ya tiene en favoritos (eso va en "Tus favoritos").
    // soloApto e idRestriccion funcionan igual que en ObtenerMejorPuntuados.
    public static List<TarjetaRestaurante> ObtenerRecomendados(int cantidad, int idUsuario, bool soloApto, int? idRestriccion = null)
    {
        List<TarjetaRestaurante> candidatos;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            candidatos = conexion.Query<TarjetaRestaurante>("ListarRecomendables",
                new { idUsuario, soloApto, idRestriccion },
                commandType: CommandType.StoredProcedure).ToList();
        }

        Random azar = new Random();
        return candidatos
            .OrderByDescending(tarjeta => tarjeta.Afinidad + azar.NextDouble() * 3)
            .Take(cantidad)
            .ToList();
    }

    // ---------- 05 · Descubrir ----------

    // Las palabras que no sirven para buscar ("parrilla en Palermo" -> "parrilla", "Palermo")
    private static readonly List<string> PalabrasVacias = new List<string> { "en", "de", "la", "el", "los", "las", "del", "con", "y" };

    // Arma los parámetros de los procedimientos de búsqueda (todos reciben los mismos). El procedimiento compara cada palabra
    // con el nombre, el barrio y la categoría: en @p1..@p5 va la palabra con % y en @s1..@s5 la misma sin la "s" final
    // ("Parrillas" encuentra "Parrilla"). Se usan hasta 5 palabras; las vacías ("en", "de"...) se ignoran.
    // El procedimiento ignora tildes y mayúsculas ("nunez" encuentra "Núñez").
    // soloApto: igual que en Inicio, solo los que cumplen al menos una restricción del usuario.
    // idBarrio: el desplegable "Barrio". null = todos los barrios.
    // Si hay zona, solo lo que cae dentro de lo que se ve en el mapa. Si hay Limite, solo los Limite lugares de mejor puntaje.
    private static DynamicParameters ArmarParametrosBusqueda(FiltroBusqueda f)
    {
        DynamicParameters parametros = new DynamicParameters();
        parametros.Add("idUsuario", f.IdUsuario);
        parametros.Add("soloApto", f.SoloApto);
        parametros.Add("idBarrio", f.IdBarrio, DbType.Int32);
        parametros.Add("idCategoria", f.IdCategoria, DbType.Int32);
        parametros.Add("tieneZona", f.TieneZona);
        parametros.Add("norte", f.Norte, DbType.Double);
        parametros.Add("sur", f.Sur, DbType.Double);
        parametros.Add("este", f.Este, DbType.Double);
        parametros.Add("oeste", f.Oeste, DbType.Double);
        parametros.Add("limite", f.Limite, DbType.Int32);

        List<string> palabras = new List<string>();
        foreach (string palabra in f.Texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!PalabrasVacias.Contains(palabra.ToLower())) palabras.Add(palabra);
        }

        for (int numero = 1; numero <= 5; numero++)
        {
            string? conPorcentaje = null;
            string? sinS = null;
            if (numero <= palabras.Count)
            {
                string palabra = palabras[numero - 1];
                string palabraSinS = palabra.Length > 3 && palabra.EndsWith("s") ? palabra.Substring(0, palabra.Length - 1) : palabra;
                conPorcentaje = "%" + palabra + "%";
                sinS = "%" + palabraSinS + "%";
            }
            parametros.Add("p" + numero, conPorcentaje, DbType.String);
            parametros.Add("s" + numero, sinS, DbType.String);
        }
        return parametros;
    }

    // Procedimiento ObtenerProDestacado: el Pro que sale destacado ("Recomendado") arriba de todo en la página 1 cuando no hay barrio elegido:
    // el más cercano al usuario entre los que coinciden con la búsqueda. null si no hay ningún Pro.
    public static int? ObtenerProDestacado(FiltroBusqueda filtroBusqueda, double? latitud, double? longitud)
    {
        DynamicParameters parametros = ArmarParametrosBusqueda(filtroBusqueda);
        parametros.Add("latitudUsuario", latitud, DbType.Double);
        parametros.Add("longitudUsuario", longitud, DbType.Double);

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<int?>("ObtenerProDestacado", parametros,
                commandType: CommandType.StoredProcedure);
        }
    }

    // Una página de resultados (procedimiento BuscarRestaurantesPagina).
    // idDestacado: el Pro de ObtenerProDestacado. Va primero (o sea, arriba de la página 1) y no se repite más abajo.
    // orden: "cerca" (del más cerca al más lejos) o "puntaje" (mejor puntuados primero; los empates, por distancia).
    // latitud y longitud: desde dónde se mide (la ubicación del navegador o el centro del barrio del perfil).
    // Llegan como parámetros y no se guardan. Si son null, no hay distancia.
    // Los que no tienen distancia (sin ubicación o el lugar sin coordenadas) o sin reseñas van al final.
    // El procedimiento trae los primeros desde + cantidad resultados y acá se saltan los de las páginas anteriores.
    // El pin del mapa sale de las coordenadas del lugar o, si no tiene, de las de su barrio.
    public static List<TarjetaRestaurante> BuscarRestaurantes(FiltroBusqueda filtroBusqueda,
        double? latitud, double? longitud, string orden, int? idDestacado, int desde, int cantidad)
    {
        DynamicParameters parametros = ArmarParametrosBusqueda(filtroBusqueda);
        parametros.Add("latitudUsuario", latitud, DbType.Double);
        parametros.Add("longitudUsuario", longitud, DbType.Double);
        parametros.Add("orden", orden);
        parametros.Add("idDestacado", idDestacado, DbType.Int32);
        parametros.Add("hasta", desde + cantidad);

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>("BuscarRestaurantesPagina", parametros,
                commandType: CommandType.StoredProcedure).Skip(desde).ToList();
        }
    }

    // Procedimiento ListarPinesMapa: los lugares de la búsqueda para dibujar sus pins en el mapa: los mismos que la lista
    // (la lista va de a páginas, el mapa los muestra juntos).
    // Trae solo lo que necesita un pin y su tarjetita; los que no tienen coordenadas (ni propias ni de su barrio) no van.
    public static List<TarjetaRestaurante> ObtenerPinesMapa(FiltroBusqueda filtroBusqueda)
    {
        DynamicParameters parametros = ArmarParametrosBusqueda(filtroBusqueda);

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>("ListarPinesMapa", parametros,
                commandType: CommandType.StoredProcedure).ToList();
        }
    }

    // Procedimiento ContarRestaurantesBusqueda: cuántos resultados hay en total (para la paginación)
    public static int ContarRestaurantes(FiltroBusqueda filtroBusqueda)
    {
        DynamicParameters parametros = ArmarParametrosBusqueda(filtroBusqueda);

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("ContarRestaurantesBusqueda", parametros,
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ObtenerBarrioConMasResultados: cuando la búsqueda no da nada en el barrio elegido,
    // el otro barrio donde el texto tiene más resultados
    // (para "No encontramos sushi en Palermo. Hay 3 en Belgrano"). null si no aparece en ningún otro.
    public static BarrioConResultados? ObtenerBarrioConMasResultados(FiltroBusqueda filtroBusqueda, int idBarrioElegido)
    {
        DynamicParameters parametros = ArmarParametrosBusqueda(filtroBusqueda);
        parametros.Add("idBarrioElegido", idBarrioElegido);

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<BarrioConResultados>("ObtenerBarrioConMasResultados", parametros,
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento GuardarBusqueda
    public static void GuardarBusqueda(int idUsuario, string texto)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("GuardarBusqueda",
                new { idUsuario, texto },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Procedimiento ListarBusquedasRecientes: las últimas búsquedas del usuario, sin repetir, la más nueva primero
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

    // Procedimiento ObtenerRestaurante. Devuelve null si el id no existe.
    // El procedimiento llama Promedio al puntaje: acá se copia a Puntaje.
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

    // Procedimiento ComprobarEsFavorito
    public static bool EsFavorito(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("ComprobarEsFavorito",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Completa una tarjeta que solo trae los datos de la tabla Restaurantes:
    // categoría, barrio y puntaje (ObtenerRestaurante) y si es favorito (ComprobarEsFavorito)
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

    // Todo lo de 04 · Detalle + Reseña menos las reseñas. Devuelve null si el id no existe.
    public static DetalleRestaurante? ObtenerDetalleRestaurante(int idRestaurante, int idUsuario)
    {
        DetalleRestaurante? detalle = ObtenerRestaurante(idRestaurante);
        if (detalle == null) return null;

        detalle.EsFavorito = EsFavorito(idUsuario, idRestaurante);
        detalle.Restricciones = ObtenerRestriccionesDeRestaurante(idRestaurante);
        return detalle;
    }

    // Procedimiento ListarRestriccionesRestaurante: los chips dietarios "Sin TACC", "Vegetariano", etc.
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

    // Promedio y cantidad de reseñas (para actualizar la pantalla después de dejar una)
    public static DetalleRestaurante ObtenerPuntajeRestaurante(int idRestaurante)
    {
        return ObtenerRestaurante(idRestaurante) ?? new DetalleRestaurante();
    }

    // ---------- Reseñas ----------
    // Reseñas.Fecha es la fecha de la visita. Una reseña se publica 24 horas después:
    // por eso todas las consultas que muestran o promedian reseñas piden Fecha <= ahora - 24 horas.

    // Si el usuario ya dejó una reseña de ese lugar (publicada o no). Se deja una sola por lugar.
    public static bool YaReseno(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<bool>("ComprobarYaReseno",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    // "Visita verificada": el último plan confirmado de un grupo del usuario en ese lugar
    // que ya pasó. Devuelve null si no hay ninguno.
    public static Planes? ObtenerVisitaVerificada(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Planes>("ObtenerVisitaVerificada",
                new { idUsuario, idRestaurante },
                commandType: CommandType.StoredProcedure);
        }
    }

    // Las reseñas de un lugar de la más nueva a la más vieja, salteando las primeras "desde".
    // cantidad = null trae todas las que quedan.
    // Procedimiento ListarResenasRestaurante (ya viene de la más nueva a la más vieja).
    // El procedimiento no filtra ni pagina: las 24 horas y el "desde / cantidad" se aplican acá.
    // Devuelve el nombre de usuario; el nombre completo se pide con ObtenerPerfilUsuario.
    public static List<ResenaComunidad> ObtenerResenasDeRestaurante(int idRestaurante, int desde, int? cantidad)
    {
        List<ResenaComunidad> resenas;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            resenas = conexion.Query<ResenaComunidad>("ListarResenasRestaurante",
                new { idRestaurante },
                commandType: CommandType.StoredProcedure).ToList();
        }

        DateTime limite = DateTime.Now.AddHours(-24);
        resenas = resenas.Where(resena => resena.Fecha <= limite).Skip(desde).ToList();
        if (cantidad != null) resenas = resenas.Take(cantidad.Value).ToList();

        foreach (ResenaComunidad resena in resenas)
        {
            Usuarios? autor = ObtenerPerfilUsuario(resena.IdUsuario);
            resena.NombreCompleto = autor?.NombreCompleto ?? resena.NombreUsuario;
            resena.FotoPerfil = autor?.FotoPerfil;
        }
        return resenas;
    }

    // Procedimiento CrearResena. La fecha de la reseña es ahora (la pone el procedimiento).
    // Devuelve 0 si se guardó, 1 si el puntaje no es de 1 a 5 o 2 si el usuario ya reseñó ese lugar.
    public static int GuardarResena(Reseñas resena)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>("CrearResena",
                new { resena.IdUsuario, resena.IdRestaurante, resena.Puntaje, resena.Comentario, resena.Foto },
                commandType: CommandType.StoredProcedure);
        }
    }

    // ---------- Favoritos ----------

    // Procedimiento MostrarFavoritos: los favoritos del usuario, en el orden en que los guardó.
    // El procedimiento devuelve tres columnas Nombre seguidas (restaurante, barrio y categoría),
    // por eso se leen por posición. El puntaje se completa con ObtenerRestaurante.
    // Los lugares que ofrecen una restricción: con esto el chip de Inicio filtra "Tus favoritos"
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

    // Si ya era favorito lo saca (QuitarFavorito), si no lo agrega (AgregarFavorito).
    // Devuelve true si quedó como favorito.
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
}
