using System.Text.Json;
using TravelAgency.Core.Entities;
using TravelAgency.Core.Interfaces;
using TravelAgency.Server.Services;

namespace TravelAgency.Server.Commands;

public static class TourCommands
{
    // --- ORM ВЕРСИЯ ---
    // GET_TOURS_ORM|token
    public static string GetToursOrm(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var tours = repo.GetAllAsync().Result;
        return $"OK|{JsonSerializer.Serialize(tours)}";
    }

    // ADD_TOUR_ORM|token|Name|Desc|Country|City|Days|Price|StartDate|Seats
    public static string AddTourOrm(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 10 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        var tour = new Tour
        {
            Name = parts[2],
            Description = parts[3],
            Country = parts[4],
            City = parts[5],
            DurationDays = int.Parse(parts[6]),
            Price = decimal.Parse(parts[7]),
            StartDate = DateTime.Parse(parts[8]),
            AvailableSeats = int.Parse(parts[9])
        };

        repo.AddAsync(tour).Wait();
        return $"OK|Тур добавлен. ID: {tour.Id}";
    }

    // --- SQL ВЕРСИЯ ---
    // GET_TOURS_SQL|token
    public static string GetToursSql(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 2 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация";

        var tours = repo.GetAllAsync().Result;
        return $"OK|{JsonSerializer.Serialize(tours)}";
    }

    // ADD_TOUR_SQL|token|Name|Desc|Country|City|Days|Price|StartDate|Seats
    public static string AddTourSql(string[] parts, IRepository<Tour> repo, SessionManager sessionManager)
    {
        if (parts.Length < 10 || !sessionManager.ValidateToken(parts[1], out _))
            return "ERROR|Требуется авторизация или неверные параметры";

        var tour = new Tour
        {
            Name = parts[2],
            Description = parts[3],
            Country = parts[4],
            City = parts[5],
            DurationDays = int.Parse(parts[6]),
            Price = decimal.Parse(parts[7]),
            StartDate = DateTime.Parse(parts[8]),
            AvailableSeats = int.Parse(parts[9])
        };

        repo.AddAsync(tour).Wait();
        return $"OK|Тур добавлен (SQL). ID: {tour.Id}";
    }
}