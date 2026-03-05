namespace RealTimeFinancialMonitor.Contracts;

public interface ITransactionRepository
{
    Task AddAsync(Transaction entity, CancellationToken ct);
    Task<IReadOnlyList<Transaction>> GetRecentAsync(int limit, CancellationToken ct);
    Task<Transaction?> GetByIdAsync(long id, CancellationToken ct);
    Task<Transaction?> GetByGuidAsync(Guid transactionGuid, CancellationToken ct);
    Task UpdateAsync(Transaction entity, CancellationToken ct);
}
