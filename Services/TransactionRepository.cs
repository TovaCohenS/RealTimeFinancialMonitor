namespace RealTimeFinancialMonitor.Services;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Transaction entity, CancellationToken ct)
    {
        _db.Transactions.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new DuplicateTransactionException("A transaction with this ID already exists.", ex);
        }
    }

    public async Task<IReadOnlyList<Transaction>> GetRecentAsync(int limit, CancellationToken ct)
    {
        limit = Math.Clamp(limit, 1, 5000);

        return await _db.Transactions
            .AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .ToListAsync(ct);
    }
}
