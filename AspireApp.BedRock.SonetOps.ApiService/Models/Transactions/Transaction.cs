using System;

namespace AspireApp.BedRock.SonetOps.ApiService.Models.Transactions
{
    public class Transaction
    {
        public Guid TransactionId { get; set; }
        public string IdempotencyKey { get; private set; }
        public TransactionStatus Status { get; private set; }
        public TransactionType Type { get; private set; }
        public decimal Amount { get; private set; }
        public string SourceAccount { get; private set; }
        public string DestinationAccount { get; private set; }
        public int RetryCount { get; private set; }
        public DateTime? LastRetryDate { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }
        public CompensationStatus CompensationStatus { get; private set; }
        public string ErrorMessage { get; private set; }
        public Dictionary<string, string> Metadata { get; private set; }

        private Transaction() { }

        public static Transaction Create(
            string idempotencyKey,
            TransactionType type,
            decimal amount,
            string sourceAccount,
            string destinationAccount,
            Dictionary<string, string> metadata = null)
        {
            return new Transaction
            {
                TransactionId = Guid.NewGuid(),
                IdempotencyKey = idempotencyKey,
                Status = TransactionStatus.Pending,
                Type = type,
                Amount = amount,
                SourceAccount = sourceAccount,
                DestinationAccount = destinationAccount,
                RetryCount = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CompensationStatus = CompensationStatus.NotRequired,
                Metadata = metadata ?? new Dictionary<string, string>()
            };
        }

        public void MarkAsProcessing()
        {
            Status = TransactionStatus.Processing;
            UpdatedDate = DateTime.UtcNow;
        }

        public void MarkAsCompleted()
        {
            Status = TransactionStatus.Completed;
            UpdatedDate = DateTime.UtcNow;
        }

        public void MarkAsFailed(string error)
        {
            Status = TransactionStatus.Failed;
            ErrorMessage = error;
            UpdatedDate = DateTime.UtcNow;
        }

        public void IncrementRetry()
        {
            RetryCount++;
            LastRetryDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }

        public void InitiateCompensation()
        {
            CompensationStatus = CompensationStatus.Initiated;
            Status = TransactionStatus.CompensationInitiated;
            UpdatedDate = DateTime.UtcNow;
        }

        public void CompleteCompensation()
        {
            CompensationStatus = CompensationStatus.Completed;
            Status = TransactionStatus.CompensationCompleted;
            UpdatedDate = DateTime.UtcNow;
        }

        public bool CanRetry(int maxRetries)
        {
            return Status == TransactionStatus.Failed && RetryCount < maxRetries;
        }
    }
}