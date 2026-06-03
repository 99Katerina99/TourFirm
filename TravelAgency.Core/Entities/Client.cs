public class Client
{
    public int Id { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }

    // Паспортные данные (детализировано)
    public string PassportSeries { get; set; }      // например "4512"
    public string PassportNumber { get; set; }       // например "123456"
    public string PassportIssuedBy { get; set; }     // кем выдан
    public DateTime? PassportIssuedDate { get; set; } // когда выдан

    public string Phone { get; set; }
    public string Email { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public ICollection<Request> Requests { get; set; }
}