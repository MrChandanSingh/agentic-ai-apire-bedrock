using System;
using System.ComponentModel.DataAnnotations;

namespace AspireApp.BedRock.SonetOps.ApiService.Models;

public class Fish
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public string ImageUrl { get; set; }
    
    public string Species { get; set; }
    
    public decimal Weight { get; set; }
    
    public bool InStock { get; set; }
}

public class FishDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public string Species { get; set; }
    public decimal Weight { get; set; }
    public bool InStock { get; set; }
}