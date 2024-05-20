using Data.Contexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderProvider.Factories;
using OrderProvider.Models;
using System.Net;
using System.Text;

namespace OrderProvider.Services;

public class OrderService(OrderDBContext dbContext, HttpClient httpClient)
{
    private readonly OrderDBContext _dbContext = dbContext;
    private readonly HttpClient _httpClient = httpClient;
    private readonly string? _productProviderUrl = Environment.GetEnvironmentVariable("PRODUCT_PROVIDER_URL");

    #region CREATE
    public async Task<ServiceResultModel<OrderResponse>> CreateOrder(OrderRequest request)
    {
        ServiceResultModel<OrderResponse> resultModel = new();
        
        try
        {
            OrderEntity order = OrderFactory.CreateOrderEntity(request);

            Dictionary<Guid, int> productDic = [];

            foreach (Guid productId in request.Products)
            {
                if (productDic.ContainsKey(productId))
                    productDic[productId]++;
                else
                    productDic.Add(productId, 1);
            }

            List<Guid> productIds = [.. productDic.Keys];
            var newRequest = JsonConvert.SerializeObject(productIds);
            var response = await _httpClient.PostAsync(_productProviderUrl, new StringContent(JsonConvert.SerializeObject(productIds), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                decimal orderTotalPrice = 0;

                string responseBody = await response.Content.ReadAsStringAsync();
                List<OrderProductRequest> productDetails = JsonConvert.DeserializeObject<List<OrderProductRequest>>(responseBody) ?? [];
                List<OrderProductEntity> products = OrderFactory.CreateOrderProductEntities(productDetails);

                foreach(OrderProductEntity product in products)
                {
                    var key = productDic.Keys.FirstOrDefault(x => x == product.ProductId);
                    product.Count = productDic[key];
                    orderTotalPrice += product.UnitPrice * product.Count;
                }
                order.Products.AddRange(products);

                if (orderTotalPrice == request.TotalPrice)
                {
                    await _dbContext.Orders.AddAsync(order);
                    await _dbContext.SaveChangesAsync();
                    OrderResponse orderResponse = OrderFactory.GetOrder(order);
                    resultModel.StatusCode = HttpStatusCode.OK;
                    resultModel.Data = orderResponse;
                }
                else
                {
                    resultModel.StatusCode = HttpStatusCode.BadRequest;
                }
            }
            resultModel.StatusCode = response.StatusCode;
        }
        catch
        {
            resultModel.StatusCode = HttpStatusCode.InternalServerError;
        }
        return resultModel;
    }
    #endregion

    #region READ

    public async Task<ServiceResultModel<List<OrderResponse>>> GetMyOrders(Guid userId)
    {
        ServiceResultModel<List<OrderResponse>> result = new();

        try
        {
            List<OrderEntity> orders = await _dbContext.Orders.Include(o => o.Products).Where(o => o.UserId == userId).ToListAsync();
            List<OrderResponse> orderResponses = OrderFactory.GetOrders(orders);
            result.StatusCode = HttpStatusCode.OK;
            result.Data = orderResponses;
        }
        catch
        {
            result.StatusCode = HttpStatusCode.InternalServerError;
        }
        return result;
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
