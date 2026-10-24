using System.Text.RegularExpressions;

namespace CoffeeShop.Helpers;

public static class PasswordHashing
{
    private static readonly Regex BcryptHashPattern = new(
        @"\A\$2[aby]\$(?:0[4-9]|[12]\d|3[01])\$[./A-Za-z0-9]{53}\z",
        RegexOptions.CultureInvariant);

    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool Verify(string password, string? storedPassword, out bool needsRehash)
    {
        needsRehash = false;
        if (string.IsNullOrEmpty(storedPassword))
        {
            return false;
        }

        if (BcryptHashPattern.IsMatch(storedPassword))
        {
            return BCrypt.Net.BCrypt.Verify(password, storedPassword);
        }

        needsRehash = true;
        return password == storedPassword;
    }
}
