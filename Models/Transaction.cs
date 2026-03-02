namespace RealTimeFinancialMonitor.Models;

public sealed record Transaction
{
    public long Id { get; set; }
    public Guid TransactionGuid { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public TransactionStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
