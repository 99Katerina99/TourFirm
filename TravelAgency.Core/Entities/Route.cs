namespace TravelAgency.Core.Entities;

public class Route
{
    public int Id { get; set; }
    public string Name { get; set; }           // например "Москва-Париж-Рим"
    public string Description { get; set; }    // описание маршрута

    // Навигационное свойство
    public ICollection<Tour> Tours { get; set; }
}