namespace RealTimeFinancialMonitor.Routes;
public static class TransactionsRoutes
{
    public static IEndpointRouteBuilder MapTransactionsRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions")
            .WithTags("Transactions");

        group.MapPost("/", AddTransaction);
        group.MapGet("/recent", GetRecent);
        group.MapPut("/{transactionGuid}/status", UpdateTransactionStatus);

        return app;
    }

    static async Task<Ok<List<TransactionDto>>> GetRecent(
           int? limit,
            ITransactionRepository repo,
            CancellationToken ct)
    {
        var limitValue = limit.GetValueOrDefault(100);
        var entities = await repo.GetRecentAsync(limitValue, ct);

        var dtos = entities.Select(x => new TransactionDto
        {
            TransactionId = x.TransactionGuid,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            Timestamp = DateTime.SpecifyKind(x.Timestamp, DateTimeKind.Utc)
        }).ToList();

        return TypedResults.Ok(dtos);
    }

    static async Task<Results<ValidationProblem, Ok<AddTransactionResponse>>> AddTransaction(
            TransactionDto dto,
            IValidator<TransactionDto> validator,
            ITransactionProcessor processor,
            CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var tid = await processor.ProcessAsync(dto, ct);
        return TypedResults.Ok(new AddTransactionResponse(tid));
    }

    static async Task<Results<NotFound, Ok<TransactionDto>>> UpdateTransactionStatus(
        Guid transactionGuid,
        TransactionStatus newStatus,
        ITransactionRepository repo,
        ITransactionBroadcaster broadcaster,
        CancellationToken ct)
    {
        var transaction = await repo.GetByGuidAsync(transactionGuid, ct);
        
        if (transaction is null)
            return TypedResults.NotFound();

        // Create new record with updated status (record types are immutable)
        var updatedTransaction = transaction with { Status = newStatus };

        // This will throw ConcurrencyException if RowVersion doesn't match
        await repo.UpdateAsync(updatedTransaction, ct);

        var dto = TransactionMapper.ToDto(updatedTransaction);
        await broadcaster.BroadcastAsync(dto, ct);

        return TypedResults.Ok(dto);
    }
}
