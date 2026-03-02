namespace RealTimeFinancialMonitor.Contracts;

public interface ITransactionProcessor
{
    Task<Guid> ProcessAsync(TransactionDto dto, CancellationToken ct);
}
