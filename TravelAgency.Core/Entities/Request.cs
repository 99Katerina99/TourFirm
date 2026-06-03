public class Request
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Новая";  // Новая, В обработке, Подтверждена
    public string Wishes { get; set; }

    public Client Client { get; set; }
    public ICollection<ClientContract> ClientContracts { get; set; }
}