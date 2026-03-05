namespace RealTimeFinancialMonitor.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        var problemDetails = exception switch
        {
            TransactionValidationException ex => HandleValidationException(ex, traceId),
            DuplicateTransactionException ex => HandleDuplicateTransaction(ex, traceId),
            ConcurrencyException ex => HandleConcurrencyException(ex, traceId),
            InvalidOperationException ex => HandleInvalidOperation(ex, traceId),
            ArgumentException ex => HandleArgumentException(ex, traceId),
            _ => HandleUnexpectedException(exception, traceId)
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails HandleValidationException(TransactionValidationException ex, string traceId)
    {
        _logger.LogWarning(ex, "Validation error: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        return CreateProblemDetails(
            StatusCodes.Status400BadRequest,
            "Validation Error",
            ex.Message,
            "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            traceId,
            "VALIDATION_ERROR"
        );
    }

    private ProblemDetails HandleArgumentException(ArgumentException ex, string traceId)
    {
        _logger.LogWarning(ex, "Validation error: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        return CreateProblemDetails(
            StatusCodes.Status400BadRequest,
            "Bad Request",
            ex.Message,
            "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            traceId,
            "BAD_REQUEST"
        );
    }

    private ProblemDetails HandleDuplicateTransaction(DuplicateTransactionException ex, string traceId)
    {
        _logger.LogWarning(ex, "Duplicate transaction attempt: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        return CreateProblemDetails(
            StatusCodes.Status409Conflict,
            "Duplicate Transaction",
            ex.Message,
            "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.9",
            traceId,
            "DUPLICATE_TRANSACTION"
        );
    }

    private ProblemDetails HandleConcurrencyException(ConcurrencyException ex, string traceId)
    {
        _logger.LogWarning(ex, "Concurrency conflict: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        return CreateProblemDetails(
            StatusCodes.Status409Conflict,
            "Concurrency Conflict",
            ex.Message,
            "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.9",
            traceId,
            "CONCURRENCY_CONFLICT"
        );
    }

    private ProblemDetails HandleInvalidOperation(InvalidOperationException ex, string traceId)
    {
        _logger.LogError(ex, "Operation error: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        return CreateProblemDetails(
            StatusCodes.Status422UnprocessableEntity,
            "Unprocessable Entity",
            _environment.IsDevelopment() 
                ? ex.Message 
                : "The operation could not be completed.",
            "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
            traceId,
            "UNPROCESSABLE_ENTITY"
        );
    }

    private ProblemDetails HandleUnexpectedException(Exception ex, string traceId)
    {
        _logger.LogError(ex, "Unhandled server error: {Message}. TraceId: {TraceId}", ex.Message, traceId);

        return CreateProblemDetails(
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            _environment.IsDevelopment() 
                ? ex.Message 
                : "An unexpected error occurred. Please contact support with the trace ID.",
            "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
            traceId,
            "INTERNAL_SERVER_ERROR"
        );
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        string type,
        string traceId,
        string errorCode)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = type
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["errorCode"] = errorCode;

        return problemDetails;
    }
}
