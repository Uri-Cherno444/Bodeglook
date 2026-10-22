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
    public static List<TarjetaRestaurante> ObtenerMejorPuntuados(int cantidad, int idUsuario)
    {
        string consulta = @"
            SELECT TOP (@cantidad) r.Id, r.Nombre, r.Foto, c.Nombre AS Categoria, b.Nombre AS Barrio,
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id) AS Puntaje,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Favoritos WHERE IdUsuario = @idUsuario AND IdRestaurante = r.Id)
                        THEN 1 ELSE 0 END AS bit) AS EsFavorito
            FROM Restaurantes r
            LEFT JOIN Categorias c ON c.Id = r.IdCategoria
            LEFT JOIN Barrios b ON b.Id = r.IdBarrio
            ORDER BY CASE WHEN (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id) IS NULL THEN 1 ELSE 0 END,
                     Puntaje DESC, r.Id";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.Query<TarjetaRestaurante>(consulta, new { cantidad, idUsuario }).ToList();
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
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id) AS Puntaje,
                   (SELECT COUNT(*) FROM Reseñas WHERE IdRestaurante = r.Id) AS CantidadResenas,
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
            FROM Reseñas WHERE IdRestaurante = @idRestaurante";
        using (SqlConnection conexion = new SqlConnection(CadenaConexion))
        {
            return conexion.QueryFirst<DetalleRestaurante>(consulta, new { idRestaurante });
        }
    }

    // ---------- Reseñas ----------

    // Las reseñas de un lugar de la más nueva a la más vieja, salteando las primeras "desde".
    // cantidad = null trae todas las que quedan.
    public static List<ResenaComunidad> ObtenerResenasDeRestaurante(int idRestaurante, int desde, int? cantidad)
    {
        string consulta = @"
            SELECT re.Id, re.Puntaje, re.Comentario, re.Fecha, u.NombreCompleto
            FROM Reseñas re
            JOIN Usuarios u ON u.Id = re.IdUsuario
            WHERE re.IdRestaurante = @idRestaurante
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
                   (SELECT AVG(CAST(Puntaje AS float)) FROM Reseñas WHERE IdRestaurante = r.Id) AS Puntaje,
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
