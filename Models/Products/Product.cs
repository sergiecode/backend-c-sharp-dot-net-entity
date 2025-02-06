using BackendUsuarios.Models.Categories;
using BackendUsuarios.Models.ProductTypes;
using BackendUsuarios.Models.Stocks;
using BackendUsuarios.Models.Suppliers;

namespace BackendUsuarios.Models.Products;

public class Product : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid StockId { get; set; }
    public Stock? Stock { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public Guid ProductTypeId { get; set; }
    public ProductType? ProductType { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
}