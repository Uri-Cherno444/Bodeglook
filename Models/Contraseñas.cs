using System.Security.Cryptography;

namespace Bodeglook.Models;

public static class Contraseñas
{
    public static string Hashear(string contraseña, string email)
    {
        byte[] sal = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(email.Trim().ToLower()));
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, 100000, HashAlgorithmName.SHA256, 24);
        return Convert.ToBase64String(hash);
    }
}
