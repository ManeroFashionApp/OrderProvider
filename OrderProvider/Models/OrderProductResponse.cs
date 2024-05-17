namespace OrderProvider.Models;

public class OrderProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Count { get; set; }
    public decimal UnitPrice { get; set; }


}
