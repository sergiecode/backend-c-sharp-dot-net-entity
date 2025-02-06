public class ProductTypeUpdateDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool isActive { get; set; } = true;
}
