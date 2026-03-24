using AspireApp.BedRock.SonetOps.ApiService.Models.Transactions;

namespace AspireApp.BedRock.SonetOps.ApiService.Services
{
    public interface ITransactionProcessingService
    {
        Task<Transaction> ProcessTransactionAsync(string idempotencyKey, TransactionType type, decimal amount, string sourceAccount, string destinationAccount, Dictionary<string, string> metadata = null);
        Task RetryFailedTransactionAsync(Guid transactionId);
        Task InitiateCompensationAsync(Guid transactionId, string reason);
        Task<TransactionStatus> GetTransactionStatusAsync(Guid transactionId);
    }
}