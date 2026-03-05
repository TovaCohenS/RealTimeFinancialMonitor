namespace RealTimeFinancialMonitor.Dtos;

public sealed record TransactionDto
{
    public required Guid TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = "Pending";
    public required DateTime Timestamp { get; init; }
}

public sealed record AddTransactionResponse(Guid transactionID);