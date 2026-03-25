using System.Collections.Generic;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Controllers;
using AspireApp.BedRock.SonetOps.ApiService.Models;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AspireApp.BedRock.SonetOps.Tests.ApiTests;

public class FishControllerTests
{
    private readonly Mock<IFishService> _mockFishService;
    private readonly FishController _controller;

    public FishControllerTests()
    {
        _mockFishService = new Mock<IFishService>();
        _controller = new FishController(_mockFishService.Object);
    }

    [Fact]
    public async Task GetFishList_ReturnsAllFish()
    {
        // Arrange
        var expectedFish = new List<FishDto>
        {
            new FishDto { Id = 1, Name = "Salmon", Price = 15.99m },
            new FishDto { Id = 2, Name = "Tuna", Price = 12.99m }
        };

        _mockFishService.Setup(s => s.GetAllFishAsync())
            .ReturnsAsync(expectedFish);

        // Act
        var result = await _controller.GetFishList();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedFish = Assert.IsAssignableFrom<IEnumerable<FishDto>>(okResult.Value);
        Assert.Equal(expectedFish.Count, ((List<FishDto>)returnedFish).Count);
    }

    [Fact]
    public async Task GetFish_WithValidId_ReturnsFish()
    {
        // Arrange
        int fishId = 1;
        var expectedFish = new FishDto { Id = fishId, Name = "Salmon", Price = 15.99m };

        _mockFishService.Setup(s => s.GetFishByIdAsync(fishId))
            .ReturnsAsync(expectedFish);

        // Act
        var result = await _controller.GetFish(fishId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedFish = Assert.IsType<FishDto>(okResult.Value);
        Assert.Equal(expectedFish.Id, returnedFish.Id);
    }

    [Fact]
    public async Task GetFish_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        int fishId = 999;
        _mockFishService.Setup(s => s.GetFishByIdAsync(fishId))
            .ReturnsAsync((FishDto)null);

        // Act
        var result = await _controller.GetFish(fishId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetFishBySpecies_ReturnsFilteredFish()
    {
        // Arrange
        string species = "Salmon";
        var expectedFish = new List<FishDto>
        {
            new FishDto { Id = 1, Name = "Atlantic Salmon", Species = species },
            new FishDto { Id = 2, Name = "Pacific Salmon", Species = species }
        };

        _mockFishService.Setup(s => s.GetFishBySpeciesAsync(species))
            .ReturnsAsync(expectedFish);

        // Act
        var result = await _controller.GetFishBySpecies(species);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedFish = Assert.IsAssignableFrom<IEnumerable<FishDto>>(okResult.Value);
        Assert.Equal(expectedFish.Count, ((List<FishDto>)returnedFish).Count);
        Assert.All(returnedFish, fish => Assert.Equal(species, fish.Species));
    }
}