using BCrypt.Net;

namespace TravelAgency.Data.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public static bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}