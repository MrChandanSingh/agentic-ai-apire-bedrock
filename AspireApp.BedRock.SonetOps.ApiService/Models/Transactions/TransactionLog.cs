using System;

namespace AspireApp.BedRock.SonetOps.ApiService.Models.Transactions
{
    public class TransactionLog
    {
        public Guid LogId { get; private set; }
        public Guid TransactionId { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string Message { get; private set; }
        public DateTime CreatedDate { get; private set; }

        private TransactionLog() { }

        public static TransactionLog Create(Guid transactionId, TransactionStatus status, string message)
        {
            return new TransactionLog
            {
                LogId = Guid.NewGuid(),
                TransactionId = transactionId,
                Status = status,
                Message = message,
                CreatedDate = DateTime.UtcNow
            };
        }
    }
}