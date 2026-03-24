using AspireApp.BedRock.SonetOps.ApiService.Models.Transactions;

namespace AspireApp.BedRock.SonetOps.ApiService.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(Guid transactionId);
        Task<Transaction> GetByIdempotencyKeyAsync(string idempotencyKey);
        Task<IEnumerable<Transaction>> GetPendingTransactionsAsync();
        Task<IEnumerable<Transaction>> GetFailedTransactionsAsync();
        Task CreateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task CreateLogAsync(TransactionLog log);
        Task<IEnumerable<TransactionLog>> GetTransactionLogsAsync(Guid transactionId);
    }
}