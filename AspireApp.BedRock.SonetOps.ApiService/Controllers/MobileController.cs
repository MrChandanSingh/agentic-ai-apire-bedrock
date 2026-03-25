using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using AspireApp.BedRock.SonetOps.ApiService.Services;

namespace AspireApp.BedRock.SonetOps.ApiService.Controllers;

[ApiController]
[Route("mobile/[controller]")]
public class MobileController : ControllerBase
{
    private readonly ILogger<MobileController> _logger;
    private readonly ITransactionProcessingService _transactionService;

    public MobileController(
        ILogger<MobileController> logger,
        ITransactionProcessingService transactionService)
    {
        _logger = logger;
        _transactionService = transactionService;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var transactions = await _transactionService.GetRecentTransactionsAsync(page, pageSize);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, "An error occurred while retrieving transactions");
        }
    }

    [HttpGet("transaction/{id}")]
    public async Task<IActionResult> GetTransaction(string id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the transaction");
        }
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var transaction = await _transactionService.CreateTransactionAsync(request);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, "An error occurred while creating the transaction");
        }
    }
}

public class TransactionRequest
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Currency { get; set; } = "USD";
}