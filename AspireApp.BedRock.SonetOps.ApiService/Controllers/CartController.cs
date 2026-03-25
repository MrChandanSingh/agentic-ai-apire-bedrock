using System;
using System.Threading.Tasks;
using AspireApp.BedRock.SonetOps.ApiService.Models;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = User.Identity.Name;
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItemToCart(CartItemDto item)
    {
        var userId = User.Identity.Name;
        var cart = await _cartService.AddItemToCartAsync(userId, item);
        return Ok(cart);
    }

    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(int itemId, CartItemDto item)
    {
        var userId = User.Identity.Name;
        var cart = await _cartService.UpdateCartItemAsync(userId, itemId, item);
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult<CartDto>> RemoveCartItem(int itemId)
    {
        var userId = User.Identity.Name;
        var cart = await _cartService.RemoveCartItemAsync(userId, itemId);
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    [HttpPost("clear")]
    public async Task<ActionResult> ClearCart()
    {
        var userId = User.Identity.Name;
        await _cartService.ClearCartAsync(userId);
        return Ok();
    }
}