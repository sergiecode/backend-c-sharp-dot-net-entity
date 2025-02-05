using BackendUsuarios.Models.Products;

namespace BackendUsuarios.Models.ProductTypes;
public class ProductType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool isActive { get; set; } = true;
}
