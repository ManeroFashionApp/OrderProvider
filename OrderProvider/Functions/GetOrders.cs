using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderProvider.Services;
using System.IdentityModel.Tokens.Jwt;
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
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            try
            {
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
                        _orderService.GetMyOrders(Guid.Parse(userId));
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
