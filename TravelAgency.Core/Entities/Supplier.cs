public class Supplier
{
    public int Id { get; set; }
    public string SupplierType { get; set; }  // "Отель", "Авиакомпания", "Туроператор"
    public string Name { get; set; }
    public string ContractPerson { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    public ICollection<SupplierContract> SupplierContracts { get; set; }
}