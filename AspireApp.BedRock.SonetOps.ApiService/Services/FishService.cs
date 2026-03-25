using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Models;
using AspireApp.BedRock.SonetOps.DapperORM;
using AspireApp.BedRock.SonetOps.DapperORM.Repository;

namespace AspireApp.BedRock.SonetOps.ApiService.Services;

public class FishService : IFishService
{
    private readonly IDapperRepository _repository;

    public FishService(IDapperRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FishDto>> GetAllFishAsync()
    {
        var fish = await _repository.QueryAsync<Fish>("SELECT * FROM Fish WHERE InStock = 1");
        return fish.Select(f => MapToDto(f));
    }

    public async Task<FishDto> GetFishByIdAsync(int id)
    {
        var fish = await _repository.QueryFirstOrDefaultAsync<Fish>("SELECT * FROM Fish WHERE Id = @Id", new { Id = id });
        return fish != null ? MapToDto(fish) : null;
    }

    public async Task<IEnumerable<FishDto>> GetFishBySpeciesAsync(string species)
    {
        var fish = await _repository.QueryAsync<Fish>("SELECT * FROM Fish WHERE Species = @Species AND InStock = 1", 
            new { Species = species });
        return fish.Select(f => MapToDto(f));
    }

    private FishDto MapToDto(Fish fish)
    {
        return new FishDto
        {
            Id = fish.Id,
            Name = fish.Name,
            Description = fish.Description,
            Price = fish.Price,
            ImageUrl = fish.ImageUrl,
            Species = fish.Species,
            Weight = fish.Weight,
            InStock = fish.InStock
        };
    }
}