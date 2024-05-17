using Data.Enums;

namespace OrderProvider.Models;
public class OrderResponse
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime Created { get; set; }
    public List<OrderProductResponse> Products { get; set; } = [];
}

