namespace RealTimeFinancialMonitor.Contracts;

public interface ITransactionRepository
{
    Task AddAsync(Transaction entity, CancellationToken ct);
    Task<IReadOnlyList<Transaction>> GetRecentAsync(int limit, CancellationToken ct);
}
