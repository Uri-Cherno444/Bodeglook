using System.Text.Json;

namespace Bodeglook.Models;

public static class Sesion
{
    private const string Clave = "usuario";

    public static bool HayUsuario(ISession sesion)
    {
        return sesion.GetString(Clave) != null;
    }

    public static Usuarios? Leer(ISession sesion)
    {
        string? json = sesion.GetString(Clave);
        if (json == null) return null;
        return JsonSerializer.Deserialize<Usuarios>(json);
    }

    public static void Guardar(ISession sesion, Usuarios usuario)
    {
        usuario.Contraseña = null;
        sesion.SetString(Clave, JsonSerializer.Serialize(usuario));
    }

    public static void Cerrar(ISession sesion)
    {
        sesion.Clear();
    }
}
