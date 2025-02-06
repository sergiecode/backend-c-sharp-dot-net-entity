public class ProductCreateDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProductTypeId { get; set; }
    public Guid StockId { get; set; }
    public Guid SupplierId { get; set; }
}
