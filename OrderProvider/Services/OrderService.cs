using Data.Contexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderProvider.Factories;
using OrderProvider.Models;
using System.Text;

namespace OrderProvider.Services;

public class OrderService(OrderDBContext dbContext, HttpClient httpClient)
{
    private readonly OrderDBContext _dbContext = dbContext;
    private readonly HttpClient _httpClient = httpClient;
    private readonly string? _productProviderUrl = Environment.GetEnvironmentVariable("PRODUCT_PROVIDER_URL");

    #region CREATE
    public async Task<bool> CreateOrder(OrderRequest request)
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

        List<string> productIds = [.. productDic.Keys];


        //send that list to ProductProvider and get back list of products containing Id, name and unitprice
        //get back something like that :

        var response = await _httpClient.PostAsync(_productProviderUrl, new StringContent(JsonConvert.SerializeObject(productIds), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var productDetails = JsonConvert.DeserializeObject<List<OrderProductRequest>>(responseBody) ?? [];
            List<OrderProductEntity> products = OrderFactory.CreateOrderProductEntities(productDetails);

            //List<OrderProductEntity> products = [
            //new() { ProductId = Guid.Parse("cbe3a2f5-0c3e-4f1b-a4c2-4c9c572c3082"), Name = "t-shirt", UnitPrice = 123 },
            //new() { ProductId = Guid.Parse("cbe3a2f5-0c3e-4f1b-a4c2-4c9c572c3082"), Name = "t-shirt", UnitPrice = 123 },
            //new() { ProductId = Guid.Parse("cbe3a2f5-0c3e-4f1b-a4c2-4c9c572c3082"), Name = "t-shirt", UnitPrice = 123 }
            //];

            foreach (var key in productDic.Keys)
            {
                OrderProductEntity? product = products.FirstOrDefault(x => x.ProductId.ToString() == key);
                if (product != null)
                {
                    product.Count = productDic[key];
                    order.Products.Add(product);
                    orderTotalPrice += product.UnitPrice * product.Count;
                }
            }

            if (orderTotalPrice == request.TotalPrice)
            {
                await _dbContext.Orders.AddAsync(order);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                //return something
            }
            return new();
        }
        return new();

        
    }
    #endregion

    #region READ

    public List<OrderResponse> GetMyOrders(Guid userId)
    {
        try
        {
            List<OrderEntity> orders = _dbContext.Orders.Include(o => o.Products).Where(o => o.UserId == userId).ToList();
            List<OrderResponse> orderResponses = OrderFactory.GetOrders(orders);
            return orderResponses;
        }
        catch
        {
            return [];
        }
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
