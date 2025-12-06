using System.Security.Cryptography;

static string GeneratePasswordHashUsingSalt(string passwordText, byte[] salt)
{
    int iterate = 10000;
    byte[] hash = Rfc2898DeriveBytes.Pbkdf2(passwordText, salt, iterate, HashAlgorithmName.SHA512, 20);
    byte[] hashBytes = new byte[36];
    Array.Copy(salt, 0, hashBytes, 0, 16);
    Array.Copy(hash, 0, hashBytes, 16, 20);
    string passwordHash = Convert.ToBase64String(hashBytes);
    return passwordHash;
}

byte[] saltBytes = new byte[16];
using var rng = RandomNumberGenerator.Create();
rng.GetBytes(saltBytes);
Console.WriteLine($"Generated Salt (Base64): {Convert.ToBase64String(saltBytes)}");

string result = GeneratePasswordHashUsingSalt("MySecure", saltBytes);
Console.WriteLine($"passwordHash: {result}");
Console.ReadKey();