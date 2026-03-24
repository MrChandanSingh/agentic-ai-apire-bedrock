# Transaction Processing System Design

## Message Broker Comparison

### Apache Kafka
#### Pros:
- High throughput and low latency
- Excellent for event streaming
- Better for high-volume transaction processing
- Built-in partitioning for scalability
- Message persistence and replay capability
- Strong ordering guarantees per partition

#### Cons:
- More complex to set up and manage
- Requires ZooKeeper (until Kafka 3.0)
- Higher operational overhead
- More resource intensive

### Azure Service Bus
#### Pros:
- Managed service (less operational overhead)
- Built-in dead-letter queues
- Native Azure integration
- AMQP protocol support
- Built-in retry policies
- Message sessions for ordering
- Simpler to set up and use

#### Cons:
- Higher latency than Kafka
- Limited throughput compared to Kafka
- Azure-specific (vendor lock-in)

## Recommended Choice: Azure Service Bus
For this system, Azure Service Bus is recommended because:
1. Native Azure integration with our existing infrastructure
2. Built-in dead-letter queue for failed transactions
3. Message sessions for maintaining transaction order
4. Simpler management and monitoring
5. Cost-effective for our transaction volume

## System Architecture

### Components

1. **Transaction Processor Service**
   - Processes pending transactions
   - Implements retry logic
   - Handles compensation/rollback
   - Maintains idempotency

2. **Transaction State Manager**
   - Tracks transaction states
   - Manages idempotency keys
   - Handles distributed locking

3. **Compensation Service**
   - Handles failed transaction rollbacks
   - Manages refunds/reversals
   - Ensures data consistency

4. **Monitoring Service**
   - Tracks transaction metrics
   - Alerts on failures
   - Provides audit trail

### Data Model

```sql
CREATE TABLE Transactions (
    TransactionId UNIQUEIDENTIFIER PRIMARY KEY,
    IdempotencyKey VARCHAR(100) UNIQUE,
    Status VARCHAR(50),
    Type VARCHAR(50),
    Amount DECIMAL(18,2),
    SourceAccount VARCHAR(100),
    DestinationAccount VARCHAR(100),
    RetryCount INT,
    LastRetryDate DATETIME,
    CreatedDate DATETIME,
    UpdatedDate DATETIME,
    CompensationStatus VARCHAR(50),
    ErrorMessage VARCHAR(MAX),
    Metadata VARCHAR(MAX)
);

CREATE TABLE TransactionLogs (
    LogId UNIQUEIDENTIFIER PRIMARY KEY,
    TransactionId UNIQUEIDENTIFIER,
    Status VARCHAR(50),
    Message VARCHAR(MAX),
    CreatedDate DATETIME,
    FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
);

CREATE TABLE IdempotencyKeys (
    KeyId UNIQUEIDENTIFIER PRIMARY KEY,
    IdempotencyKey VARCHAR(100) UNIQUE,
    TransactionId UNIQUEIDENTIFIER,
    ExpiryDate DATETIME,
    CreatedDate DATETIME,
    FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
);
```

### Message Flow

1. **Transaction Initiation**
```json
{
    "transactionId": "guid",
    "idempotencyKey": "unique-key",
    "type": "PAYMENT",
    "amount": 100.00,
    "sourceAccount": "account1",
    "destinationAccount": "account2",
    "metadata": {}
}
```

2. **Transaction Status Updates**
```json
{
    "transactionId": "guid",
    "status": "PROCESSING|COMPLETED|FAILED",
    "retryCount": 0,
    "error": "error-message",
    "timestamp": "datetime"
}
```

3. **Compensation Events**
```json
{
    "transactionId": "guid",
    "type": "REFUND",
    "amount": 100.00,
    "sourceAccount": "account2",
    "destinationAccount": "account1",
    "originalTransactionId": "guid",
    "reason": "FAILED_PROCESSING"
}
```

### Idempotency Implementation

1. **Request Phase**
   ```csharp
   public async Task<TransactionResult> ProcessTransaction(TransactionRequest request)
   {
       var idempotencyKey = request.IdempotencyKey;
       using var lockResult = await _distributedLock.AcquireLock(idempotencyKey);
       
       if (!lockResult.IsAcquired)
           throw new ConcurrentProcessingException();

       var existingTransaction = await _transactionRepository.GetByIdempotencyKey(idempotencyKey);
       if (existingTransaction != null)
           return TransactionResult.FromExisting(existingTransaction);

       var transaction = await CreateNewTransaction(request);
       await _serviceBus.PublishAsync(new ProcessTransactionMessage(transaction));
       
       return TransactionResult.Created(transaction);
   }
   ```

2. **Processing Phase**
   ```csharp
   public async Task HandleTransactionMessage(ProcessTransactionMessage message)
   {
       var transaction = await _transactionRepository.GetById(message.TransactionId);
       
       if (transaction.Status != TransactionStatus.Pending)
           return; // Already processed

       try
       {
           await ProcessTransactionLogic(transaction);
           transaction.Status = TransactionStatus.Completed;
       }
       catch (Exception ex)
       {
           transaction.RetryCount++;
           if (transaction.RetryCount >= MaxRetries)
           {
               await InitiateCompensation(transaction);
           }
           else
           {
               await _serviceBus.ScheduleMessageAsync(message, RetryDelay);
           }
       }
       
       await _transactionRepository.UpdateAsync(transaction);
   }
   ```

### Retry Strategy

1. **Progressive Delays**
```csharp
private TimeSpan GetRetryDelay(int retryCount)
{
    return TimeSpan.FromMinutes(Math.Pow(2, retryCount)); // Exponential backoff
}
```

2. **Maximum Retries**
```csharp
private const int MaxRetries = 3;
private static readonly TimeSpan MaxRetryPeriod = TimeSpan.FromHours(24);
```

### Compensation Logic

```csharp
public async Task InitiateCompensation(Transaction transaction)
{
    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
    
    try
    {
        // Create compensation record
        var compensation = new CompensationTransaction
        {
            OriginalTransactionId = transaction.TransactionId,
            Amount = transaction.Amount,
            SourceAccount = transaction.DestinationAccount, // Reversed
            DestinationAccount = transaction.SourceAccount  // Reversed
        };

        // Update original transaction status
        transaction.Status = TransactionStatus.CompensationInitiated;
        await _transactionRepository.UpdateAsync(transaction);

        // Publish compensation message
        await _serviceBus.PublishAsync(new ProcessCompensationMessage(compensation));

        scope.Complete();
    }
    catch (Exception ex)
    {
        // Log critical error - manual intervention needed
        await _monitoringService.AlertCriticalFailure(transaction, ex);
    }
}
```

### Monitoring and Alerting

1. **Transaction Metrics**
```csharp
public interface ITransactionMetrics
{
    Task TrackProcessingTime(Guid transactionId, TimeSpan duration);
    Task IncrementRetryCount(Guid transactionId);
    Task TrackFailedTransaction(Guid transactionId, string reason);
    Task TrackCompensation(Guid transactionId, CompensationReason reason);
}
```

2. **Health Checks**
```csharp
public class TransactionProcessingHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        var stuckTransactions = await GetStuckTransactions();
        var failedCompensations = await GetFailedCompensations();
        
        return new HealthCheckResult(
            status: GetHealthStatus(stuckTransactions, failedCompensations),
            description: "Transaction Processing Health Check"
        );
    }
}
```

## Implementation Steps

1. **Infrastructure Setup**
   - Set up Azure Service Bus namespace
   - Create required queues and topics
   - Configure dead-letter queue policies

2. **Database Setup**
   - Create transaction tables
   - Set up indexes for performance
   - Implement database versioning

3. **Service Implementation**
   - Transaction processing service
   - Compensation service
   - Monitoring service
   - Health check service

4. **Testing**
   - Unit tests for core logic
   - Integration tests with Service Bus
   - Load testing for concurrent processing
   - Chaos testing for failure scenarios