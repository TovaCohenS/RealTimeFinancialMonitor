using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealTimeFinancialMonitor.Contracts;
using RealTimeFinancialMonitor.Models;

namespace RealTimeFinancialMonitor.Tests.Fakes
{
    public sealed class InMemoryRepository : ITransactionRepository
    {
        private readonly object _lock = new();
        private readonly List<TransactionEntity> _items = new();

        public int Count
        {
            get { lock (_lock) return _items.Count; }
        }

        public Task AddAsync(TransactionEntity entity, CancellationToken ct)
        {
            lock (_lock)
            {
                if (_items.Any(x => x.TransactionGuid == entity.TransactionGuid))
                    throw new InvalidOperationException("Duplicate transactionId (already exists).");

                // simulate auto increment
                entity.Id = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;

                _items.Add(entity);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TransactionEntity>> GetRecentAsync(int limit, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 5000);

            lock (_lock)
            {
                var result = _items
                    .OrderByDescending(x => x.Timestamp)
                    .ThenByDescending(x => x.Id)
                    .Take(limit)
                    .ToList()
                    .AsReadOnly();

                return Task.FromResult((IReadOnlyList<TransactionEntity>)result);
            }
        }
    }
}
