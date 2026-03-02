using RealTimeFinancialMonitor.Enums;
using RealTimeFinancialMonitor.Models;
using RealTimeFinancialMonitor.Tests.Fakes;

namespace RealTimeFinancialMonitor.Tests
{
    public sealed class StorageTests
    {
        [Fact]
        public async Task GetRecentAsync_ReturnsNewestFirst()
        {
            var repo = new InMemoryRepository();

            var older = new TransactionEntity
            {
                TransactionGuid = Guid.NewGuid(),
                Amount = 1m,
                Currency = "USD",
                Status = TransactionStatus.Pending,
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
            };

            var newer = new TransactionEntity
            {
                TransactionGuid = Guid.NewGuid(),
                Amount = 2m,
                Currency = "USD",
                Status = TransactionStatus.Failed,
                Timestamp = DateTime.UtcNow,
            };

            await repo.AddAsync(older, CancellationToken.None);
            await repo.AddAsync(newer, CancellationToken.None);

            var recent = await repo.GetRecentAsync(10, CancellationToken.None);

            Assert.Equal(2, recent.Count);
            Assert.Equal(newer.TransactionGuid, recent[0].TransactionGuid);
            Assert.Equal(older.TransactionGuid, recent[1].TransactionGuid);
        }

        [Fact]
        public async Task GetRecentAsync_RespectsLimit()
        {
            var repo = new InMemoryRepository();

            for (int i = 0; i < 20; i++)
            {
                await repo.AddAsync(new TransactionEntity
                {
                    TransactionGuid = Guid.NewGuid(),
                    Amount = i,
                    Currency = "USD",
                    Status = TransactionStatus.Completed,
                    Timestamp = DateTime.UtcNow.AddMinutes(-i),
                }, CancellationToken.None);
            }

            var recent5 = await repo.GetRecentAsync(5, CancellationToken.None);

            Assert.Equal(5, recent5.Count);
            // first is newest
            Assert.True(recent5[0].Timestamp >= recent5[4].Timestamp);
        }
    }
}
