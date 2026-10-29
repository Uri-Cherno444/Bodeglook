using Dapper;
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
            int resultado = conexion.QueryFirst<int>("SELECT 1");
            return resultado == 1;
        }
    }

    // ---------- Usuarios ----------

    // Busca por nombre de usuario o por email (el login acepta los dos)
    public static Usuarios? ObtenerUsuarioPorUsuarioOEmail(string texto)
    {
        string consulta = "SELECT * FROM Usuarios WHERE NombreUsuario = @texto OR Email = @texto";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>(consulta, new { texto });
        }
    }

    public static bool ExisteEmail(string email)
    {
        string consulta = "SELECT COUNT(*) FROM Usuarios WHERE Email = @email";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { email }) > 0;
        }
    }

    public static bool ExisteNombreUsuario(string nombreUsuario)
    {
        string consulta = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @nombreUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { nombreUsuario }) > 0;
        }
    }

    // Guarda el usuario y devuelve el Id nuevo. La contraseña ya tiene que venir hasheada.
    public static int RegistrarUsuario(Usuarios usuario)
    {
        string consulta = @"
            INSERT INTO Usuarios (NombreCompleto, NombreUsuario, Email, Contraseña, FechaRegistro)
            OUTPUT INSERTED.Id
            VALUES (@NombreCompleto, @NombreUsuario, @Email, @Contraseña, @FechaRegistro)";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, usuario);
        }
    }

    // ---------- 08 · Perfil ----------

    // "Palermo, CABA". null si el usuario no tiene barrio (el registro no lo pide).
    public static string? ObtenerNombreBarrio(int? idBarrio)
    {
        if (idBarrio == null) return null;
        string consulta = "SELECT Nombre FROM Barrios WHERE Id = @idBarrio";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<string>(consulta, new { idBarrio });
        }
    }

    // Todas las reseñas que escribió, publicadas o no
    public static int ContarResenasDeUsuario(int idUsuario)
    {
        string consulta = "SELECT COUNT(*) FROM Reseñas WHERE IdUsuario = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idUsuario });
        }
    }

    // ---------- Onboarding (gustos y filtro dietario) ----------

    // Todas las restricciones: son los chips de "Restricciones dietarias"
    public static List<Restricciones> ObtenerRestricciones()
    {
        string consulta = "SELECT Id, Nombre FROM Restricciones ORDER BY Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Restricciones>(consulta).ToList();
        }
    }

    // Todos los barrios: son los chips de "Zonas que te interesan"
    public static List<Barrios> ObtenerBarrios()
    {
        string consulta = "SELECT Id, Nombre FROM Barrios ORDER BY Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Barrios>(consulta).ToList();
        }
    }

    // Las restricciones que marcó el usuario
    public static List<Restricciones> ObtenerRestriccionesDeUsuario(int idUsuario)
    {
        string consulta = @"
            SELECT re.Id, re.Nombre
            FROM RestriccionesUsuarios ru
            JOIN Restricciones re ON re.Id = ru.IdRestricciones
            WHERE ru.IdUsuario = @idUsuario
            ORDER BY re.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Restricciones>(consulta, new { idUsuario }).ToList();
        }
    }

    // Borra las restricciones que tenía el usuario y guarda las nuevas
    public static void GuardarRestriccionesDeUsuario(int idUsuario, List<int> idsRestricciones)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute("DELETE FROM RestriccionesUsuarios WHERE IdUsuario = @idUsuario", new { idUsuario });
            foreach (int idRestriccion in idsRestricciones)
            {
                conexion.Execute(
                    "INSERT INTO RestriccionesUsuarios (IdUsuario, IdRestricciones) VALUES (@idUsuario, @idRestriccion)",
                    new { idUsuario, idRestriccion });
            }
        }
    }

    // ---------- Grupos y chats ----------

    // Integrantes de un grupo (sin la contraseña)
    public static List<Usuarios> ObtenerIntegrantesDeGrupo(int idGrupo)
    {
        string consulta = @"
            SELECT u.Id, u.NombreCompleto, u.NombreUsuario
            FROM Usuarios u
            JOIN UsuariosGrupos ug ON ug.IdUsuario = u.Id
            WHERE ug.IdGrupo = @idGrupo
            ORDER BY u.NombreCompleto";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Usuarios>(consulta, new { idGrupo }).ToList();
        }
    }

    public static bool EsIntegranteDeGrupo(int idUsuario, int idGrupo)
    {
        string consulta = "SELECT COUNT(*) FROM UsuariosGrupos WHERE IdUsuario = @idUsuario AND IdGrupo = @idGrupo";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idUsuario, idGrupo }) > 0;
        }
    }

    // Los grupos del usuario con la fecha del último mensaje y cuántos no leyó.
    // No leídos = mensajes de otros posteriores a su UltimaLectura.
    public static List<ResumenChat> ObtenerChatsDeUsuario(int idUsuario)
    {
        string consulta = @"
            SELECT g.Id AS IdGrupo, g.Nombre, g.Foto,
                   (SELECT MAX(FechaHora) FROM Mensajes WHERE IdGrupo = g.Id) AS UltimaFecha,
                   (SELECT COUNT(*) FROM Mensajes m
                    WHERE m.IdGrupo = g.Id AND m.IdUsuario <> @idUsuario
                      AND (ug.UltimaLectura IS NULL OR m.FechaHora > ug.UltimaLectura)) AS NoLeidos
            FROM Grupos g
            JOIN UsuariosGrupos ug ON ug.IdGrupo = g.Id
            WHERE ug.IdUsuario = @idUsuario
            ORDER BY COALESCE((SELECT MAX(FechaHora) FROM Mensajes WHERE IdGrupo = g.Id), g.FechaCreacion) DESC";
        List<ResumenChat> chats;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            chats = conexion.Query<ResumenChat>(consulta, new { idUsuario }).ToList();
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

    public static Grupos? ObtenerGrupo(int idGrupo)
    {
        string consulta = "SELECT Id, Nombre, Foto, FechaCreacion FROM Grupos WHERE Id = @idGrupo";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Grupos>(consulta, new { idGrupo });
        }
    }

    // Los mensajes del grupo con Id mayor a desdeId, del más viejo al más nuevo.
    // Con desdeId = 0 trae todos; el chat después pide solo los nuevos.
    public static List<MensajeChat> ObtenerMensajes(int idGrupo, int desdeId)
    {
        string consulta = @"
            SELECT m.Id, m.Contenido, m.FechaHora, m.IdUsuario, u.NombreCompleto, m.IdPlan
            FROM Mensajes m
            JOIN Usuarios u ON u.Id = m.IdUsuario
            WHERE m.IdGrupo = @idGrupo AND m.Id > @desdeId
            ORDER BY m.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<MensajeChat>(consulta, new { idGrupo, desdeId }).ToList();
        }
    }

    public static void GuardarMensaje(int idGrupo, int idUsuario, string contenido)
    {
        string consulta = @"
            INSERT INTO Mensajes (Contenido, FechaHora, IdGrupo, IdUsuario)
            VALUES (@contenido, GETDATE(), @idGrupo, @idUsuario)";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { contenido, idGrupo, idUsuario });
        }
    }

    // Al abrir el chat (y cada vez que llegan mensajes con el chat abierto) sus no leídos quedan en cero
    public static void MarcarComoLeido(int idUsuario, int idGrupo)
    {
        string consulta = "UPDATE UsuariosGrupos SET UltimaLectura = GETDATE() WHERE IdUsuario = @idUsuario AND IdGrupo = @idGrupo";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { idUsuario, idGrupo });
        }
    }

    // Crea el grupo con el creador y los integrantes que sumó. Devuelve el Id nuevo.
    public static int CrearGrupo(string nombre, int idCreador, List<int> idsIntegrantes)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idGrupo = conexion.QueryFirst<int>(
                "INSERT INTO Grupos (Nombre, FechaCreacion) OUTPUT INSERTED.Id VALUES (@nombre, GETDATE())",
                new { nombre });

            conexion.Execute(
                "INSERT INTO UsuariosGrupos (IdUsuario, IdGrupo, UltimaLectura) VALUES (@idCreador, @idGrupo, GETDATE())",
                new { idCreador, idGrupo });
            foreach (int idIntegrante in idsIntegrantes)
            {
                conexion.Execute(
                    "INSERT INTO UsuariosGrupos (IdUsuario, IdGrupo) VALUES (@idIntegrante, @idGrupo)",
                    new { idIntegrante, idGrupo });
            }
            return idGrupo;
        }
    }

    public static void ActualizarFotoGrupo(int idGrupo, string foto)
    {
        string consulta = "UPDATE Grupos SET Foto = @foto WHERE Id = @idGrupo";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { foto, idGrupo });
        }
    }

    // "Sumá amigos": las amistades todavía no tienen tabla en la base,
    // así que por ahora se puede sumar a cualquier otro usuario registrado.
    public static List<Usuarios> ObtenerOtrosUsuarios(int idUsuario)
    {
        string consulta = @"
            SELECT Id, NombreCompleto, NombreUsuario
            FROM Usuarios
            WHERE Id <> @idUsuario
            ORDER BY NombreCompleto";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Usuarios>(consulta, new { idUsuario }).ToList();
        }
    }

    // ---------- Planes ----------

    // Los planes de todos los grupos del usuario, sin los cancelados.
    // Planes.Estado: 0 = En votación, 1 = Confirmado, 2 = Cancelado. confirmado = null trae votación y confirmados.
    // La tarjeta muestra el texto del chip, por eso la consulta lo devuelve ya escrito.
    public static List<TarjetaPlan> ObtenerPlanesDeUsuario(int idUsuario, bool? confirmado)
    {
        string consulta = @"
            SELECT p.Id, p.Titulo, p.IdGrupo, p.IdRestauranteElegido, r.Nombre AS LugarElegido,
                   CASE WHEN p.Estado = 1 THEN 'Confirmado' ELSE 'En votación' END AS Estado,
                   (SELECT COUNT(*) FROM PlanesRestaurantes WHERE IdPlan = p.Id) AS CantidadLugares
            FROM Planes p
            JOIN UsuariosGrupos ug ON ug.IdGrupo = p.IdGrupo AND ug.IdUsuario = @idUsuario
            LEFT JOIN Restaurantes r ON r.Id = p.IdRestauranteElegido
            WHERE p.Estado <> 2 AND (@confirmado IS NULL OR p.Estado = @confirmado)
            ORDER BY p.Id DESC";
        List<TarjetaPlan> planes;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            planes = conexion.Query<TarjetaPlan>(consulta, new { idUsuario, confirmado }).ToList();
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

    public static Planes? ObtenerPlan(int idPlan)
    {
        string consulta = "SELECT * FROM Planes WHERE Id = @idPlan";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Planes>(consulta, new { idPlan });
        }
    }

    // La encuesta de un plan como la ve idUsuario (con sus votos marcados):
    // lugares, horarios y lo que votó cada integrante del grupo.
    // PlanesHorarios tiene una fila por lugar y por hora; cada horario de la encuesta se identifica
    // con la primera fila de esa hora (MIN(Id)) y sus votos se cuentan por hora.
    public static EncuestaPlan? ObtenerEncuesta(int idPlan, int idUsuario)
    {
        string consultaPlan = @"
            SELECT p.Id, p.Titulo, p.Estado, p.FechaSalida, p.IdGrupo, g.Nombre AS NombreGrupo,
                   p.IdUsuarioCreador AS IdCreador, u.NombreCompleto AS NombreCreador, p.IdRestauranteElegido,
                   (SELECT COUNT(*) FROM UsuariosGrupos WHERE IdGrupo = p.IdGrupo) AS CantidadIntegrantes,
                   (SELECT IdRestaurante FROM Votos WHERE IdPlan = p.Id AND IdUsuario = @idUsuario) AS MiVoto,
                   (SELECT MIN(ph.Id) FROM Votos v
                    JOIN PlanesHorarios votado ON votado.Id = v.IdHorarioVotado
                    JOIN PlanesHorarios ph ON ph.IdPlan = v.IdPlan AND ph.FechaHoraPropuesta = votado.FechaHoraPropuesta
                    WHERE v.IdPlan = p.Id AND v.IdUsuario = @idUsuario) AS MiVotoHorario
            FROM Planes p
            JOIN Grupos g ON g.Id = p.IdGrupo
            LEFT JOIN Usuarios u ON u.Id = p.IdUsuarioCreador
            WHERE p.Id = @idPlan";
        // Los lugares en el orden en que se propusieron
        string consultaOpciones = @"
            SELECT r.Id AS IdRestaurante, r.Nombre, c.Nombre AS Categoria, b.Nombre AS Barrio, r.SitioWeb,
                   (SELECT COUNT(*) FROM Votos WHERE IdPlan = pr.IdPlan AND IdRestaurante = r.Id) AS Votos
            FROM PlanesRestaurantes pr
            JOIN Restaurantes r ON r.Id = pr.IdRestaurante
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE pr.IdPlan = @idPlan
            ORDER BY pr.Id";
        // Los horarios de más temprano a más tarde
        string consultaHorarios = @"
            SELECT MIN(ph.Id) AS IdHorario, ph.FechaHoraPropuesta,
                   (SELECT COUNT(*) FROM Votos v
                    JOIN PlanesHorarios votado ON votado.Id = v.IdHorarioVotado
                    WHERE v.IdPlan = @idPlan AND votado.FechaHoraPropuesta = ph.FechaHoraPropuesta) AS Votos
            FROM PlanesHorarios ph
            WHERE ph.IdPlan = @idPlan
            GROUP BY ph.FechaHoraPropuesta
            ORDER BY ph.FechaHoraPropuesta";
        // Todos los integrantes del grupo con su voto de lugar y de horario (null si no votaron).
        // Primero el que organiza y después por nombre.
        string consultaParticipantes = @"
            SELECT u.Id AS IdUsuario, u.NombreCompleto,
                   v.IdRestaurante AS IdLugarVotado, r.Nombre AS LugarVotado,
                   (SELECT MIN(ph.Id) FROM PlanesHorarios ph
                    WHERE ph.IdPlan = p.Id AND ph.FechaHoraPropuesta = votado.FechaHoraPropuesta) AS IdHorarioVotado,
                   votado.FechaHoraPropuesta AS HorarioVotado
            FROM Planes p
            JOIN UsuariosGrupos ug ON ug.IdGrupo = p.IdGrupo
            JOIN Usuarios u ON u.Id = ug.IdUsuario
            LEFT JOIN Votos v ON v.IdPlan = p.Id AND v.IdUsuario = u.Id
            LEFT JOIN Restaurantes r ON r.Id = v.IdRestaurante
            LEFT JOIN PlanesHorarios votado ON votado.Id = v.IdHorarioVotado
            WHERE p.Id = @idPlan
            ORDER BY CASE WHEN u.Id = p.IdUsuarioCreador THEN 0 ELSE 1 END, u.NombreCompleto";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            EncuestaPlan? encuesta = conexion.QueryFirstOrDefault<EncuestaPlan>(consultaPlan, new { idPlan, idUsuario });
            if (encuesta == null) return null;
            encuesta.Opciones = conexion.Query<OpcionPlan>(consultaOpciones, new { idPlan }).ToList();
            encuesta.Horarios = conexion.Query<OpcionHorario>(consultaHorarios, new { idPlan }).ToList();
            encuesta.Participantes = conexion.Query<Participante>(consultaParticipantes, new { idPlan }).ToList();
            return encuesta;
        }
    }

    // Las encuestas que se publicaron en el chat del grupo
    public static List<EncuestaPlan> ObtenerEncuestasDeGrupo(int idGrupo, int idUsuario)
    {
        List<int> idsPlanes;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            idsPlanes = conexion.Query<int>(
                "SELECT IdPlan FROM Mensajes WHERE IdGrupo = @idGrupo AND IdPlan IS NOT NULL ORDER BY Id",
                new { idGrupo }).ToList();
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
        string consulta = @"
            SELECT TOP 6 r.Id, r.Nombre, c.Nombre AS Categoria, b.Nombre AS Barrio,
                   CAST(CASE WHEN NOT EXISTS (
                            SELECT 1 FROM RestriccionesUsuarios ru
                            JOIN UsuariosGrupos ug ON ug.IdUsuario = ru.IdUsuario
                            WHERE ug.IdGrupo = @idGrupo
                              AND NOT EXISTS (SELECT 1 FROM RestriccionesRestaurantes rr
                                              WHERE rr.IdRestaurante = r.Id AND rr.IdRestriccion = ru.IdRestricciones))
                        THEN 1 ELSE 0 END AS bit) AS AptoParaTodos
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE r.Nombre LIKE @texto
            ORDER BY AptoParaTodos DESC, r.Nombre";
        List<LugarBuscado> lugares;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            lugares = conexion.Query<LugarBuscado>(consulta, new { idGrupo, texto = "%" + texto + "%" }).ToList();
        }

        foreach (LugarBuscado lugar in lugares)
        {
            lugar.Restricciones = ObtenerRestriccionesDeRestaurante(lugar.Id);
        }
        return lugares;
    }

    // Un lugar suelto: el que viene ya cargado desde el Detalle del restaurante
    public static LugarBuscado? ObtenerLugar(int idRestaurante)
    {
        string consulta = @"
            SELECT r.Id, r.Nombre, c.Nombre AS Categoria, b.Nombre AS Barrio
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE r.Id = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<LugarBuscado>(consulta, new { idRestaurante });
        }
    }

    // Las restricciones de los integrantes del grupo, sin repetir ("Sin TACC", "Vegetariano")
    public static List<string> ObtenerRestriccionesDeGrupo(int idGrupo)
    {
        string consulta = @"
            SELECT DISTINCT re.Nombre
            FROM RestriccionesUsuarios ru
            JOIN UsuariosGrupos ug ON ug.IdUsuario = ru.IdUsuario
            JOIN Restricciones re ON re.Id = ru.IdRestricciones
            WHERE ug.IdGrupo = @idGrupo";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>(consulta, new { idGrupo }).ToList();
        }
    }

    // Guarda el plan En votación con sus lugares y horarios y lo publica en el chat como mensaje encuesta.
    // Mientras se vota, FechaSalida es solo el día (FechaCierreVotacion es la misma). Devuelve el Id nuevo.
    public static int CrearPlan(string titulo, DateTime dia, int idGrupo, int idCreador, List<int> idsLugares, List<TimeSpan> horarios)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idPlan = conexion.QueryFirst<int>(@"
                INSERT INTO Planes (Titulo, FechaCierreVotacion, FechaSalida, Estado, IdGrupo, IdUsuarioCreador)
                OUTPUT INSERTED.Id
                VALUES (@titulo, @dia, @dia, 0, @idGrupo, @idCreador)",
                new { titulo, dia, idGrupo, idCreador });

            // PlanesHorarios va por lugar: cada horario se guarda una vez por cada lugar, con el día y la hora
            foreach (int idRestaurante in idsLugares)
            {
                conexion.Execute(
                    "INSERT INTO PlanesRestaurantes (IdPlan, IdRestaurante) VALUES (@idPlan, @idRestaurante)",
                    new { idPlan, idRestaurante });
                foreach (TimeSpan horario in horarios)
                {
                    conexion.Execute(
                        "INSERT INTO PlanesHorarios (IdPlan, IdRestaurante, FechaHoraPropuesta) VALUES (@idPlan, @idRestaurante, @fechaHora)",
                        new { idPlan, idRestaurante, fechaHora = dia.Add(horario) });
                }
            }

            conexion.Execute(@"
                INSERT INTO Mensajes (Contenido, FechaHora, IdGrupo, IdUsuario, IdPlan)
                VALUES (@titulo, GETDATE(), @idGrupo, @idCreador, @idPlan)",
                new { titulo, idGrupo, idCreador, idPlan });
            return idPlan;
        }
    }

    public static bool EsLugarDelPlan(int idPlan, int idRestaurante)
    {
        string consulta = "SELECT COUNT(*) FROM PlanesRestaurantes WHERE IdPlan = @idPlan AND IdRestaurante = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idPlan, idRestaurante }) > 0;
        }
    }

    // El lugar que votó el usuario en el plan (null si todavía no votó)
    public static int? ObtenerLugarVotado(int idPlan, int idUsuario)
    {
        string consulta = "SELECT IdRestaurante FROM Votos WHERE IdPlan = @idPlan AND IdUsuario = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<int?>(consulta, new { idPlan, idUsuario });
        }
    }

    // Votos guarda lugar y horario en la misma fila y se graba con el procedimiento VotarPlanCompleto.
    // El horario es la fila de PlanesHorarios del lugar votado: al cambiar de lugar se busca la misma hora
    // en el lugar nuevo, así el horario que ya había votado no se pierde.
    public static void Votar(int idPlan, int idUsuario, int idRestaurante)
    {
        string consultaHorario = @"
            SELECT ph.Id FROM Votos v
            JOIN PlanesHorarios votado ON votado.Id = v.IdHorarioVotado
            JOIN PlanesHorarios ph ON ph.IdPlan = v.IdPlan AND ph.FechaHoraPropuesta = votado.FechaHoraPropuesta
            WHERE v.IdPlan = @idPlan AND v.IdUsuario = @idUsuario AND ph.IdRestaurante = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int? idHorario = conexion.QueryFirstOrDefault<int?>(consultaHorario, new { idPlan, idUsuario, idRestaurante });
            conexion.Execute("VotarPlanCompleto",
                new { idUsuario, idPlan, idRestaurante, idHorarioDisponibleVotado = idHorario },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }

    public static bool EsHorarioDelPlan(int idPlan, int idHorario)
    {
        string consulta = "SELECT COUNT(*) FROM PlanesHorarios WHERE IdPlan = @idPlan AND Id = @idHorario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idPlan, idHorario }) > 0;
        }
    }

    // Cambia el horario del voto. Hay que haber votado un lugar antes (Votos.idRestaurante es obligatorio).
    // idHorario es cualquier fila de esa hora; se guarda la fila del lugar que votó.
    public static void VotarHorario(int idPlan, int idUsuario, int idRestaurante, int idHorario)
    {
        string consultaHorario = @"
            SELECT ph.Id FROM PlanesHorarios ph
            JOIN PlanesHorarios elegido ON elegido.Id = @idHorario AND elegido.FechaHoraPropuesta = ph.FechaHoraPropuesta
            WHERE ph.IdPlan = @idPlan AND ph.IdRestaurante = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int idFila = conexion.QueryFirst<int>(consultaHorario, new { idPlan, idRestaurante, idHorario });
            conexion.Execute("VotarPlanCompleto",
                new { idUsuario, idPlan, idRestaurante, idHorarioDisponibleVotado = idFila },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }

    // Queda Confirmado con el lugar ganador; FechaSalida pasa a ser el día y la hora ganadora
    public static void ConfirmarPlan(int idPlan, int idRestaurante, DateTime fechaSalida)
    {
        string consulta = @"
            UPDATE Planes
            SET Estado = 1, IdRestauranteElegido = @idRestaurante,
                FechaSalida = @fechaSalida, FechaCierreVotacion = @fechaSalida
            WHERE Id = @idPlan";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { idPlan, idRestaurante, fechaSalida });
        }
    }

    public static void CancelarPlan(int idPlan)
    {
        string consulta = "UPDATE Planes SET Estado = 2 WHERE Id = @idPlan";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { idPlan });
        }
    }

    // ---------- Restaurantes ----------

    // Los restaurantes con mejor promedio de reseñas. Los que no tienen reseñas van al final.
    // EsFavorito dice si el usuario ya lo guardó (para pintar el corazón).
    // soloApto = true deja solo los que cumplen al menos una restricción del usuario
    // (si el usuario no marcó ninguna, trae todos).
    public static List<TarjetaRestaurante> ObtenerMejorPuntuados(int cantidad, int idUsuario, bool soloApto)
    {
        string consulta = @"
            SELECT TOP (@cantidad) r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio,
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS Puntaje,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = r.Id)
                        THEN 1 ELSE 0 END AS bit) AS EsFavorito
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE @soloApto = 0
               OR NOT EXISTS (SELECT 1 FROM RestriccionesUsuarios WHERE IdUsuario = @idUsuario)
               OR EXISTS (SELECT 1 FROM RestriccionesRestaurantes rr
                          JOIN RestriccionesUsuarios ru ON ru.IdRestricciones = rr.IdRestriccion
                          WHERE rr.IdRestaurante = r.Id AND ru.IdUsuario = @idUsuario)
            ORDER BY CASE WHEN (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) IS NULL THEN 1 ELSE 0 END,
                     Puntaje DESC, r.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>(consulta, new { cantidad, idUsuario, soloApto }).ToList();
        }
    }

    // ---------- 05 · Descubrir ----------

    // Las palabras que no sirven para buscar ("parrilla en Palermo" -> "parrilla", "Palermo")
    private static readonly List<string> PalabrasVacias = new List<string> { "en", "de", "la", "el", "los", "las", "del", "con", "y" };

    // Arma el WHERE de la búsqueda. Cada palabra tiene que aparecer en el nombre, el barrio o la categoría.
    // Si termina en "s" también se prueba sin la "s" ("Parrillas" encuentra "Parrilla").
    // COLLATE ..._CI_AI hace que no importen mayúsculas ni tildes ("nunez" encuentra "Núñez").
    // soloApto: igual que en Inicio, solo los que cumplen al menos una restricción del usuario.
    private static string ArmarFiltroBusqueda(string texto, int idUsuario, bool soloApto, DynamicParameters parametros)
    {
        string filtro = @"
            WHERE (@soloApto = 0
               OR NOT EXISTS (SELECT 1 FROM RestriccionesUsuarios WHERE IdUsuario = @idUsuario)
               OR EXISTS (SELECT 1 FROM RestriccionesRestaurantes rr
                          JOIN RestriccionesUsuarios ru ON ru.IdRestricciones = rr.IdRestriccion
                          WHERE rr.IdRestaurante = r.Id AND ru.IdUsuario = @idUsuario))";
        parametros.Add("idUsuario", idUsuario);
        parametros.Add("soloApto", soloApto);

        int numero = 0;
        foreach (string palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (PalabrasVacias.Contains(palabra.ToLower())) continue;

            string sinS = palabra.Length > 3 && palabra.EndsWith("s") ? palabra.Substring(0, palabra.Length - 1) : palabra;
            parametros.Add("palabra" + numero, "%" + palabra + "%");
            parametros.Add("sinS" + numero, "%" + sinS + "%");

            filtro += $@"
              AND (r.Nombre COLLATE Latin1_General_CI_AI LIKE @palabra{numero}
                OR b.Nombre COLLATE Latin1_General_CI_AI LIKE @palabra{numero}
                OR c.Nombre COLLATE Latin1_General_CI_AI LIKE @palabra{numero}
                OR c.Nombre COLLATE Latin1_General_CI_AI LIKE @sinS{numero})";
            numero++;
        }
        return filtro;
    }

    // Una página de resultados. Primero los Pro (salen destacados) y después por mejor puntaje.
    public static List<TarjetaRestaurante> BuscarRestaurantes(string texto, int idUsuario, bool soloApto, int desde, int cantidad)
    {
        DynamicParameters parametros = new DynamicParameters();
        string filtro = ArmarFiltroBusqueda(texto, idUsuario, soloApto, parametros);
        parametros.Add("desde", desde);
        parametros.Add("cantidad", cantidad);

        string consulta = @"
            SELECT r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio, r.EsPro, r.Latitud, r.Longitud,
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS Puntaje,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = r.Id)
                        THEN 1 ELSE 0 END AS bit) AS EsFavorito
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio" + filtro + @"
            ORDER BY r.EsPro DESC,
                     CASE WHEN (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) IS NULL THEN 1 ELSE 0 END,
                     Puntaje DESC, r.Id
            OFFSET @desde ROWS FETCH NEXT @cantidad ROWS ONLY";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>(consulta, parametros).ToList();
        }
    }

    // Cuántos resultados hay en total (para la paginación)
    public static int ContarRestaurantes(string texto, int idUsuario, bool soloApto)
    {
        DynamicParameters parametros = new DynamicParameters();
        string filtro = ArmarFiltroBusqueda(texto, idUsuario, soloApto, parametros);

        string consulta = @"
            SELECT COUNT(*)
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio" + filtro;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, parametros);
        }
    }

    public static void GuardarBusqueda(int idUsuario, string texto)
    {
        string consulta = "INSERT INTO Busquedas (IdUsuario, Texto, Fecha) VALUES (@idUsuario, @texto, GETDATE())";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { idUsuario, texto });
        }
    }

    // Las últimas búsquedas del usuario, sin repetir, la más nueva primero
    public static List<string> ObtenerBusquedasRecientes(int idUsuario, int cantidad)
    {
        string consulta = @"
            SELECT TOP (@cantidad) Texto
            FROM Busquedas
            WHERE IdUsuario = @idUsuario
            GROUP BY Texto
            ORDER BY MAX(Fecha) DESC, MAX(Id) DESC";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>(consulta, new { idUsuario, cantidad }).ToList();
        }
    }

    public static bool ExisteRestaurante(int idRestaurante)
    {
        string consulta = "SELECT COUNT(*) FROM Restaurantes WHERE Id = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idRestaurante }) > 0;
        }
    }

    // Todo lo de 04 · Detalle + Reseña menos las reseñas. Devuelve null si el id no existe.
    public static DetalleRestaurante? ObtenerDetalleRestaurante(int idRestaurante, int idUsuario)
    {
        string consulta = @"
            SELECT r.Id, r.Nombre, r.Foto, r.Descripcion, c.Nombre AS Categoria, b.Nombre AS Barrio,
                   r.DireccionCalle, r.DireccionNumero, r.DireccionCodigoPostal, r.Telefono,
                   r.HorarioApertura, r.HorarioCerrado,
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS Puntaje,
                   (SELECT COUNT(*) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS CantidadResenas,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = r.Id)
                        THEN 1 ELSE 0 END AS bit) AS EsFavorito
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE r.Id = @idRestaurante";
        DetalleRestaurante? detalle;
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            detalle = conexion.QueryFirstOrDefault<DetalleRestaurante>(consulta, new { idRestaurante, idUsuario });
        }

        if (detalle != null) detalle.Restricciones = ObtenerRestriccionesDeRestaurante(idRestaurante);
        return detalle;
    }

    // Los chips dietarios: "Sin TACC", "Vegetariano", etc.
    public static List<string> ObtenerRestriccionesDeRestaurante(int idRestaurante)
    {
        string consulta = @"
            SELECT re.Nombre
            FROM RestriccionesRestaurantes rr
            JOIN Restricciones re ON re.Id = rr.IdRestriccion
            WHERE rr.IdRestaurante = @idRestaurante
            ORDER BY re.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>(consulta, new { idRestaurante }).ToList();
        }
    }

    // Promedio y cantidad de reseñas (para actualizar la pantalla después de dejar una)
    public static DetalleRestaurante ObtenerPuntajeRestaurante(int idRestaurante)
    {
        string consulta = @"
            SELECT AVG(CAST(Puntaje AS float)) AS Puntaje, COUNT(*) AS CantidadResenas
            FROM Reseñas WHERE IdRestaurante = @idRestaurante AND Fecha <= DATEADD(hour, -24, GETDATE())";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<DetalleRestaurante>(consulta, new { idRestaurante });
        }
    }

    // ---------- Reseñas ----------
    // Reseñas.Fecha es la fecha de la visita. Una reseña se publica 24 horas después:
    // por eso todas las consultas que muestran o promedian reseñas piden Fecha <= ahora - 24 horas.

    // Si el usuario ya dejó una reseña de ese lugar (publicada o no). Se deja una sola por lugar.
    public static bool YaReseno(int idUsuario, int idRestaurante)
    {
        string consulta = "SELECT COUNT(*) FROM Reseñas WHERE IdUsuario = @idUsuario AND IdRestaurante = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idUsuario, idRestaurante }) > 0;
        }
    }

    // "Visita verificada": el último plan confirmado de un grupo del usuario en ese lugar
    // que ya pasó. Devuelve null si no hay ninguno.
    public static Planes? ObtenerVisitaVerificada(int idUsuario, int idRestaurante)
    {
        string consulta = @"
            SELECT TOP 1 p.*
            FROM Planes p
            JOIN UsuariosGrupos ug ON ug.IdGrupo = p.IdGrupo AND ug.IdUsuario = @idUsuario
            WHERE p.Estado = 1 AND p.IdRestauranteElegido = @idRestaurante AND p.FechaSalida <= GETDATE()
            ORDER BY p.FechaSalida DESC";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Planes>(consulta, new { idUsuario, idRestaurante });
        }
    }

    // Las reseñas de un lugar de la más nueva a la más vieja, salteando las primeras "desde".
    // cantidad = null trae todas las que quedan.
    public static List<ResenaComunidad> ObtenerResenasDeRestaurante(int idRestaurante, int desde, int? cantidad)
    {
        string consulta = @"
            SELECT re.Id, re.Puntaje, re.Comentario, re.Fecha, u.NombreCompleto
            FROM Reseñas re
            JOIN Usuarios u ON u.Id = re.IdUsuario
            WHERE re.IdRestaurante = @idRestaurante AND re.Fecha <= DATEADD(hour, -24, GETDATE())
            ORDER BY re.Fecha DESC, re.Id DESC
            OFFSET @desde ROWS";
        if (cantidad != null) consulta += " FETCH NEXT @cantidad ROWS ONLY";

        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<ResenaComunidad>(consulta, new { idRestaurante, desde, cantidad }).ToList();
        }
    }

    // Guarda la reseña y devuelve el Id nuevo
    public static int GuardarResena(Reseñas resena)
    {
        string consulta = @"
            INSERT INTO Reseñas (Puntaje, Comentario, Fecha, IdUsuario, IdRestaurante)
            OUTPUT INSERTED.Id
            VALUES (@Puntaje, @Comentario, @Fecha, @IdUsuario, @IdRestaurante)";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, resena);
        }
    }

    // ---------- Favoritos ----------

    // Los favoritos del usuario, en el orden en que los guardó
    public static List<TarjetaRestaurante> ObtenerFavoritos(int idUsuario)
    {
        string consulta = @"
            SELECT r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio,
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS Puntaje,
                   CAST(1 AS bit) AS EsFavorito
            FROM Favoritos f
            JOIN Restaurantes r ON r.Id = f.IdRestaurante
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE f.IdUsuario = @idUsuario
            ORDER BY f.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>(consulta, new { idUsuario }).ToList();
        }
    }

    // Si ya era favorito lo saca, si no lo agrega. Devuelve true si quedó como favorito.
    public static bool AlternarFavorito(int idUsuario, int idRestaurante)
    {
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            int borrados = conexion.Execute(
                "DELETE FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = @idRestaurante",
                new { idUsuario, idRestaurante });
            if (borrados > 0) return false;

            conexion.Execute(
                "INSERT INTO Favoritos (IdUsuario, IdRestaurante) VALUES (@idUsuario, @idRestaurante)",
                new { idUsuario, idRestaurante });
            return true;
        }
    }
}
