public class Booking
{
    public int Id { get; set; }
    public int ClientContractId { get; set; }
    public int SupplierContractId { get; set; }
    public string BookingNum { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Забронировано";

    public ClientContract ClientContract { get; set; }
    public SupplierContract SupplierContract { get; set; }
}