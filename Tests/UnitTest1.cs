using Data.Contexts;
using Data.Entities;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using OrderProvider.Models;
using OrderProvider.Services;
using System.Net;
using System.Text;

public class OrderServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly DbContextOptions<OrderDBContext> _dbContextOptions;
    private readonly OrderDBContext _dbContext;

    public OrderServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _dbContextOptions = new DbContextOptionsBuilder<OrderDBContext>()
            .UseInMemoryDatabase(databaseName: "OrderTestDB")
            .Options;
        _dbContext = new OrderDBContext(_dbContextOptions);
    }

    [Fact]
    public async Task CreateOrder_Success()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            UserId = Guid.NewGuid(),
            UserEmailAddress = "alice.azmr@gmail.com",
            Products =
            [
                new() { Id = Guid.NewGuid(), ProductName = "t-shirt", Count = 2, Price = 20 },
                new() { Id = Guid.NewGuid(), ProductName = "dress", Count = 3, Price = 20 }
            ],
            TotalPrice = 100,
            FirstName = "Alice",
            LastName = "Az",
            Address = "Någotvägen 9",
            ZipCode = "12345",
            City = "Stockholm",
            Country = "Sweden"
        };

        var productDetails = new List<OrderProductRequest>
        {
            new() { Id = orderRequest.Products[0].Id, ProductName = "t-shirt", Price = 20 },
            new() { Id = orderRequest.Products[1].Id, ProductName = "dress", Price = 20 }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(productDetails), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        HttpClient client = new HttpClient(_mockHttpMessageHandler.Object);

        var orderService = new OrderService(_dbContext, client);

        // Act
        //note: replace _productProviderUrl (url from environment variables) with a random (but valid) url,
        //otherwise gets in the catch
        var result = await orderService.CreateOrder(orderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(orderRequest.TotalPrice, result.Data.Total);
        Assert.Equal(orderRequest.Products.Count, result.Data.Products.Count);
    }

    [Fact]
    public async Task CreateOrder_Fail_InvalidTotalPrice()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            Products =
            [
                 new() { Id = Guid.NewGuid(), ProductName = "t-shirt", Count = 2, Price = 20 },
                new() { Id = Guid.NewGuid(), ProductName = "dress", Count = 3, Price = 20 }
            ],
            TotalPrice = 50 // Set to a different total price to force a failure
        };

        var productDetails = new List<OrderProductRequest>
        {
            new() { Id = orderRequest.Products[0].Id, ProductName = "t-shirt", Price = 20 },
            new() { Id = orderRequest.Products[1].Id, ProductName = "dress", Price = 20 }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(productDetails), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        HttpClient client = new HttpClient(_mockHttpMessageHandler.Object);

        var orderService = new OrderService(_dbContext, client);

        // Act
        var result = await orderService.CreateOrder(orderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetMyOrders_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Random random = new();

        var orders = new List<OrderEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "Alice",
                LastName = "Az",
                Address = "Någotvägen 9",
                ZipCode = "12345",
                City = "Stockholm",
                Country = "Sweden",
                Status = OrderStatus.Registered,
                Created = DateTime.Now,
                Products =
                [
                    new() 
                    { 
                        Id = random.Next(100), 
                        ProductId = Guid.NewGuid(),
                        Name = "t-shirt",
                        UnitPrice = 10, 
                        Count = 1 }
                ]
            }
        };
        await _dbContext.Orders.AddRangeAsync(orders);
        await _dbContext.SaveChangesAsync();

        var orderService = new OrderService(_dbContext, new HttpClient());

        // Act
        var result = await orderService.GetMyOrders(userId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(orders[0].Products[0].Count, result.Data.Count);
        Assert.Equal(orders[0].Id, result.Data[0].Id);
    }

    [Fact]
    public async Task GetMyOrders_Fail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderService = new OrderService(_dbContext, new HttpClient());

        // Act
        var result = await orderService.GetMyOrders(userId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
}
