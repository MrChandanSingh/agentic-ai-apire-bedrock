using System.Collections.Generic;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Models;

namespace AspireApp.BedRock.SonetOps.ApiService.Services;

public interface IFishService
{
    Task<IEnumerable<FishDto>> GetAllFishAsync();
    Task<FishDto> GetFishByIdAsync(int id);
    Task<IEnumerable<FishDto>> GetFishBySpeciesAsync(string species);
}