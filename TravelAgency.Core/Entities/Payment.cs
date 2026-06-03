public class Payment
{
    public int Id { get; set; }
    public int ClientContractId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; }  // "Наличные", "Карта", "Перевод"
    public string PaymentStatus { get; set; } = "Ожидается";

    public ClientContract ClientContract { get; set; }
}