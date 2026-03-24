using AspireApp.BedRock.SonetOps.ApiService.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.ApiService.Services
{
    public class TransactionProcessingBackgroundService : BackgroundService
    {
        private readonly ITransactionRepository _repository;
        private readonly ITransactionProcessingService _processingService;
        private readonly ILogger<TransactionProcessingBackgroundService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _errorInterval = TimeSpan.FromMinutes(5);

        public TransactionProcessingBackgroundService(
            ITransactionRepository repository,
            ITransactionProcessingService processingService,
            ILogger<TransactionProcessingBackgroundService> logger)
        {
            _repository = repository;
            _processingService = processingService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingTransactionsAsync(stoppingToken);
                    await RetryFailedTransactionsAsync(stoppingToken);
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in transaction processing background service");
                    await Task.Delay(_errorInterval, stoppingToken);
                }
            }
        }

        private async Task ProcessPendingTransactionsAsync(CancellationToken stoppingToken)
        {
            var pendingTransactions = await _repository.GetPendingTransactionsAsync();
            
            foreach (var transaction in pendingTransactions)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation("Processing pending transaction {TransactionId}", transaction.TransactionId);
                    
                    await _processingService.ProcessTransactionAsync(
                        transaction.IdempotencyKey,
                        transaction.Type,
                        transaction.Amount,
                        transaction.SourceAccount,
                        transaction.DestinationAccount,
                        transaction.Metadata);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing transaction {TransactionId}", transaction.TransactionId);
                }
            }
        }

        private async Task RetryFailedTransactionsAsync(CancellationToken stoppingToken)
        {
            var failedTransactions = await _repository.GetFailedTransactionsAsync();
            
            foreach (var transaction in failedTransactions)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (!transaction.CanRetry(3))  // Max retries hardcoded to 3
                    continue;

                try
                {
                    _logger.LogInformation("Retrying failed transaction {TransactionId}", transaction.TransactionId);
                    await _processingService.RetryFailedTransactionAsync(transaction.TransactionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrying transaction {TransactionId}", transaction.TransactionId);
                    
                    // If max retries reached, initiate compensation
                    if (transaction.RetryCount >= 3)
                    {
                        try
                        {
                            await _processingService.InitiateCompensationAsync(
                                transaction.TransactionId,
                                "Max retries exceeded");
                        }
                        catch (Exception compensationEx)
                        {
                            _logger.LogError(compensationEx, 
                                "Error initiating compensation for transaction {TransactionId}", 
                                transaction.TransactionId);
                        }
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Transaction processing background service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}