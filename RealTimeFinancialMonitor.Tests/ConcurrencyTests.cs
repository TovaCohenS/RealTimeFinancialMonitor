using RealTimeFinancialMonitor.Dtos;
using RealTimeFinancialMonitor.Services;
using RealTimeFinancialMonitor.Tests.Fakes;

namespace RealTimeFinancialMonitor.Tests
{
    public sealed class ConcurrencyTests
    {
        [Fact]
        public async Task ProcessAsync_ManyConcurrentTransactions_AllStoredAndBroadcasted()
        {
            var repo = new InMemoryRepository();
            var broadcaster = new FakeBroadcaster();
            var processor = new TransactionProcessor(repo, broadcaster);

            var tasks = Enumerable.Range(0, 300).Select(i =>
            {
                var tx = new TransactionDto
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Amount = 1m + i,
                    Currency = "USD",
                    Status = (i % 3 == 0) ? "Pending" : (i % 3 == 1) ? "Completed" : "Failed",
                    Timestamp = DateTime.UtcNow
                };
                return processor.ProcessAsync(tx, CancellationToken.None);
            });

            await Task.WhenAll(tasks);

            Assert.Equal(300, repo.Count);
            Assert.Equal(300, broadcaster.Broadcasted.Count);
        }

        [Fact]
        public async Task ProcessAsync_ConcurrentDuplicateIds_OnlyOneSucceeds()
        {
            var repo = new InMemoryRepository();
            var broadcaster = new FakeBroadcaster();
            var processor = new TransactionProcessor(repo, broadcaster);

            var sameId = Guid.NewGuid().ToString();

            var tasks = Enumerable.Range(0, 50).Select(_ =>
            {
                var tx = new TransactionDto
                {
                    TransactionId = sameId,
                    Amount = 10m,
                    Currency = "USD",
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow
                };

                return Task.Run(async () =>
                {
                    try
                    {
                        await processor.ProcessAsync(tx, CancellationToken.None);
                        return true; // succeeded
                    }
                    catch
                    {
                        return false; // failed
                    }
                });
            });

            var results = await Task.WhenAll(tasks);

            Assert.Equal(1, results.Count(x => x)); // exactly one success
            Assert.Equal(1, repo.Count);
            Assert.Single(broadcaster.Broadcasted);
        }
    }
}
