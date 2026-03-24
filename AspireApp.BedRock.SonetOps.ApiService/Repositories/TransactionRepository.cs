using AspireApp.BedRock.SonetOps.ApiService.Models.Transactions;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AspireApp.BedRock.SonetOps.ApiService.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

        public async Task<Transaction> GetByIdAsync(Guid transactionId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM Transactions 
                WHERE TransactionId = @TransactionId";

            return await connection.QueryFirstOrDefaultAsync<Transaction>(sql, new { TransactionId = transactionId });
        }

        public async Task<Transaction> GetByIdempotencyKeyAsync(string idempotencyKey)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM Transactions 
                WHERE IdempotencyKey = @IdempotencyKey";

            return await connection.QueryFirstOrDefaultAsync<Transaction>(sql, new { IdempotencyKey = idempotencyKey });
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM Transactions 
                WHERE Status = @Status 
                ORDER BY CreatedDate";

            return await connection.QueryAsync<Transaction>(sql, new { Status = TransactionStatus.Pending });
        }

        public async Task<IEnumerable<Transaction>> GetFailedTransactionsAsync()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM Transactions 
                WHERE Status = @Status 
                ORDER BY UpdatedDate";

            return await connection.QueryAsync<Transaction>(sql, new { Status = TransactionStatus.Failed });
        }

        public async Task CreateAsync(Transaction transaction)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO Transactions (
                    TransactionId, IdempotencyKey, Status, Type, Amount, 
                    SourceAccount, DestinationAccount, RetryCount, LastRetryDate,
                    CreatedDate, UpdatedDate, CompensationStatus, ErrorMessage, Metadata)
                VALUES (
                    @TransactionId, @IdempotencyKey, @Status, @Type, @Amount,
                    @SourceAccount, @DestinationAccount, @RetryCount, @LastRetryDate,
                    @CreatedDate, @UpdatedDate, @CompensationStatus, @ErrorMessage,
                    @Metadata)";

            await connection.ExecuteAsync(sql, new
            {
                transaction.TransactionId,
                transaction.IdempotencyKey,
                Status = transaction.Status.ToString(),
                Type = transaction.Type.ToString(),
                transaction.Amount,
                transaction.SourceAccount,
                transaction.DestinationAccount,
                transaction.RetryCount,
                transaction.LastRetryDate,
                transaction.CreatedDate,
                transaction.UpdatedDate,
                CompensationStatus = transaction.CompensationStatus.ToString(),
                transaction.ErrorMessage,
                Metadata = System.Text.Json.JsonSerializer.Serialize(transaction.Metadata)
            });
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE Transactions 
                SET Status = @Status,
                    RetryCount = @RetryCount,
                    LastRetryDate = @LastRetryDate,
                    UpdatedDate = @UpdatedDate,
                    CompensationStatus = @CompensationStatus,
                    ErrorMessage = @ErrorMessage,
                    Metadata = @Metadata
                WHERE TransactionId = @TransactionId";

            await connection.ExecuteAsync(sql, new
            {
                transaction.TransactionId,
                Status = transaction.Status.ToString(),
                transaction.RetryCount,
                transaction.LastRetryDate,
                transaction.UpdatedDate,
                CompensationStatus = transaction.CompensationStatus.ToString(),
                transaction.ErrorMessage,
                Metadata = System.Text.Json.JsonSerializer.Serialize(transaction.Metadata)
            });
        }

        public async Task CreateLogAsync(TransactionLog log)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO TransactionLogs (
                    LogId, TransactionId, Status, Message, CreatedDate)
                VALUES (
                    @LogId, @TransactionId, @Status, @Message, @CreatedDate)";

            await connection.ExecuteAsync(sql, new
            {
                log.LogId,
                log.TransactionId,
                Status = log.Status.ToString(),
                log.Message,
                log.CreatedDate
            });
        }

        public async Task<IEnumerable<TransactionLog>> GetTransactionLogsAsync(Guid transactionId)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM TransactionLogs 
                WHERE TransactionId = @TransactionId 
                ORDER BY CreatedDate";

            return await connection.QueryAsync<TransactionLog>(sql, new { TransactionId = transactionId });
        }
    }
}