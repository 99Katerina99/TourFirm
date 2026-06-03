using TravelAgency.Core.Entities;

public class SupplierContract
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int TourId { get; set; }
    public DateTime ContractDate { get; set; } = DateTime.UtcNow;
    public string Service { get; set; }  // "Проживание", "Перелёт", "Экскурсии"
    public string ConfirmationStatus { get; set; } = "Ожидает подтверждения";
    public decimal Cost { get; set; }

    public Supplier Supplier { get; set; }
    public Tour Tour { get; set; }
    public ICollection<Booking> Bookings { get; set; }
}