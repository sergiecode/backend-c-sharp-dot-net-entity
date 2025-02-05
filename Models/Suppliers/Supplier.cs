using BackendUsuarios.Models.Products;

namespace BackendUsuarios.Models.Suppliers;
public class Supplier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}