using Azure.Messaging.ServiceBus;
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

            List<Guid> productIds = request.Products.Select(p => p.Id).ToList();
            if (productIds.Count > 0)
            {
                var newRequest = JsonConvert.SerializeObject(productIds);

                //FOR TESTING: replace _productProviderUrl with a random (but valid) url, for example "https://google.com/"
                var response = await _httpClient.PostAsync(_productProviderUrl, new StringContent(JsonConvert.SerializeObject(productIds), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    decimal orderTotalPrice = 0;

                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<OrderProductRequest> productDetails = JsonConvert.DeserializeObject<List<OrderProductRequest>>(responseBody) ?? [];
                    List<OrderProductEntity> products = OrderFactory.CreateOrderProductEntities(productDetails);

                    foreach (OrderProductEntity product in products)
                    {
                        OrderProductRequest? productRequest = request.Products.FirstOrDefault(x => x.Id == product.ProductId);
                        if (productRequest != null)
                        {
                            product.Count = productRequest.Count;
                            orderTotalPrice += product.UnitPrice * product.Count;
                        }
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
                else
                {
                    resultModel.StatusCode = response.StatusCode;
                }
            }
            else
            {
                resultModel.StatusCode = HttpStatusCode.BadRequest;
            }
            
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

    #region CREATE EMAIL REQUEST
    public EmailRequest CreateEmailRequest(OrderResponse newOrder, string userEmail)
    {
        try
        {
            EmailRequest emailRequest = new()
            {
                To = userEmail,
                Subject = $"Order #{newOrder.Id} confirmation",
                HtmlBody = $@"
                         <!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Order confirmation</title>

                        </head>
                        <body style='font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        margin: 0;
                        padding: 0;'>
                                <div style='width: 100%; max-width: 600px; margin: 20px auto; padding: 20px; background-color: #fff; border-radius: 5px;'>
                                <div style='text-align: center;'>
                                    <img src='https://example.com/logo.png' alt='Logo'>
                                </div>
                                <div style='text-align: center; font-size: 24px; margin: 30px 0;'>
                                    <p>Hello!<br>
                                    You have successfully placed an order. You can find it on your Manero app or at manero.com under My Orders.<br>
                                    <br>
                                    Order Id : #{newOrder.Id}.
                                    <P>
                                </div>
                                <div style='text-align: center; margin: 20px 0; color: #666;'>
                                    Kind Regards,<br>
                                    Manero Team
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                PlainText = $"Your order (Id {newOrder.Id}) has been created"
            };
            return emailRequest;
        }
        catch
        {
            return null!;
        }
    }
    #endregion

    #region CREATE SERVICEBUS MESSAGE
    public ServiceBusMessage CreateServiceBusMessageAsync(string payload)
    {
        return new(Encoding.UTF8.GetBytes(payload))
        {
            ContentType = "application/json",
        };
    }
    #endregion

    #region SEND SERVICEBUS MESSAGE
    public async Task<bool> SendServiceBusMessageAsync(ServiceBusMessage message)
    {
        bool result;
        try
        {
            var client = new ServiceBusClient(Environment.GetEnvironmentVariable("MAIL_SERVICEBUS"));
            var sender = client.CreateSender(Environment.GetEnvironmentVariable("QUEUENAME_MAILREQUEST"));
            await sender.SendMessageAsync(message);
            result = true;
            await client.DisposeAsync();
            await sender.DisposeAsync();
        }
        catch
        {
            result = false;
        }
        return result;
    }
    #endregion
}
