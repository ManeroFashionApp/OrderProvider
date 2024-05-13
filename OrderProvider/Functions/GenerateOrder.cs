using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProvider.Models;
using OrderProvider.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                OrderRequest data = JsonConvert.DeserializeObject<OrderRequest>(body) ?? new();

                if (req.Headers.TryGetValues("Authorization", out var authorizationHeaders))
                {
                    var token = authorizationHeaders.FirstOrDefault()?.Split(" ").Last();
                    //send token to validate to TokenProvider

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtTokenObject = tokenHandler.ReadJwtToken(token);

                    var claims = jwtTokenObject.Claims;
                    Claim? userIdClaim = claims.FirstOrDefault(c => c.Type == "userId");

                    if (userIdClaim != null)
                    {
                        var userId = userIdClaim.Value;
                        _orderService.CreateOrder(data, Guid.Parse(userId));
                    }
                    
                }
                //to modify
                return HttpResponseData.CreateResponse(req);
            }
            catch
            {
                //to modify
                return HttpResponseData.CreateResponse(req);
            }
        }
    }
}
