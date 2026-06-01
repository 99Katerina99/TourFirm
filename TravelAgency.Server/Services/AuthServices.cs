using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;
using TravelAgency.Data.Helpers;

namespace TravelAgency.Server.Services;

public class AuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly SessionManager _sessionManager;

    // Конструктор принимает репозиторий (для тестов мы будем его мокать)
    public AuthService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
        _sessionManager = new SessionManager();
    }

    /// <summary>
    /// Проверяет логин/пароль и возвращает токен, если всё верно.
    /// </summary>
    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        // Ищем пользователя в БД
        var users = await _userRepository.FindAsync(u => u.Username == username);
        var user = users.FirstOrDefault();

        // Если пользователя нет или пароль не совпадает — возвращаем null
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        // Создаём сессию и возвращаем токен
        var token = _sessionManager.CreateSession(user);
        return token;
    }

    /// <summary>
    /// Проверяет, валиден ли токен (существует ли такая сессия).
    /// </summary>
    public Task<bool> ValidateTokenAsync(string token)
    {
        bool isValid = _sessionManager.ValidateToken(token, out _);
        return Task.FromResult(isValid);
    }
}