using System.Text.Json;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;
using TravelAgency.Data.Helpers;
using TravelAgency.Server.Services;

namespace TravelAgency.Server.Commands;

public static class AuthCommands
{
    // Формат: LOGIN|username|password
    public static string Login(string[] parts, IRepository<User> repo, SessionManager sessionManager)
    {
        if (parts.Length < 3) return "ERROR|Использование: LOGIN|username|password";

        var username = parts[1];
        var password = parts[2];

        var users = repo.FindAsync(u => u.Username == username).Result;
        var user = users.FirstOrDefault();

        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            return "ERROR|Неверный логин или пароль";

        var token = sessionManager.CreateSession(user);
        var response = new { Token = token, Username = user.Username, Role = user.Role };
        return $"OK|{JsonSerializer.Serialize(response)}";
    }
}