using Data.Entities;
using Data.Enums;
using OrderProvider.Models;

namespace OrderProvider.Factories;

public static class OrderFactory
{
    public static OrderEntity CreateOrderEntity(OrderRequest request)
    {
        return new()
        {
            UserId = request.UserId,
            Products = [],
            DeliveryFee = request.DeliveryFee,
            RecipientCO = request.RecipientCO,
            Address = request.Address,
            ZipCode = request.ZipCode,
            City = request.City,
            Country = request.Country,
            Status = OrderStatus.Registered,
            Created = DateTime.Now,
        };
    }
}
