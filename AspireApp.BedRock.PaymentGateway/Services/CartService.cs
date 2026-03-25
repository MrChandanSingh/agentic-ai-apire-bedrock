using AspireApp.BedRock.PaymentGateway.Models;

namespace AspireApp.BedRock.PaymentGateway.Services;

public class CartItem
{
    public FishProduct Product { get; set; } = null!;
    public int Quantity { get; set; }
}

public interface ICartService
{
    event Action? OnChange;
    Task AddToCartAsync(FishProduct product);
    Task<List<CartItem>> GetCartItemsAsync();
    Task UpdateQuantityAsync(int productId, int quantity);
    Task RemoveFromCartAsync(int productId);
    Task<int> GetCartCountAsync();
    Task<decimal> GetSubtotalAsync();
    Task<bool> ValidateItemQuantitiesAsync();
}

public class CartService : ICartService
{
    private readonly List<CartItem> _cartItems = new();
    public event Action? OnChange;

    public Task<List<CartItem>> GetCartItemsAsync()
    {
        return Task.FromResult(_cartItems.ToList());
    }

    public Task AddToCartAsync(FishProduct product)
    {
        var existingItem = _cartItems.FirstOrDefault(item => item.Product.Id == product.Id);
        
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            _cartItems.Add(new CartItem { Product = product, Quantity = 1 });
        }

        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public Task UpdateQuantityAsync(int productId, int quantity)
    {
        var item = _cartItems.FirstOrDefault(item => item.Product.Id == productId);
        
        if (item != null)
        {
            if (quantity <= 0)
            {
                _cartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            OnChange?.Invoke();
        }

        return Task.CompletedTask;
    }

    public Task RemoveFromCartAsync(int productId)
    {
        var item = _cartItems.FirstOrDefault(item => item.Product.Id == productId);
        
        if (item != null)
        {
            _cartItems.Remove(item);
            OnChange?.Invoke();
        }

        return Task.CompletedTask;
    }

    public Task<int> GetCartCountAsync()
    {
        return Task.FromResult(_cartItems.Sum(item => item.Quantity));
    }

    public Task<decimal> GetSubtotalAsync()
    {
        var subtotal = _cartItems.Sum(item => item.Product.Price * item.Quantity);
        return Task.FromResult(subtotal);
    }

    public Task<bool> ValidateItemQuantitiesAsync()
    {
        return Task.FromResult(_cartItems.All(item => item.Quantity > 0));
    }
}