namespace BackendUsuarios.Models.Suppliers;
public class Supplier : BaseEntity
{
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}