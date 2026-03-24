using AspireApp.BedRock.SonetOps.ApiService.Models.Transactions;
using AspireApp.BedRock.SonetOps.ApiService.Repositories;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.ApiService.Services
{
    public class TransactionProcessingService : ITransactionProcessingService
    {
        private readonly ITransactionRepository _repository;
        private readonly IDistributedLock _distributedLock;
        private readonly ILogger<TransactionProcessingService> _logger;
        private const int MaxRetries = 3;

        public TransactionProcessingService(
            ITransactionRepository repository,
            IDistributedLock distributedLock,
            ILogger<TransactionProcessingService> logger)
        {
            _repository = repository;
            _distributedLock = distributedLock;
            _logger = logger;
        }

        public async Task<Transaction> ProcessTransactionAsync(
            string idempotencyKey,
            TransactionType type,
            decimal amount,
            string sourceAccount,
            string destinationAccount,
            Dictionary<string, string> metadata = null)
        {
            using var lockResult = await _distributedLock.AcquireLockAsync(
                $"transaction:{idempotencyKey}",
                TimeSpan.FromSeconds(30));

            if (!lockResult.IsAcquired)
            {
                throw new ConcurrencyException("Failed to acquire lock for transaction processing");
            }

            var existingTransaction = await _repository.GetByIdempotencyKeyAsync(idempotencyKey);
            if (existingTransaction != null)
            {
                return existingTransaction;
            }

            var transaction = Transaction.Create(
                idempotencyKey,
                type,
                amount,
                sourceAccount,
                destinationAccount,
                metadata);

            await _repository.CreateAsync(transaction);
            await _repository.CreateLogAsync(
                TransactionLog.Create(transaction.TransactionId, TransactionStatus.Pending, "Transaction created"));

            try
            {
                transaction.MarkAsProcessing();
                await _repository.UpdateAsync(transaction);
                await _repository.CreateLogAsync(
                    TransactionLog.Create(transaction.TransactionId, TransactionStatus.Processing, "Processing started"));

                // TODO: Implement actual transaction processing logic here
                // This could include:
                // 1. Validating accounts
                // 2. Checking balances
                // 3. Processing the transfer
                // 4. Updating external systems

                transaction.MarkAsCompleted();
                await _repository.UpdateAsync(transaction);
                await _repository.CreateLogAsync(
                    TransactionLog.Create(transaction.TransactionId, TransactionStatus.Completed, "Transaction completed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction {TransactionId}", transaction.TransactionId);
                transaction.MarkAsFailed(ex.Message);
                await _repository.UpdateAsync(transaction);
                await _repository.CreateLogAsync(
                    TransactionLog.Create(transaction.TransactionId, TransactionStatus.Failed, $"Transaction failed: {ex.Message}"));

                if (transaction.RetryCount >= MaxRetries)
                {
                    await InitiateCompensationAsync(transaction.TransactionId, "Max retries exceeded");
                }
            }

            return transaction;
        }

        public async Task RetryFailedTransactionAsync(Guid transactionId)
        {
            var transaction = await _repository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction {transactionId} not found");
            }

            if (!transaction.CanRetry(MaxRetries))
            {
                throw new InvalidOperationException("Transaction cannot be retried");
            }

            using var lockResult = await _distributedLock.AcquireLockAsync(
                $"transaction:{transaction.IdempotencyKey}",
                TimeSpan.FromSeconds(30));

            if (!lockResult.IsAcquired)
            {
                throw new ConcurrencyException("Failed to acquire lock for transaction retry");
            }

            transaction.IncrementRetry();
            await _repository.UpdateAsync(transaction);
            await _repository.CreateLogAsync(
                TransactionLog.Create(transaction.TransactionId, transaction.Status, $"Retry attempt {transaction.RetryCount}"));

            // Re-process the transaction
            await ProcessTransactionAsync(
                transaction.IdempotencyKey,
                transaction.Type,
                transaction.Amount,
                transaction.SourceAccount,
                transaction.DestinationAccount,
                transaction.Metadata);
        }

        public async Task InitiateCompensationAsync(Guid transactionId, string reason)
        {
            var transaction = await _repository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction {transactionId} not found");
            }

            using var lockResult = await _distributedLock.AcquireLockAsync(
                $"compensation:{transaction.TransactionId}",
                TimeSpan.FromSeconds(30));

            if (!lockResult.IsAcquired)
            {
                throw new ConcurrencyException("Failed to acquire lock for compensation");
            }

            transaction.InitiateCompensation();
            await _repository.UpdateAsync(transaction);
            await _repository.CreateLogAsync(
                TransactionLog.Create(transaction.TransactionId, transaction.Status, $"Compensation initiated: {reason}"));

            try
            {
                // TODO: Implement compensation logic
                // This could include:
                // 1. Reversing the transaction
                // 2. Refunding the amount
                // 3. Updating external systems
                // 4. Sending notifications

                transaction.CompleteCompensation();
                await _repository.UpdateAsync(transaction);
                await _repository.CreateLogAsync(
                    TransactionLog.Create(transaction.TransactionId, transaction.Status, "Compensation completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing compensation for transaction {TransactionId}", transaction.TransactionId);
                await _repository.CreateLogAsync(
                    TransactionLog.Create(transaction.TransactionId, transaction.Status, $"Compensation failed: {ex.Message}"));
                throw;
            }
        }

        public async Task<TransactionStatus> GetTransactionStatusAsync(Guid transactionId)
        {
            var transaction = await _repository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction {transactionId} not found");
            }

            return transaction.Status;
        }
    }
}