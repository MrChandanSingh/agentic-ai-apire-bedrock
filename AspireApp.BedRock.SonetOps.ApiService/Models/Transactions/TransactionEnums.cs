namespace AspireApp.BedRock.SonetOps.ApiService.Models.Transactions
{
    public enum TransactionStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        CompensationInitiated,
        CompensationCompleted
    }

    public enum TransactionType
    {
        Payment,
        Refund,
        Transfer,
        Compensation
    }

    public enum CompensationStatus
    {
        NotRequired,
        Initiated,
        Completed,
        Failed
    }
}