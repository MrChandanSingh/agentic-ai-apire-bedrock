using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Models;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FishController : ControllerBase
{
    private readonly IFishService _fishService;

    public FishController(IFishService fishService)
    {
        _fishService = fishService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FishDto>>> GetFishList()
    {
        var fish = await _fishService.GetAllFishAsync();
        return Ok(fish);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FishDto>> GetFish(int id)
    {
        var fish = await _fishService.GetFishByIdAsync(id);
        if (fish == null)
        {
            return NotFound();
        }
        return Ok(fish);
    }

    [HttpGet("species/{species}")]
    public async Task<ActionResult<IEnumerable<FishDto>>> GetFishBySpecies(string species)
    {
        var fish = await _fishService.GetFishBySpeciesAsync(species);
        return Ok(fish);
    }
}