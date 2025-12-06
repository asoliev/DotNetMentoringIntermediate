using System.Security.Cryptography;

static string GeneratePasswordHashUsingSalt(string passwordText, byte[] salt)
{
    const int iterate = 10000;
    const int hashSize = 20;
    const int saltSize = 16;

    byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
        passwordText,
        salt,
        iterate,
        HashAlgorithmName.SHA512,
        hashSize
    );

    Span<byte> hashBytes = stackalloc byte[saltSize + hashSize];
    salt.CopyTo(hashBytes);
    hash.CopyTo(hashBytes[saltSize..]);

    return Convert.ToBase64String(hashBytes);//passwordHash
}

byte[] saltBytes = new byte[16];
using var rng = RandomNumberGenerator.Create();
rng.GetBytes(saltBytes);
Console.WriteLine($"Generated Salt (Base64): {Convert.ToBase64String(saltBytes)}");

string result = GeneratePasswordHashUsingSalt("MySecure", saltBytes);
Console.WriteLine($"passwordHash: {result}");

Console.ReadKey();