using AspireApp.BedRock.SonetOps.ApiService.Models.Transactions;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionProcessingService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionProcessingService transactionService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var transaction = await _transactionService.ProcessTransactionAsync(
                    request.IdempotencyKey,
                    request.Type,
                    request.Amount,
                    request.SourceAccount,
                    request.DestinationAccount,
                    request.Metadata);

                return Ok(new
                {
                    transactionId = transaction.TransactionId,
                    status = transaction.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction");
                return StatusCode(500, new { error = "Error processing transaction" });
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetTransactionStatus(Guid transactionId)
        {
            try
            {
                var status = await _transactionService.GetTransactionStatusAsync(transactionId);
                return Ok(new { status });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction status");
                return StatusCode(500, new { error = "Error getting transaction status" });
            }
        }

        [HttpPost("{transactionId}/retry")]
        public async Task<IActionResult> RetryTransaction(Guid transactionId)
        {
            try
            {
                await _transactionService.RetryFailedTransactionAsync(transactionId);
                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying transaction");
                return StatusCode(500, new { error = "Error retrying transaction" });
            }
        }
    }

    public class CreateTransactionRequest
    {
        public string IdempotencyKey { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccount { get; set; }
        public string DestinationAccount { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}