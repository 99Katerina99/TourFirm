namespace TravelAgency.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int TourId { get; set; }
    public int UserId { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public int PersonsCount { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "pending"; // pending | confirmed | cancelled
}