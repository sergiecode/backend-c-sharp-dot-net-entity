namespace BackendUsuarios.Models;

public class BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
