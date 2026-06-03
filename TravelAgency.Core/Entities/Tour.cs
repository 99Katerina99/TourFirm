namespace TravelAgency.Core.Entities;

public class Tour
{
    public int Id { get; set; }
    public int RouteId { get; set; }              // FK на Routes
    public string Name { get; set; } = string.Empty;
    public string TourType { get; set; } = string.Empty;  // "Круизный", "Курортный"...
    public string Destination { get; set; } = string.Empty;  // "Франция, Париж"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int MaxSeats { get; set; }
    public string Description { get; set; } = string.Empty;

    // Навигационное свойство (опционально)
    public Route? Route { get; set; }
    public ICollection<ClientContract> ClientContracts { get; set; } = new List<ClientContract>();
    public ICollection<SupplierContract> SupplierContracts { get; set; } = new List<SupplierContract>();
}