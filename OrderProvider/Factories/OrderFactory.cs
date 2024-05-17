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

    public static List<OrderResponse> GetOrders(List<OrderEntity> orders)
    {
        List<OrderResponse> orderResponses = [];

        foreach(OrderEntity order in orders)
        {
            orderResponses.Add(new OrderResponse()
            {
                Id = order.Id,
                Status = order.Status,
                Created = order.Created,
                Products = GetProductResponse(order.Products)
            });
        }
        return orderResponses;
    }

    private static List<OrderProductResponse> GetProductResponse(List<OrderProductEntity> products)
    {
        List<OrderProductResponse> response = [];

        foreach (OrderProductEntity product in products)
        {
            response.Add(new OrderProductResponse()
            {
                Id = product.ProductId,
                Name = product.Name,
                Count = product.Count,
                UnitPrice = product.UnitPrice,
            });
        }
        return response;
    }

    public static List<OrderProductEntity> CreateOrderProductEntities(List<OrderProductRequest> products)
    {
        List<OrderProductEntity> response = [];

        foreach (OrderProductRequest product in products)
        {
            response.Add(new OrderProductEntity()
            {
                ProductId = product.Id,
                Name = product.ProductName,
                UnitPrice = product.Price,
            });
        }
        return response;
    }
}
