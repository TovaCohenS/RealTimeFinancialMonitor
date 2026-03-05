using RealTimeFinancialMonitor.Hub;

namespace RealTimeFinancialMonitor.Services;

public sealed class SignalRTransactionBroadcaster : ITransactionBroadcaster
{
    private readonly IHubContext<TransactionsHub> _hub;

    public SignalRTransactionBroadcaster(IHubContext<TransactionsHub> hub) => _hub = hub;

    public async Task BroadcastAsync(TransactionDto dto, CancellationToken ct)
    {
        await  _hub.Clients.All.SendAsync("transactionReceived", dto, ct);
    }
}
