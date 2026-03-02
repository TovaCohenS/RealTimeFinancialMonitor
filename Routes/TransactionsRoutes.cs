namespace RealTimeFinancialMonitor.Routes;
public static class TransactionsRoutes
{
    public static IEndpointRouteBuilder MapTransactionsRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions")
            .WithTags("Transactions");

        group.MapPost("/", AddTransaction);

        group.MapGet("/recent", GetRecent);

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

    static async Task<Results<ValidationProblem, Ok<AddTransactionReponse>>> AddTransaction(
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
        return TypedResults.Ok(new AddTransactionReponse(tid));
    }


}
