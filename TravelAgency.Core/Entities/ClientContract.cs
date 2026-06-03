using TravelAgency.Core.Entities;

public class ClientContract
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int TourId { get; set; }
    public DateTime ContractDate { get; set; } = DateTime.UtcNow;
    public string ContractNumber { get; set; }  // например "CN-2026-001"
    public string Status { get; set; } = "Черновик";
    public decimal TotalCost { get; set; }

    public Request Request { get; set; }
    public Tour Tour { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<Booking> Bookings { get; set; }
}