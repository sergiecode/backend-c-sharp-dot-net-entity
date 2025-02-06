namespace BackendUsuarios.Models.ProductTypes;
public class ProductType : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public bool isActive { get; set; } = true;
}
