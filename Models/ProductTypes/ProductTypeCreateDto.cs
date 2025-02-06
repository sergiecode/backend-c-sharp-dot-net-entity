public class ProductTypeCreateDto
{
    public Guid ProductId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool isActive { get; set; } = true;
}
