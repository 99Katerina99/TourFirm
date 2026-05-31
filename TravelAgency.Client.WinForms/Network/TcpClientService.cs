using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TravelAgency.Client.WinForms.Network;

public class TcpClientService : IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamWriter? _writer;
    private StreamReader? _reader;

    public string? CurrentToken { get; private set; }
    public bool IsConnected => _client?.Connected == true;

    public async Task<bool> ConnectAsync(string host, int port)
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _writer = new StreamWriter(_stream, new UTF8Encoding(false), 8192) { AutoFlush = true };

            _reader = new StreamReader(_stream, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> SendCommandAsync(string command)
    {
        if (!IsConnected || _writer == null || _reader == null)
            throw new InvalidOperationException("Не подключено к серверу");

        await _writer.WriteLineAsync(command);
        var response = await _reader.ReadLineAsync();
        return response ?? "ERROR|Пустой ответ";
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await SendCommandAsync($"LOGIN|{username}|{password}");
        if (response.StartsWith("OK|"))
        {
            var json = response.Substring(3);
            var authResult = JsonSerializer.Deserialize<AuthResponse>(json);
            if (authResult?.Token != null)
            {
                CurrentToken = authResult.Token;
                return true;
            }
        }
        return false;
    }

    public void Disconnect()
    {
        _writer?.Close();
        _reader?.Close();
        _stream?.Close();
        _client?.Close();
        CurrentToken = null;
    }

    public void Dispose() => Disconnect();
}

// Модель для ответа авторизации
public class AuthResponse
{
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
}