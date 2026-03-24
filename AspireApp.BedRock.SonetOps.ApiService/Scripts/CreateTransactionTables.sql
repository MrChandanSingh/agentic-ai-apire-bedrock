CREATE TABLE Transactions (
    TransactionId UNIQUEIDENTIFIER PRIMARY KEY,
    IdempotencyKey NVARCHAR(100) UNIQUE,
    Status NVARCHAR(50) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    SourceAccount NVARCHAR(100) NOT NULL,
    DestinationAccount NVARCHAR(100) NOT NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    LastRetryDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL,
    UpdatedDate DATETIME2 NOT NULL,
    CompensationStatus NVARCHAR(50) NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    Metadata NVARCHAR(MAX) NULL
);

CREATE INDEX IX_Transactions_Status ON Transactions(Status);
CREATE INDEX IX_Transactions_CreatedDate ON Transactions(CreatedDate);
CREATE INDEX IX_Transactions_UpdatedDate ON Transactions(UpdatedDate);

CREATE TABLE TransactionLogs (
    LogId UNIQUEIDENTIFIER PRIMARY KEY,
    TransactionId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL,
    FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
);

CREATE INDEX IX_TransactionLogs_TransactionId ON TransactionLogs(TransactionId);