namespace Bodeglook.Models;

public class NuevoRestaurante
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Contraseña { get; set; }
    public int? IdBarrio { get; set; }
    public int? IdCategoria { get; set; }
    public string? Plan { get; set; }

    public static string PlanInicial(string? plan)
    {
        return plan == "pro" ? "pro" : "gratis";
    }

    public object Registrar()
    {
        string nombre = (Nombre ?? "").Trim();
        string email = (Email ?? "").Trim();
        string contraseña = Contraseña ?? "";

        if (nombre == "")
            return new { ok = false, campo = "nombre", mensaje = "Completá el nombre del restaurante." };
        if (nombre.Length > 100)
            return new { ok = false, campo = "nombre", mensaje = "El nombre puede tener hasta 100 caracteres." };
        if (email == "" || !email.Contains('@') || email.Length > 100)
            return new { ok = false, campo = "email", mensaje = "Ingresá un email válido." };
        if (contraseña == "")
            return new { ok = false, campo = "contrasena", mensaje = "Elegí una contraseña." };
        if (contraseña.Length < 8)
            return new { ok = false, campo = "contrasena", mensaje = "La contraseña tiene que tener al menos 8 caracteres." };
        if (IdBarrio == null)
            return new { ok = false, campo = "barrio", mensaje = "Elegí un barrio." };
        if (IdCategoria == null)
            return new { ok = false, campo = "categoria", mensaje = "Elegí una categoría." };
        if (Plan != "gratis" && Plan != "pro")
            return new { ok = false, campo = "aviso", mensaje = "Elegí un plan." };

        int error = BD.RegistrarRestaurante(nombre, email, Contraseñas.Hashear(contraseña, email),
            IdBarrio.Value, IdCategoria.Value, Plan == "pro");
        if (error == 1) return new { ok = false, campo = "email", mensaje = "Ese email ya tiene una cuenta." };
        if (error == 2) return new { ok = false, campo = "barrio", mensaje = "Elegí un barrio de la lista." };
        if (error == 3) return new { ok = false, campo = "categoria", mensaje = "Elegí una categoría de la lista." };

        return new { ok = true, destino = "/Cuenta/IniciarSesion?restaurante=1" };
    }
}
