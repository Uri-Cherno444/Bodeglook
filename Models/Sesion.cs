using System.Text.Json;

namespace Bodeglook.Models;

// No es una tabla: guarda y lee el usuario logueado en la Session (como JSON, sin la contraseña).
// La usan todos los controllers y el _Layout.
public static class Sesion
{
    private const string Clave = "usuario";

    public static bool HayUsuario(ISession sesion)
    {
        return sesion.GetString(Clave) != null;
    }

    // null si no hay nadie logueado
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
