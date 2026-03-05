using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RealTimeFinancialMonitor.Services;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TransactionRepository> _logger;
    
    private const string RecentCacheKeyPrefix = "transactions:recent:";
    private const string TransactionByIdPrefix = "transaction:id:";
    private const string TransactionByGuidPrefix = "transaction:guid:";
    
    
    private static readonly int[] CommonLimits = { 10, 50, 100, 500, 1000, 5000 };

    public TransactionRepository(
        AppDbContext db, 
        IDistributedCache cache,
        ILogger<TransactionRepository> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task AddAsync(Transaction entity, CancellationToken ct)
    {
        _db.Transactions.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
            
           
            await InvalidateCacheAsync(entity, ct);
            _logger.LogInformation("Transaction {TransactionGuid} added and cache invalidated", entity.TransactionGuid);
        }
        catch (DbUpdateException ex)
        {
            throw new DuplicateTransactionException("A transaction with this ID already exists.", ex);
        }
    }

    public async Task<IReadOnlyList<Transaction>> GetRecentAsync(int limit, CancellationToken ct)
    {
        limit = Math.Clamp(limit, 1, 5000);
        var cacheKey = $"{RecentCacheKeyPrefix}{limit}";

       
        var cachedBytes = await _cache.GetAsync(cacheKey, ct);
        if (cachedBytes is not null)
        {
            var cached = JsonSerializer.Deserialize<List<Transaction>>(cachedBytes);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for recent transactions (limit: {Limit})", limit);
                return cached;
            }
        }

        _logger.LogDebug("Cache miss for recent transactions (limit: {Limit})", limit);

        
        var transactions = await _db.Transactions
            .AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .ToListAsync(ct);

        
        var json = JsonSerializer.SerializeToUtf8Bytes(transactions);
        await _cache.SetAsync(cacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        }, ct);

        return transactions;
    }

    public async Task<Transaction?> GetByIdAsync(long id, CancellationToken ct)
    {
        var cacheKey = $"{TransactionByIdPrefix}{id}";

       
        var cachedBytes = await _cache.GetAsync(cacheKey, ct);
        if (cachedBytes is not null)
        {
            var cached = JsonSerializer.Deserialize<Transaction>(cachedBytes);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for transaction ID: {Id}", id);
                return cached;
            }
        }

        _logger.LogDebug("Cache miss for transaction ID: {Id}", id);

        
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (transaction is not null)
        {
            
            var json = JsonSerializer.SerializeToUtf8Bytes(transaction);
            await _cache.SetAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }, ct);
        }

        return transaction;
    }

    public async Task<Transaction?> GetByGuidAsync(Guid transactionGuid, CancellationToken ct)
    {
        var cacheKey = $"{TransactionByGuidPrefix}{transactionGuid}";

        var cachedBytes = await _cache.GetAsync(cacheKey, ct);
        if (cachedBytes is not null)
        {
            var cached = JsonSerializer.Deserialize<Transaction>(cachedBytes);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for transaction GUID: {TransactionGuid}", transactionGuid);
                return cached;
            }
        }

        _logger.LogDebug("Cache miss for transaction GUID: {TransactionGuid}", transactionGuid);

        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(x => x.TransactionGuid == transactionGuid, ct);

        if (transaction is not null)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(transaction);
            await _cache.SetAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }, ct);
        }

        return transaction;
    }

    public async Task UpdateAsync(Transaction entity, CancellationToken ct)
    {
        _db.Transactions.Update(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
            
            
            await InvalidateCacheAsync(entity, ct);
            _logger.LogInformation("Transaction {TransactionGuid} updated and cache invalidated", entity.TransactionGuid);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(
                $"The transaction (ID: {entity.Id}) was modified by another process. Please retry the operation.", 
                ex);
        }
    }

    private async Task InvalidateCacheAsync(Transaction entity, CancellationToken ct)
    {
        
        foreach (var limit in CommonLimits)
        {
            await _cache.RemoveAsync($"{RecentCacheKeyPrefix}{limit}", ct);
        }
        
        
        await _cache.RemoveAsync($"{TransactionByIdPrefix}{entity.Id}", ct);
        await _cache.RemoveAsync($"{TransactionByGuidPrefix}{entity.TransactionGuid}", ct);
        
        _logger.LogDebug("Cache invalidated for transaction {TransactionGuid}", entity.TransactionGuid);
    }
}
