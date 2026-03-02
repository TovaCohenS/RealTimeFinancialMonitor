namespace RealTimeFinancialMonitor.Contracts;

public interface ITransactionBroadcaster
{
    Task BroadcastAsync(TransactionDto dto, CancellationToken ct);
}
