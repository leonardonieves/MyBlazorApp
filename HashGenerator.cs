using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        GenerateHash("admin123");
        GenerateHash("user123");
    }

    static void GenerateHash(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var base64Hash = Convert.ToBase64String(hashedBytes);
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Base64 Hash: {base64Hash}");
            Console.WriteLine();
        }
    }
}
