using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspireApp.BedRock.SonetOps.ApiService.Models;

public class Cart
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public decimal TotalAmount => Items?.Sum(item => item.Quantity * item.Price) ?? 0;
}

public class CartItem
{
    public int Id { get; set; }
    
    [Required]
    public int FishId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public Fish Fish { get; set; }
}

public class CartDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public List<CartItemDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int FishId { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public FishDto Fish { get; set; }
}