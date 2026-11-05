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
            int resultado = conexion.QueryFirst<int>("SELECT 1");
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

    // Los barrios con el centro (Latitud y Longitud), para ubicar los lugares nuevos y armar los límites de la importación
    public static List<Barrios> ObtenerBarriosConCoordenadas()
    {
        string consulta = "SELECT Id, Nombre, Latitud, Longitud FROM Barrios ORDER BY Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<Barrios>(consulta).ToList();
        }
    }

    // El barrio del perfil del usuario, con su centro. En Descubrir se usa para medir las distancias
    // cuando el navegador no da la ubicación. null si el usuario no tiene barrio.
    public static Barrios? ObtenerBarrioDeUsuario(int idUsuario)
    {
        string consulta = @"
            SELECT b.Id, b.Nombre, b.Latitud, b.Longitud
            FROM Usuarios u
            JOIN Barrios b ON b.Id = u.IdBarrio
            WHERE u.Id = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Barrios>(consulta, new { idUsuario });
        }
    }

    // ---------- Lugares de Geoapify ----------

    // Guarda un lugar traído de Geoapify. Si ya existe (mismo IdGeoapify) lo actualiza en vez de duplicarlo.
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

        string actualizar = @"
            UPDATE Restaurantes
            SET Nombre = @Nombre, DireccionCalle = @Calle, DireccionNumero = @Numero, DireccionCodigoPostal = @CodigoPostal,
                Latitud = @Latitud, Longitud = @Longitud, idBarrio = @idBarrio, idCategoria = @idCategoria
            WHERE IdGeoapify = @IdGeoapify";
        string insertar = @"
            INSERT INTO Restaurantes (Nombre, DireccionCalle, DireccionNumero, DireccionCodigoPostal, Descripcion, Telefono,
                                      HorarioApertura, HorarioCerrado, RangoPrecio, Foto, idBarrio, idCategoria, EsPro,
                                      SitioWeb, IdGeoapify, Latitud, Longitud, Origen)
            VALUES (@Nombre, @Calle, @Numero, @CodigoPostal, '', '',
                    '19000101', '19000101', 0, NULL, @idBarrio, @idCategoria, 0,
                    NULL, @IdGeoapify, @Latitud, @Longitud, 'Geoapify')";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            if (conexion.Execute(actualizar, datos) > 0) return false;
            conexion.Execute(insertar, datos);
            return true;
        }
    }

    // De dónde salió el lugar: "Manual" o "Geoapify" (decide si se muestran los créditos). null si no existe.
    public static string? ObtenerOrigenRestaurante(int idRestaurante)
    {
        string consulta = "SELECT Origen FROM Restaurantes WHERE Id = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<string>(consulta, new { idRestaurante });
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

    // ---------- 08c · Editar perfil y gustos ----------

    // Lo que carga el modal Editar perfil: nombre, usuario, foto, barrio, frecuencia y "solo lo apto"
    public static Usuarios? ObtenerDatosEdicion(int idUsuario)
    {
        string consulta = @"
            SELECT Id, NombreCompleto, NombreUsuario, FotoPerfil, IdBarrio, Frecuencia, SoloApto
            FROM Usuarios
            WHERE Id = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<Usuarios>(consulta, new { idUsuario });
        }
    }

    // Los Id de los barrios que le interesan al usuario
    public static List<int> ObtenerZonasDeUsuario(int idUsuario)
    {
        string consulta = "SELECT IdBarrio FROM ZonasUsuarios WHERE IdUsuario = @idUsuario ORDER BY IdBarrio";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<int>(consulta, new { idUsuario }).ToList();
        }
    }

    // Con quién sale: "Amigos", "Pareja", etc.
    public static List<string> ObtenerCompaniasDeUsuario(int idUsuario)
    {
        string consulta = "SELECT Compania FROM CompaniasUsuarios WHERE IdUsuario = @idUsuario ORDER BY Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<string>(consulta, new { idUsuario }).ToList();
        }
    }

    // "Solo lo apto para mí". Si todavía no eligió (null) se muestra todo.
    public static bool ObtenerSoloApto(int idUsuario)
    {
        string consulta = "SELECT SoloApto FROM Usuarios WHERE Id = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<bool?>(consulta, new { idUsuario }) == true;
        }
    }

    // Paso 2 del onboarding (02d): guarda "Solo lo apto para mí" (true) o "Ver todo" (false)
    public static void GuardarSoloApto(int idUsuario, bool soloApto)
    {
        string consulta = "UPDATE Usuarios SET SoloApto = @soloApto WHERE Id = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            conexion.Execute(consulta, new { idUsuario, soloApto });
        }
    }

    // true si otro usuario (no el idUsuario) ya tiene ese nombre de usuario
    public static bool ExisteNombreUsuario(string nombreUsuario, int idUsuario)
    {
        string consulta = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @nombreUsuario AND Id <> @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { nombreUsuario, idUsuario }) > 0;
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
                string consulta = @"
                    UPDATE Usuarios
                    SET NombreCompleto = @nombreCompleto, NombreUsuario = @nombreUsuario, IdBarrio = @idBarrio,
                        FotoPerfil = ISNULL(@fotoNueva, FotoPerfil), SoloApto = @soloApto
                    WHERE Id = @idUsuario";
                conexion.Execute(consulta,
                    new { idUsuario, nombreCompleto, nombreUsuario, idBarrio, fotoNueva, soloApto }, transaccion);
                GuardarGustosEnTransaccion(conexion, transaccion, idUsuario, gustos);
                transaccion.Commit();
            }
        }
    }

    // Borra lo que tenía el usuario y guarda lo nuevo (las 4 listas de gustos)
    private static void GuardarGustosEnTransaccion(SqlConnection conexion, SqlTransaction transaccion, int idUsuario, DatosGustos gustos)
    {
        conexion.Execute("UPDATE Usuarios SET Frecuencia = @frecuencia WHERE Id = @idUsuario",
            new { idUsuario, frecuencia = gustos.Frecuencia }, transaccion);

        conexion.Execute("DELETE FROM RestriccionesUsuarios WHERE IdUsuario = @idUsuario", new { idUsuario }, transaccion);
        foreach (int idRestriccion in gustos.Restricciones)
        {
            conexion.Execute("INSERT INTO RestriccionesUsuarios (IdUsuario, IdRestricciones) VALUES (@idUsuario, @idRestriccion)",
                new { idUsuario, idRestriccion }, transaccion);
        }

        conexion.Execute("DELETE FROM ZonasUsuarios WHERE IdUsuario = @idUsuario", new { idUsuario }, transaccion);
        foreach (int idBarrio in gustos.Zonas)
        {
            conexion.Execute("INSERT INTO ZonasUsuarios (IdUsuario, IdBarrio) VALUES (@idUsuario, @idBarrio)",
                new { idUsuario, idBarrio }, transaccion);
        }

        conexion.Execute("DELETE FROM CompaniasUsuarios WHERE IdUsuario = @idUsuario", new { idUsuario }, transaccion);
        foreach (string compania in gustos.Companias)
        {
            conexion.Execute("INSERT INTO CompaniasUsuarios (IdUsuario, Compania) VALUES (@idUsuario, @compania)",
                new { idUsuario, compania }, transaccion);
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

    // Los grupos del usuario (procedimiento ListarGruposUsuario) con la fecha del último mensaje
    // y cuántos mensajes no leyó (usa UsuariosGrupos.UltimaLectura).
    public static List<ResumenChat> ObtenerChatsDeUsuario(int idUsuario)
    {
        List<Grupos> grupos;
        List<ResumenChat> chats = new List<ResumenChat>();
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            grupos = conexion.Query<Grupos>("ListarGruposUsuario",
                new { idUsuario },
                commandType: CommandType.StoredProcedure).ToList();

            foreach (Grupos grupo in grupos)
            {
                ResumenChat chat = new ResumenChat();
                chat.IdGrupo = grupo.Id;
                chat.Nombre = grupo.Nombre;
                chat.Foto = grupo.Foto;
                chat.UltimaFecha = conexion.QueryFirst<DateTime?>(
                    "SELECT MAX(FechaHora) FROM Mensajes WHERE idGrupo = @idGrupo",
                    new { idGrupo = grupo.Id });
                // No leídos = mensajes de otros posteriores a su UltimaLectura
                chat.NoLeidos = conexion.QueryFirst<int>(@"
                    SELECT COUNT(*) FROM Mensajes m
                    JOIN UsuariosGrupos ug ON ug.idGrupo = m.idGrupo AND ug.idUsuario = @idUsuario
                    WHERE m.idGrupo = @idGrupo AND m.idUsuario <> @idUsuario
                      AND (ug.UltimaLectura IS NULL OR m.FechaHora > ug.UltimaLectura)",
                    new { idGrupo = grupo.Id, idUsuario });
                chat.FechaOrden = chat.UltimaFecha ?? grupo.FechaCreacion;
                chats.Add(chat);
            }
        }

        // El chat con el mensaje más nuevo va primero
        chats = chats.OrderByDescending(chat => chat.FechaOrden).ToList();

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
            SELECT m.Id, m.Contenido, m.FechaHora, m.IdUsuario, u.NombreCompleto, u.FotoPerfil, m.IdPlan
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

    // El contador "planes en votación" del banner de Inicio: los de todos los grupos del usuario
    public static int ContarPlanesEnVotacion(int idUsuario)
    {
        string consulta = @"
            SELECT COUNT(*)
            FROM Planes p
            JOIN UsuariosGrupos ug ON ug.IdGrupo = p.IdGrupo AND ug.IdUsuario = @idUsuario
            WHERE p.Estado = 0";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idUsuario });
        }
    }

    // El contador "amigos" del banner de Inicio. Las amistades no tienen tabla en la base,
    // así que cuenta las personas distintas que comparten algún grupo con el usuario.
    public static int ContarAmigos(int idUsuario)
    {
        string consulta = @"
            SELECT COUNT(DISTINCT otros.IdUsuario)
            FROM UsuariosGrupos mios
            JOIN UsuariosGrupos otros ON otros.IdGrupo = mios.IdGrupo AND otros.IdUsuario <> mios.IdUsuario
            WHERE mios.IdUsuario = @idUsuario";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<int>(consulta, new { idUsuario });
        }
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
            SELECT r.Id AS IdRestaurante, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio, r.SitioWeb,
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
            SELECT TOP 6 r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio,
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
            SELECT r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio
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

        string consulta = @"
            SELECT TOP (@cantidad) r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio,
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS Puntaje,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = r.Id)
                        THEN 1 ELSE 0 END AS bit) AS EsFavorito
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            WHERE (@soloApto = 0
               OR NOT EXISTS (SELECT 1 FROM RestriccionesUsuarios WHERE IdUsuario = @idUsuario)
               OR EXISTS (SELECT 1 FROM RestriccionesRestaurantes rr
                          JOIN RestriccionesUsuarios ru ON ru.IdRestricciones = rr.IdRestriccion
                          WHERE rr.IdRestaurante = r.Id AND ru.IdUsuario = @idUsuario))
              AND (@idRestriccion IS NULL
               OR EXISTS (SELECT 1 FROM RestriccionesRestaurantes
                          WHERE IdRestaurante = r.Id AND IdRestriccion = @idRestriccion))
            ORDER BY CASE WHEN (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) IS NULL THEN 1 ELSE 0 END,
                     Puntaje DESC, r.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>(consulta, new { cantidad, idUsuario, soloApto, idRestriccion }).ToList();
        }
    }

    // ---------- 05 · Descubrir ----------

    // Las palabras que no sirven para buscar ("parrilla en Palermo" -> "parrilla", "Palermo")
    private static readonly List<string> PalabrasVacias = new List<string> { "en", "de", "la", "el", "los", "las", "del", "con", "y" };

    // Arma el WHERE de la búsqueda. Cada palabra tiene que aparecer en el nombre, el barrio o la categoría.
    // Si termina en "s" también se prueba sin la "s" ("Parrillas" encuentra "Parrilla").
    // COLLATE ..._CI_AI hace que no importen mayúsculas ni tildes ("nunez" encuentra "Núñez").
    // soloApto: igual que en Inicio, solo los que cumplen al menos una restricción del usuario.
    // idBarrio: el desplegable "Barrio". null = todos los barrios.
    private static string ArmarFiltroBusqueda(string texto, int idUsuario, bool soloApto, int? idBarrio, DynamicParameters parametros)
    {
        string filtro = @"
            WHERE (@soloApto = 0
               OR NOT EXISTS (SELECT 1 FROM RestriccionesUsuarios WHERE IdUsuario = @idUsuario)
               OR EXISTS (SELECT 1 FROM RestriccionesRestaurantes rr
                          JOIN RestriccionesUsuarios ru ON ru.IdRestricciones = rr.IdRestriccion
                          WHERE rr.IdRestaurante = r.Id AND ru.IdUsuario = @idUsuario))
              AND (@idBarrio IS NULL OR r.IdBarrio = @idBarrio)";
        parametros.Add("idUsuario", idUsuario);
        parametros.Add("soloApto", soloApto);
        parametros.Add("idBarrio", idBarrio);

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

    // El Pro que sale destacado ("Recomendado") arriba de todo en la página 1 cuando no hay barrio elegido:
    // el más cercano al usuario entre los que coinciden con la búsqueda. null si no hay ningún Pro.
    public static int? ObtenerProDestacado(string texto, int idUsuario, bool soloApto, double? latitud, double? longitud)
    {
        DynamicParameters parametros = new DynamicParameters();
        string filtro = ArmarFiltroBusqueda(texto, idUsuario, soloApto, null, parametros);
        parametros.Add("latitudUsuario", latitud);
        parametros.Add("longitudUsuario", longitud);

        string consulta = @"
            SELECT TOP 1 r.Id
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio" + filtro + @"
              AND r.EsPro = 1
            ORDER BY CASE WHEN dbo.DistanciaKm(@latitudUsuario, @longitudUsuario, ISNULL(r.Latitud, b.Latitud), ISNULL(r.Longitud, b.Longitud)) IS NULL THEN 1 ELSE 0 END,
                     dbo.DistanciaKm(@latitudUsuario, @longitudUsuario, ISNULL(r.Latitud, b.Latitud), ISNULL(r.Longitud, b.Longitud)),
                     r.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<int?>(consulta, parametros);
        }
    }

    // Una página de resultados.
    // idDestacado: el Pro de ObtenerProDestacado. Va primero (o sea, arriba de la página 1) y no se repite más abajo.
    // orden: "cerca" (del más cerca al más lejos) o "puntaje" (mejor puntuados primero; los empates, por distancia).
    // latitud y longitud: desde dónde se mide (la ubicación del navegador o el centro del barrio del perfil).
    // Llegan como parámetros y no se guardan. Si son null, no hay distancia.
    // Los que no tienen distancia (sin ubicación o el lugar sin coordenadas) o sin reseñas van al final.
    // CROSS APPLY calcula la distancia y el puntaje una sola vez para usarlos en el SELECT y en el ORDER BY.
    // El pin del mapa sale de las coordenadas del lugar o, si no tiene, de las de su barrio.
    public static List<TarjetaRestaurante> BuscarRestaurantes(string texto, int idUsuario, bool soloApto, int? idBarrio,
        double? latitud, double? longitud, string orden, int? idDestacado, int desde, int cantidad)
    {
        DynamicParameters parametros = new DynamicParameters();
        string filtro = ArmarFiltroBusqueda(texto, idUsuario, soloApto, idBarrio, parametros);
        parametros.Add("latitudUsuario", latitud);
        parametros.Add("longitudUsuario", longitud);
        parametros.Add("orden", orden);
        parametros.Add("idDestacado", idDestacado);
        parametros.Add("desde", desde);
        parametros.Add("cantidad", cantidad);

        string consulta = @"
            SELECT r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio, r.EsPro, r.Origen,
                   ISNULL(r.Latitud, b.Latitud) AS Latitud, ISNULL(r.Longitud, b.Longitud) AS Longitud,
                   calculo.DistanciaKm, calculo.Puntaje,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = r.Id)
                        THEN 1 ELSE 0 END AS bit) AS EsFavorito
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            CROSS APPLY (SELECT
                dbo.DistanciaKm(@latitudUsuario, @longitudUsuario, ISNULL(r.Latitud, b.Latitud), ISNULL(r.Longitud, b.Longitud)) AS DistanciaKm,
                (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id AND Fecha <= DATEADD(hour, -24, GETDATE())) AS Puntaje
            ) calculo" + filtro + @"
            ORDER BY CASE WHEN r.Id = @idDestacado THEN 0 ELSE 1 END,
                     CASE WHEN @orden = 'puntaje' AND calculo.Puntaje IS NULL THEN 1 ELSE 0 END,
                     CASE WHEN @orden = 'puntaje' THEN calculo.Puntaje END DESC,
                     CASE WHEN calculo.DistanciaKm IS NULL THEN 1 ELSE 0 END,
                     calculo.DistanciaKm,
                     CASE WHEN calculo.Puntaje IS NULL THEN 1 ELSE 0 END,
                     calculo.Puntaje DESC, r.Id
            OFFSET @desde ROWS FETCH NEXT @cantidad ROWS ONLY";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>(consulta, parametros).ToList();
        }
    }

    // Cuántos resultados hay en total (para la paginación)
    public static int ContarRestaurantes(string texto, int idUsuario, bool soloApto, int? idBarrio)
    {
        DynamicParameters parametros = new DynamicParameters();
        string filtro = ArmarFiltroBusqueda(texto, idUsuario, soloApto, idBarrio, parametros);

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

    // Cuando la búsqueda no da nada en el barrio elegido: el otro barrio donde el texto tiene más resultados
    // (para "No encontramos sushi en Palermo. Hay 3 en Belgrano"). null si no aparece en ningún otro.
    public static BarrioConResultados? ObtenerBarrioConMasResultados(string texto, int idUsuario, bool soloApto, int idBarrioElegido)
    {
        DynamicParameters parametros = new DynamicParameters();
        string filtro = ArmarFiltroBusqueda(texto, idUsuario, soloApto, null, parametros);
        parametros.Add("idBarrioElegido", idBarrioElegido);

        string consulta = @"
            SELECT TOP 1 b.Id, b.Nombre, COUNT(*) AS Cantidad
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            JOIN Barrios b ON b.Id = r.IdBarrio" + filtro + @"
              AND r.IdBarrio <> @idBarrioElegido
            GROUP BY b.Id, b.Nombre
            ORDER BY COUNT(*) DESC, b.Nombre";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirstOrDefault<BarrioConResultados>(consulta, parametros);
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
        string consulta = "SELECT IdRestaurante FROM RestriccionesRestaurantes WHERE IdRestriccion = @idRestriccion";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<int>(consulta, new { idRestriccion }).ToList();
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
