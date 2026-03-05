namespace RealTimeFinancialMonitor.Services;

public static class TransactionMapper
{
    public static Transaction ToEntity(TransactionDto dto)
    {
        var status = ParseStatus(dto.Status);
        var timestamp = NormalizeTimestamp(dto.Timestamp);

        return new Transaction
        {
            TransactionGuid = dto.TransactionId,
            Amount = dto.Amount,
            Currency = dto.Currency.Trim().ToUpperInvariant(),
            Status = status,
            Timestamp = timestamp
        };
    }

    public static TransactionDto ToDto(Transaction entity)
    {
        return new TransactionDto
        {
            TransactionId = entity.TransactionGuid,
            Amount = entity.Amount,
            Currency = entity.Currency,
            Status = entity.Status.ToString(),
            Timestamp = DateTime.SpecifyKind(entity.Timestamp, DateTimeKind.Utc)
        };
    }

    private static TransactionStatus ParseStatus(string status)
    {
        return status.Trim() switch
        {
            var s when s.Equals("Pending", StringComparison.OrdinalIgnoreCase) => TransactionStatus.Pending,
            var s when s.Equals("Completed", StringComparison.OrdinalIgnoreCase) => TransactionStatus.Completed,
            var s when s.Equals("Failed", StringComparison.OrdinalIgnoreCase) => TransactionStatus.Failed,
            _ => throw new ArgumentException($"Invalid status: {status}. Must be one of: Pending, Completed, Failed.")
        };
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
