using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProvider.Models;
using OrderProvider.Services;
using System.Net;
using Azure.Messaging.ServiceBus;
using System.Text;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace OrderProvider.Functions
{
    public class GenerateOrder(ILogger<GenerateOrder> logger, OrderService orderService)
    {
        private readonly ILogger<GenerateOrder> _logger = logger;
        private readonly OrderService _orderService = orderService;

        [Function("GenerateOrder")]
        
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                OrderRequest? data = JsonConvert.DeserializeObject<OrderRequest>(body);

                if(data != null)
                {
                    var result = await _orderService.CreateOrder(data);
                    response.StatusCode = result.StatusCode;
                    response.Headers.Add("Content-Type", "application/json");
                    await response.WriteStringAsync(JsonConvert.SerializeObject(result.Data));

                    if (response.StatusCode == HttpStatusCode.OK && result.Data != null)
                    {
                        string messageBody = JsonConvert.SerializeObject(_orderService.CreateEmailRequest(result.Data, data.UserEmailAddress));
                        var message = _orderService.CreateServiceBusMessageAsync(messageBody);
                        var emailResult = await _orderService.SendServiceBusMessageAsync(message);
                    }
                }
                else
                {
                    response.StatusCode=HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }
    }
}
