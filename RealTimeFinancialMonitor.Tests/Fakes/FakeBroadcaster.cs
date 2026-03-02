using RealTimeFinancialMonitor.Contracts;
using RealTimeFinancialMonitor.Dtos;

namespace RealTimeFinancialMonitor.Tests.Fakes
{
    public sealed class FakeBroadcaster : ITransactionBroadcaster
    {
        private readonly object _lock = new();
        private readonly List<TransactionDto> _broadcasted = new();

        public IReadOnlyList<TransactionDto> Broadcasted
        {
            get { lock (_lock) return _broadcasted.ToList().AsReadOnly(); }
        }

        public Task BroadcastAsync(TransactionDto dto, CancellationToken ct)
        {
            lock (_lock)
            {
                _broadcasted.Add(dto);
            }
            return Task.CompletedTask;
        }
    }
}
