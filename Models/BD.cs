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
            ORDER BY CASE WHEN (SELECT MAX(FechaHora) FROM Mensajes WHERE IdGrupo = g.Id) IS NULL THEN 1 ELSE 0 END,
                     UltimaFecha DESC";
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

    // ---------- Planes ----------

    // Los planes de todos los grupos del usuario.
    // Planes.Estado es un bit: 0 = En votación, 1 = Confirmado. confirmado = null trae los dos.
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
            WHERE @confirmado IS NULL OR p.Estado = @confirmado
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
