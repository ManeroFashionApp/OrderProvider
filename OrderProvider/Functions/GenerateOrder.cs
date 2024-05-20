using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProvider.Models;
using OrderProvider.Services;
using System.Net;

namespace OrderProvider.Functions
{
    public class GenerateOrder
    {
        private readonly ILogger<GenerateOrder> _logger;
        private readonly OrderService _orderService;

        public GenerateOrder(ILogger<GenerateOrder> logger, OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

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
