using System.Security.Cryptography;
using System.Text;

// Este es un programa auxiliar para generar hashes SHA256 en Base64
// Uso: dotnet script.csx "password"

string password = args.Length > 0 ? args[0] : "admin123";

using (var sha256 = SHA256.Create())
{
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    var base64Hash = Convert.ToBase64String(hashedBytes);
    Console.WriteLine($"Password: {password}");
    Console.WriteLine($"Base64 Hash: {base64Hash}");
}
