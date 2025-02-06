using BackendUsuarios.Models.Products;

namespace BackendUsuarios.Models.Stocks;
public class Stock : BaseEntity
{
    public int Quantity { get; set; }
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
}