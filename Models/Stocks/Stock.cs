using BackendUsuarios.Models.Products;

namespace BackendUsuarios.Models.Stocks;
public class Stock
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdateDate { get; set; } = DateTime.Now;
}