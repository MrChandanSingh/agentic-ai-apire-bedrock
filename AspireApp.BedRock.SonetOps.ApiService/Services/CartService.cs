using System;
using System.Linq;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Models;
using AspireApp.BedRock.SonetOps.DapperORM;
using AspireApp.BedRock.SonetOps.DapperORM.Repository;

namespace AspireApp.BedRock.SonetOps.ApiService.Services;

public class CartService : ICartService
{
    private readonly IDapperRepository _repository;
    private readonly IFishService _fishService;

    public CartService(IDapperRepository repository, IFishService fishService)
    {
        _repository = repository;
        _fishService = fishService;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await LoadCartWithItemsAsync(cart);
    }

    public async Task<CartDto> AddItemToCartAsync(string userId, CartItemDto itemDto)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var fish = await _fishService.GetFishByIdAsync(itemDto.FishId);
        
        if (fish == null)
        {
            throw new ArgumentException("Fish not found");
        }

        var existingItem = await _repository.QueryFirstOrDefaultAsync<CartItem>(
            "SELECT * FROM CartItems WHERE CartId = @CartId AND FishId = @FishId",
            new { CartId = cart.Id, FishId = itemDto.FishId });

        if (existingItem != null)
        {
            await _repository.ExecuteAsync(
                "UPDATE CartItems SET Quantity = @Quantity WHERE Id = @Id",
                new { Id = existingItem.Id, Quantity = existingItem.Quantity + itemDto.Quantity });
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                FishId = fish.Id,
                Name = fish.Name,
                Price = fish.Price,
                Quantity = itemDto.Quantity
            };

            await _repository.ExecuteAsync(
                "INSERT INTO CartItems (CartId, FishId, Name, Price, Quantity) VALUES (@CartId, @FishId, @Name, @Price, @Quantity)",
                cartItem);
        }

        await UpdateCartTimestamp(cart.Id);
        return await LoadCartWithItemsAsync(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(string userId, int itemId, CartItemDto itemDto)
    {
        var cart = await GetOrCreateCartAsync(userId);
        
        var cartItem = await _repository.QueryFirstOrDefaultAsync<CartItem>(
            "SELECT * FROM CartItems WHERE Id = @Id AND CartId = @CartId",
            new { Id = itemId, CartId = cart.Id });

        if (cartItem == null)
        {
            return null;
        }

        await _repository.ExecuteAsync(
            "UPDATE CartItems SET Quantity = @Quantity WHERE Id = @Id",
            new { Id = itemId, Quantity = itemDto.Quantity });

        await UpdateCartTimestamp(cart.Id);
        return await LoadCartWithItemsAsync(cart);
    }

    public async Task<CartDto> RemoveCartItemAsync(string userId, int itemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        
        await _repository.ExecuteAsync(
            "DELETE FROM CartItems WHERE Id = @Id AND CartId = @CartId",
            new { Id = itemId, CartId = cart.Id });

        await UpdateCartTimestamp(cart.Id);
        return await LoadCartWithItemsAsync(cart);
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        
        await _repository.ExecuteAsync(
            "DELETE FROM CartItems WHERE CartId = @CartId",
            new { CartId = cart.Id });

        await UpdateCartTimestamp(cart.Id);
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _repository.QueryFirstOrDefaultAsync<Cart>(
            "SELECT * FROM Carts WHERE UserId = @UserId",
            new { UserId = userId });

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var cartId = await _repository.ExecuteScalarAsync<int>(
                "INSERT INTO Carts (UserId, CreatedAt, UpdatedAt) VALUES (@UserId, @CreatedAt, @UpdatedAt); SELECT SCOPE_IDENTITY()",
                cart);

            cart.Id = cartId;
        }

        return cart;
    }

    private async Task<CartDto> LoadCartWithItemsAsync(Cart cart)
    {
        var items = await _repository.QueryAsync<CartItem>(
            "SELECT * FROM CartItems WHERE CartId = @CartId",
            new { CartId = cart.Id });

        foreach (var item in items)
        {
            item.Fish = await _fishService.GetFishByIdAsync(item.FishId);
        }

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items.Select(item => new CartItemDto
            {
                Id = item.Id,
                FishId = item.FishId,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                Fish = item.Fish
            }).ToList(),
            TotalAmount = items.Sum(item => item.Price * item.Quantity),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }

    private async Task UpdateCartTimestamp(int cartId)
    {
        await _repository.ExecuteAsync(
            "UPDATE Carts SET UpdatedAt = @UpdatedAt WHERE Id = @Id",
            new { Id = cartId, UpdatedAt = DateTime.UtcNow });
    }
}