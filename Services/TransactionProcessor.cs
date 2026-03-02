namespace RealTimeFinancialMonitor.Services;

public sealed class TransactionProcessor : ITransactionProcessor
{
    private readonly ITransactionRepository _repo;
    private readonly ITransactionBroadcaster _broadcaster;

    public TransactionProcessor(ITransactionRepository repo, ITransactionBroadcaster broadcaster)
    {
        _repo = repo;
        _broadcaster = broadcaster;
    }

    public async Task<Guid> ProcessAsync(TransactionDto dto, CancellationToken ct)
    {
        var entity = TransactionMapper.ToEntity(dto);

        await _repo.AddAsync(entity, ct);
        await _broadcaster.BroadcastAsync(dto, ct);
        return entity.TransactionGuid;
    }
}
