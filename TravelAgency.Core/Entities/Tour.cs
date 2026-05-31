namespace TravelAgency.Core.Entities;

public class Tour
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsActive { get; set; } = true;
}