using AspireApp.BedRock.PaymentGateway.Models;

namespace AspireApp.BedRock.PaymentGateway.Services;

public interface IFishProductService
{
    Task<List<FishProduct>> GetFishProductsAsync();
    Task<FishProduct?> GetFishProductByIdAsync(int id);
}

public class FishProductService : IFishProductService
{
    private readonly List<FishProduct> _mockProducts = new()
    {
        new FishProduct
        {
            Id = 1,
            Name = "Atlantic Salmon",
            Description = "Fresh Atlantic salmon fillet, rich in omega-3",
            Price = 24.99m,
            ImageUrl = "/images/atlantic-salmon.jpg",
            InStock = true
        },
        new FishProduct
        {
            Id = 2,
            Name = "Tuna Steak",
            Description = "Premium yellowfin tuna steak, sushi-grade",
            Price = 29.99m,
            ImageUrl = "/images/tuna-steak.jpg",
            InStock = true
        },
        new FishProduct
        {
            Id = 3,
            Name = "Sea Bass",
            Description = "Wild-caught Mediterranean sea bass",
            Price = 19.99m,
            ImageUrl = "/images/sea-bass.jpg",
            InStock = true
        },
        new FishProduct
        {
            Id = 4,
            Name = "Tiger Prawns",
            Description = "Large tiger prawns, perfect for grilling",
            Price = 15.99m,
            ImageUrl = "/images/tiger-prawns.jpg",
            InStock = true
        },
        new FishProduct
        {
            Id = 5,
            Name = "Rainbow Trout",
            Description = "Farm-raised rainbow trout fillet",
            Price = 17.99m,
            ImageUrl = "/images/rainbow-trout.jpg",
            InStock = true
        }
    };

    public async Task<List<FishProduct>> GetFishProductsAsync()
    {
        // Simulate network delay
        await Task.Delay(300);
        return _mockProducts;
    }

    public async Task<FishProduct?> GetFishProductByIdAsync(int id)
    {
        await Task.Delay(100);
        return _mockProducts.FirstOrDefault(p => p.Id == id);
    }
}