using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProvider.Models;
using OrderProvider.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace OrderProvider.Functions
{
    public class GetOrders
    {
        private readonly ILogger<GetOrders> _logger;
        private readonly OrderService _orderService;

        public GetOrders(ILogger<GetOrders> logger, OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [Function("GetOrders")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var response = req.CreateResponse();
                string? userId = req.Headers.GetValues("userId").FirstOrDefault();

                if (userId != null)
                {
                    List<OrderResponse> data = _orderService.GetMyOrders(Guid.Parse(userId));
                    response.StatusCode = HttpStatusCode.OK;
                    response.Headers.Add("Content-Type", "application/json");
                    await response.WriteStringAsync(JsonConvert.SerializeObject(data));
                    return response;
                }
                else
                {
                    response.StatusCode=HttpStatusCode.BadRequest;
                    return response;
                }
            }
            catch
            {
                //to modify
                return HttpResponseData.CreateResponse(req);
            }
        }
    }
}
