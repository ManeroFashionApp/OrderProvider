using Data.Entities;
using Data.Enums;
using OrderProvider.Factories;
using OrderProvider.Models;

namespace OrderProvider.Services;

public class OrderService
{
    #region CREATE
    //public OrderRequest CreateOrder(OrderRequest request, Guid userId)
    //{
        
    //    OrderEntity order = new()
    //    {
    //        UserId = userId,
    //        Products = [],
    //        DeliveryFee = request.DeliveryFee,
    //        RecipientCO = request.RecipientCO,
    //        Address = request.Address,
    //        ZipCode = request.ZipCode,
    //        City = request.City,
    //        Country = request.Country,
    //        Status = OrderStatus.Registered,
    //        Created = DateTime.Now,
    //    };

    //    Dictionary<string, int> productDic = [];

    //    foreach (string productId in request.Products)
    //    {
    //        if (productDic.ContainsKey(productId))
    //            productDic[productId]++;
    //        else
    //            productDic.Add(productId, 1);
    //    }

    //    decimal orderTotalPrice = 0;
    //    foreach(var key in productDic.Keys)
    //    {
    //        //send a request for the product that matches "key" to ProductProvider to get price
    //        decimal price = 0;

    //        order.Products.Add(new OrderProductEntity { OrderId = order.Id, ProductId = Guid.Parse(key), Count = productDic[key], UnitPrice = price });
    //        orderTotalPrice += price* productDic[key];
    //    }

    //    //check if orderTotalPrice == request.TotalPrice ==> create Order in DB, return bool/orderResponse
    //    //else return BadRequest
    //    return new();
    //}

    public OrderRequest CreateOrder(OrderRequest request)
    {

        OrderEntity order = OrderFactory.CreateOrderEntity(request);

        Dictionary<string, int> productDic = [];

        foreach (string productId in request.Products)
        {
            if (productDic.ContainsKey(productId))
                productDic[productId]++;
            else
                productDic.Add(productId, 1);
        }

        decimal orderTotalPrice = 0;
        foreach (var key in productDic.Keys)
        {
            //send a request for the product that matches "key" to ProductProvider to get price
            decimal price = 0;

            order.Products.Add(new OrderProductEntity { OrderId = order.Id, ProductId = Guid.Parse(key), Count = productDic[key], UnitPrice = price });
            orderTotalPrice += price * productDic[key];
        }

        if(orderTotalPrice == request.TotalPrice) 
        {
            //create order in DB, return bool/orderResponse
        }
        else
        {
            //return something
        }
        return new();
    }
    #endregion

    #region READ

    public List<OrderResponse> GetMyOrders(Guid userId)
    {
        //get all orders from DB with userId, create a response for each.
        //including all productIds but no product details.
        //The productIds will be returned and the blazor app can send them to ProductProvider to get the details
        return [];
    }
    #endregion

    #region UPDATE
    //only possible is order status is not Sent

    //pick up order from DB
    //do changes
    //change LastUpdated
    //Add to orderHistory
    #endregion

    #region DELETE
    //only possible if order status is not Sent
    #endregion
}
