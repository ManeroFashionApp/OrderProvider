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
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var response = req.CreateResponse();

                OrderRequest? data = JsonConvert.DeserializeObject<OrderRequest>(body);

                if(data != null)
                {
                    bool isCreated = await _orderService.CreateOrder(data);
                    //response.StatusCode = HttpStatusCode.OK;
                    response.StatusCode = isCreated ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
                    response.Headers.Add("Content-Type", "application/json");
                    //await response.WriteStringAsync(JsonConvert.SerializeObject(isCreated));
                    
                    await response.WriteStringAsync(JsonConvert.SerializeObject(true));

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
