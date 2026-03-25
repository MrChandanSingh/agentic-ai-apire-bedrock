using System.Security.Claims;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Controllers;
using AspireApp.BedRock.SonetOps.ApiService.Models;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AspireApp.BedRock.SonetOps.Tests.ApiTests;

public class CartControllerTests
{
    private readonly Mock<ICartService> _mockCartService;
    private readonly CartController _controller;
    private readonly string _userId = "test-user";

    public CartControllerTests()
    {
        _mockCartService = new Mock<ICartService>();
        _controller = new CartController(_mockCartService.Object);

        // Setup controller context with test user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, _userId),
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetCart_ReturnsUserCart()
    {
        // Arrange
        var expectedCart = new CartDto
        {
            Id = 1,
            UserId = _userId,
            TotalAmount = 29.98m
        };

        _mockCartService.Setup(s => s.GetCartAsync(_userId))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.GetCart();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(expectedCart.Id, returnedCart.Id);
        Assert.Equal(expectedCart.UserId, returnedCart.UserId);
    }

    [Fact]
    public async Task AddItemToCart_ReturnsUpdatedCart()
    {
        // Arrange
        var itemToAdd = new CartItemDto
        {
            FishId = 1,
            Name = "Salmon",
            Quantity = 2,
            Price = 15.99m
        };

        var expectedCart = new CartDto
        {
            Id = 1,
            UserId = _userId,
            TotalAmount = 31.98m
        };

        _mockCartService.Setup(s => s.AddItemToCartAsync(_userId, itemToAdd))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.AddItemToCart(itemToAdd);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(expectedCart.Id, returnedCart.Id);
        Assert.Equal(expectedCart.TotalAmount, returnedCart.TotalAmount);
    }

    [Fact]
    public async Task UpdateCartItem_WithValidId_ReturnsUpdatedCart()
    {
        // Arrange
        int itemId = 1;
        var itemToUpdate = new CartItemDto
        {
            FishId = 1,
            Name = "Salmon",
            Quantity = 3,
            Price = 15.99m
        };

        var expectedCart = new CartDto
        {
            Id = 1,
            UserId = _userId,
            TotalAmount = 47.97m
        };

        _mockCartService.Setup(s => s.UpdateCartItemAsync(_userId, itemId, itemToUpdate))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.UpdateCartItem(itemId, itemToUpdate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(expectedCart.Id, returnedCart.Id);
        Assert.Equal(expectedCart.TotalAmount, returnedCart.TotalAmount);
    }

    [Fact]
    public async Task UpdateCartItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        int itemId = 999;
        var itemToUpdate = new CartItemDto
        {
            FishId = 1,
            Name = "Salmon",
            Quantity = 3,
            Price = 15.99m
        };

        _mockCartService.Setup(s => s.UpdateCartItemAsync(_userId, itemId, itemToUpdate))
            .ReturnsAsync((CartDto)null);

        // Act
        var result = await _controller.UpdateCartItem(itemId, itemToUpdate);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task RemoveCartItem_WithValidId_ReturnsUpdatedCart()
    {
        // Arrange
        int itemId = 1;
        var expectedCart = new CartDto
        {
            Id = 1,
            UserId = _userId,
            TotalAmount = 0m
        };

        _mockCartService.Setup(s => s.RemoveCartItemAsync(_userId, itemId))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.RemoveCartItem(itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(expectedCart.Id, returnedCart.Id);
        Assert.Equal(expectedCart.TotalAmount, returnedCart.TotalAmount);
    }

    [Fact]
    public async Task ClearCart_ReturnsOkResult()
    {
        // Arrange
        _mockCartService.Setup(s => s.ClearCartAsync(_userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ClearCart();

        // Assert
        Assert.IsType<OkResult>(result);
    }
}