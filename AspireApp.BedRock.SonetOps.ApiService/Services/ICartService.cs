using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Models;

namespace AspireApp.BedRock.SonetOps.ApiService.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddItemToCartAsync(string userId, CartItemDto item);
    Task<CartDto> UpdateCartItemAsync(string userId, int itemId, CartItemDto item);
    Task<CartDto> RemoveCartItemAsync(string userId, int itemId);
    Task ClearCartAsync(string userId);
}