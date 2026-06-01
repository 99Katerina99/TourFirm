using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

// 🔥 Алиас для пространства имён с нашим контекстом
using AppDb = TravelAgency.Data.DbContext;

using TravelAgency.Data.Repositories;
using TravelAgency.Server.Commands;
using TravelAgency.Server.Services;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;

namespace TravelAgency.Server;

class Program
{
    private static string _connectionString = string.Empty;

    // 🔥 Используем алиас в объявлении переменной
    private static AppDb.TravelAgencyDbContext _dbContext = null!;

    private static IRepository<User> _userRepo = null!;
    private static IRepository<Tour> _efTourRepo = null!;
    private static IRepository<Tour> _sqlTourRepo = null!;
    private static readonly SessionManager _sessionManager = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("⏳ Инициализация сервера...");

        // 1. Конфигурация и БД
        _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"]?.ConnectionString
                            ?? "Data Source=travel_agency.db;";

        // 🔥 Используем алиас при создании
        _dbContext = new AppDb.TravelAgencyDbContext(_connectionString);
        await _dbContext.Database.EnsureCreatedAsync();

        // 2. Репозитории — передаём контекст с алиасом
        _userRepo = new EfRepository<User>(_dbContext);
        _efTourRepo = new EfRepository<Tour>(_dbContext);
        _sqlTourRepo = new SqlTourRepository(_connectionString);

        // 3. Создаём дефолтного админа если нет
        await EnsureAdminUserAsync();

        // 4. Запуск TCP
        var port = int.Parse(ConfigurationManager.AppSettings["ServerPort"] ?? "8888");
        var server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine($"🚀 Сервер запущен на порту {port}. Ожидание подключений...\n");

        // 5. Цикл приёма клиентов
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            Console.WriteLine($"✅ Подключился клиент: {client.Client.RemoteEndPoint}");

            // Обрабатываем каждого клиента в отдельном потоке
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        var buffer = new byte[8192];

        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Клиент отключился

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                request = request.Trim('\uFEFF'); // Удаляем BOM
                Console.WriteLine($"📥 Запрос: {request}");

                string response = ProcessCommand(request);
                Console.WriteLine($"📤 Ответ: {response}\n");

                byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка обработки: {ex.Message}");
            await SendResponseAsync(stream, $"ERROR|{ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("🔌 Клиент отключился\n");
        }
    }

    private static string ProcessCommand(string request)
    {
        if (string.IsNullOrWhiteSpace(request)) return "ERROR|Пустой запрос";

        var parts = request.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToUpper();

        return command switch
        {
            "TEST" => "OK|Сервер работает!",
            "LOGIN" => AuthCommands.Login(parts, _userRepo, _sessionManager),

            // Туры: ORM версия
            "GET_TOURS_ORM" => TourCommands.GetToursOrm(parts, _efTourRepo, _sessionManager),
            "ADD_TOUR_ORM" => TourCommands.AddTourOrm(parts, _efTourRepo, _sessionManager),

            // Туры: SQL версия
            "GET_TOURS_SQL" => TourCommands.GetToursSql(parts, _sqlTourRepo, _sessionManager),
            "ADD_TOUR_SQL" => TourCommands.AddTourSql(parts, _sqlTourRepo, _sessionManager),

            _ => "ERROR|Неизвестная команда"
        };
    }

    private static async Task EnsureAdminUserAsync()
    {
        var admin = (await _userRepo.FindAsync(u => u.Username == "admin")).FirstOrDefault();
        if (admin == null)
        {
            await _userRepo.AddAsync(new User
            {
                Username = "admin",
                PasswordHash = TravelAgency.Data.Helpers.PasswordHelper.HashPassword("admin123"),
                Role = "admin"
            });
            Console.WriteLine("🔑 Создан дефолтный админ: admin / admin123");
        }
    }

    private static async Task SendResponseAsync(NetworkStream stream, string response)
    {
        var bytes = Encoding.UTF8.GetBytes(response + "\n");
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }
}