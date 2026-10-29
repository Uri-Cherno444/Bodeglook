using System.Security.Cryptography;

namespace Bodeglook.Models;

// Los procedimientos de la base comparan la contraseña tal cual está guardada,
// así que el hash tiene que dar siempre lo mismo para la misma cuenta. Por eso la sal sale del email:
// dos personas con la misma contraseña igual no tienen el mismo hash.
// Hash de 24 bytes = 32 caracteres en Base64, entra en varchar(50).
public static class Contraseñas
{
    public static string Hashear(string contraseña, string email)
    {
        byte[] sal = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(email.Trim().ToLower()));
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, 100000, HashAlgorithmName.SHA256, 24);
        return Convert.ToBase64String(hash);
    }
}
