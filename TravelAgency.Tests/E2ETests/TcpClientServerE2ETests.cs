using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using TravelAgency.Data.Helpers;
using Xunit;

namespace TravelAgency.Tests.E2ETests;

public class TcpClientServerE2ETests : IAsyncLifetime
{
    private readonly int _port = 19999;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    // 🔥 Статические поля — общие для всех подключений в рамках теста
    private static readonly Dictionary<string, string> _testUsers = new()
    {
        ["e2eadmin"] = PasswordHelper.HashPassword("admin123")
    };
    private static readonly List<string> _testTours = new();

    public Task InitializeAsync()
    {
        _testTours.Clear(); // Очищаем между тестами
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Loopback, _port);
        _listener.Start();
        _ = Task.Run(() => AcceptClientsLoopAsync(_cts.Token));
        return Task.CompletedTask; // Не нужно ждать, сервер готов сразу
    }

    public async Task DisposeAsync()
    {
        _cts?.Cancel();
        _listener?.Stop();

        // 🔥 Ждём завершения задач, чтобы освободить ресурсы
        await Task.Delay(100);
        _cts?.Dispose();
    }

    private async Task AcceptClientsLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!_listener.Pending()) { await Task.Delay(50, token); continue; }
            var client = await _listener.AcceptTcpClientAsync(token);
            _ = HandleClientAsync(client, token);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using var stream = client.GetStream();
        var buffer = new byte[2048];

        try
        {
            while (!token.IsCancellationRequested)
            {
                var read = await stream.ReadAsync(buffer, token);
                if (read == 0) break;

                var req = Encoding.UTF8.GetString(buffer, 0, read).Trim().Trim('\uFEFF');
                var parts = req.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts.Length > 0 ? parts[0].ToUpper() : "";

                string resp = cmd switch
                {
                    "TEST" => "OK|E2E Server Ready",

                    "LOGIN" when parts.Length >= 3 &&
                                _testUsers.TryGetValue(parts[1], out var hash) &&
                                BCrypt.Net.BCrypt.Verify(parts[2], hash)
                        => $"OK|{{\"Token\":\"e2e_tok\",\"Username\":\"{parts[1]}\",\"Role\":\"admin\"}}",

                    "LOGIN" => "ERROR|Invalid credentials",

                    // 🔥 ИСПРАВЛЕНО: parts[1] = название тура, parts[2] = страна
                    "ADD_TOUR" when parts.Length >= 3
                        => AddTourResponse(parts[1]),  // ✅ Было: parts[2]

                    "GET_TOURS" => $"OK|{JsonSerializer.Serialize(_testTours)}",

                    _ => "ERROR|Unknown command"
                };

                var respBytes = Encoding.UTF8.GetBytes(resp + "\n");
                await stream.WriteAsync(respBytes, 0, respBytes.Length, token);
            }
        }
        catch
        {
            // Игнорируем ошибки при закрытии
        }
        finally
        {
            client.Close();
        }
    }

    private static string AddTourResponse(string tourName)
    {
        lock (_testTours) // 🔥 Блокировка для потокобезопасности
        {
            _testTours.Add(tourName);
        }
        return "OK|Tour added";
    }

    private async Task<string> SendCommandAsync(string command)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port);
        using var stream = client.GetStream();

        var bytes = Encoding.UTF8.GetBytes(command + "\n");
        await stream.WriteAsync(bytes, 0, bytes.Length);

        var buf = new byte[2048];
        var read = await stream.ReadAsync(buf, 0, buf.Length);
        return Encoding.UTF8.GetString(buf, 0, read).Trim();
    }

    // ==================== 🧪 ТЕСТЫ ====================

    [Fact]
    public async Task E2E_Authentication_ShouldReturnToken_WhenCredentialsValid()
    {
        var response = await SendCommandAsync("LOGIN|e2eadmin|admin123");

        Assert.StartsWith("OK|", response);
        var json = response.Substring(3);
        var result = JsonSerializer.Deserialize<AuthResult>(json);

        Assert.NotNull(result);
        Assert.Equal("e2eadmin", result.Username);
        Assert.Equal("e2e_tok", result.Token);
    }

    [Fact]
    public async Task E2E_TourLifecycle_ShouldAddAndRetrieveSuccessfully()
    {
        // Очищаем список перед тестом (на случай параллельного запуска)
        lock (_testTours) { _testTours.Clear(); }

        var loginResp = await SendCommandAsync("LOGIN|e2eadmin|admin123");
        Assert.StartsWith("OK", loginResp);

        var addResp = await SendCommandAsync("ADD_TOUR|Париж|Франция|5");
        Assert.Equal("OK|Tour added", addResp);

        var getResp = await SendCommandAsync("GET_TOURS");
        Assert.StartsWith("OK|", getResp);

        var toursJson = getResp.Substring(3);
        var tours = JsonSerializer.Deserialize<List<string>>(toursJson);

        Assert.NotNull(tours);
        Assert.Single(tours);
        Assert.Equal("Париж", tours[0]);
    }

    private class AuthResult
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }
}