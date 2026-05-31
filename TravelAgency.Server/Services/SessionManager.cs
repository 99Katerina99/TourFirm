using TravelAgency.Core.Entities;
using System.Collections.Concurrent;

namespace TravelAgency.Server.Services;

public class SessionManager
{
    // Token -> User
    private static readonly ConcurrentDictionary<string, User> _sessions = new();

    public string CreateSession(User user)
    {
        var token = Guid.NewGuid().ToString("N");
        _sessions[token] = user;
        return token;
    }

    public bool ValidateToken(string token, out User? user) => _sessions.TryGetValue(token, out user);

    public void RemoveSession(string token) => _sessions.TryRemove(token, out _);
}