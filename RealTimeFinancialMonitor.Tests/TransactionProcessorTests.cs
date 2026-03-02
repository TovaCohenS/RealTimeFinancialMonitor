using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealTimeFinancialMonitor.Dtos;
using RealTimeFinancialMonitor.Services;
using RealTimeFinancialMonitor.Tests.Fakes;
using Xunit;

namespace RealTimeFinancialMonitor.Tests
{
    public sealed class TransactionProcessorTests
    {
        [Fact]
        public async Task ProcessAsync_ValidTransaction_StoresAndBroadcasts()
        {
            var repo = new InMemoryRepository();
            var broadcaster = new FakeBroadcaster();
            var processor = new TransactionProcessor(repo, broadcaster);

            var tx = new TransactionDto
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = 1500.5m,
                Currency = "usd",
                Status = "Completed",
                Timestamp = DateTime.UtcNow
            };

            await processor.ProcessAsync(tx, CancellationToken.None);

            Assert.Equal(1, repo.Count);
            Assert.Single(broadcaster.Broadcasted);
            Assert.Equal(tx.TransactionId, broadcaster.Broadcasted[0].TransactionId);
            Assert.Equal("Completed", broadcaster.Broadcasted[0].Status); // string contract
        }

        [Fact]
        public async Task ProcessAsync_InvalidGuid_Throws_AndDoesNotStoreOrBroadcast()
        {
            var repo = new InMemoryRepository();
            var broadcaster = new FakeBroadcaster();
            var processor = new TransactionProcessor(repo, broadcaster);

            var tx = new TransactionDto
            {
                TransactionId = "not-a-guid",
                Amount = 10m,
                Currency = "USD",
                Status = "Pending",
                Timestamp = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<ArgumentException>(() => processor.ProcessAsync(tx, CancellationToken.None));
            Assert.Equal(0, repo.Count);
            Assert.Empty(broadcaster.Broadcasted);
        }

        [Fact]
        public async Task ProcessAsync_InvalidStatus_Throws_AndDoesNotStoreOrBroadcast()
        {
            var repo = new InMemoryRepository();
            var broadcaster = new FakeBroadcaster();
            var processor = new TransactionProcessor(repo, broadcaster);

            var tx = new TransactionDto
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = 10m,
                Currency = "USD",
                Status = "UNKNOWN",
                Timestamp = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<ArgumentException>(() => processor.ProcessAsync(tx, CancellationToken.None));
            Assert.Equal(0, repo.Count);
            Assert.Empty(broadcaster.Broadcasted);
        }

        [Fact]
        public async Task ProcessAsync_DuplicateTransactionId_Throws_AndDoesNotBroadcastSecondTime()
        {
            var repo = new InMemoryRepository();
            var broadcaster = new FakeBroadcaster();
            var processor = new TransactionProcessor(repo, broadcaster);

            var id = Guid.NewGuid().ToString();

            var tx1 = new TransactionDto
            {
                TransactionId = id,
                Amount = 10m,
                Currency = "USD",
                Status = "Pending",
                Timestamp = DateTime.UtcNow
            };

            var tx2 = new TransactionDto
            {
                TransactionId = id,
                Amount = 20m,
                Currency = "USD",
                Status = "Failed",
                Timestamp = DateTime.UtcNow
            };

            await processor.ProcessAsync(tx1, CancellationToken.None);

            // second should fail on unique constraint
            await Assert.ThrowsAsync<InvalidOperationException>(() => processor.ProcessAsync(tx2, CancellationToken.None));

            Assert.Equal(1, repo.Count);
            Assert.Single(broadcaster.Broadcasted); // broadcast only first one
            Assert.Equal("Pending", broadcaster.Broadcasted[0].Status);
        }
    }
}
