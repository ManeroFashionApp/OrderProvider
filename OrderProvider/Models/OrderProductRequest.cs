namespace OrderProvider.Models;

public class OrderProductRequest
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
}
