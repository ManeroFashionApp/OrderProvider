namespace OrderProvider.Models;

public class OrderProductRequest
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = null!;
    public int Count { get; set; }
    public decimal Price { get; set; }
}
