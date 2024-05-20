using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProvider.Services;
using System.Net;

namespace OrderProvider.Functions
{
    public class GetOrders(ILogger<GetOrders> logger, OrderService orderService)
    {
        private readonly ILogger<GetOrders> _logger = logger;
        private readonly OrderService _orderService = orderService;

        [Function("GetOrders")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            try
            {
                string? userId = req.Headers.GetValues("userId").FirstOrDefault();

                if (Guid.TryParse(userId, out Guid userIdGuid))
                {
                    var result = await _orderService.GetMyOrders(userIdGuid);
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
