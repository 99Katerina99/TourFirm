namespace TravelAgency.Core.Entities;

public class TourExtendedDto
{
    public int TourId { get; set; }
    public string TourName { get; set; } = string.Empty;
    public string TourType { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int MaxSeats { get; set; }

    // Клиент
    public string? ClientName { get; set; }
    public string? ClientPhone { get; set; }

    // Договор с клиентом
    public string? ClientContractNumber { get; set; }
    public string? ClientContractStatus { get; set; }

    // Поставщик
    public string? SupplierName { get; set; }
    public string? SupplierContractNumber { get; set; }

    // Бронирование
    public string? BookingNumber { get; set; }
    public string? BookingStatus { get; set; }

    // Платёж
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal? PaymentAmount { get; set; }
}