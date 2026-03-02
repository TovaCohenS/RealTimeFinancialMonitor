namespace RealTimeFinancialMonitor.Validators;

public sealed class TransactionDtoValidator : AbstractValidator<TransactionDto>
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending", "Completed", "Failed"
    };

    public TransactionDtoValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEqual(Guid.Empty)
            .WithMessage("transactionId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0.")
            .LessThan(1_000_000_000)
            .WithMessage("Amount must be less than 1 billion.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3, 8)
            .Must(c => c.Trim().All(char.IsLetter))
            .WithMessage("Currency must be 3-8 letters only (e.g., USD, EUR).");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s.Trim()))
            .WithMessage("Status must be one of: Pending, Completed, Failed.");

        RuleFor(x => x.Timestamp)
            .NotEqual(default(DateTime))
            .WithMessage("Timestamp is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp cannot be more than 5 minutes in the future.")
            .GreaterThan(DateTime.UtcNow.AddYears(-10))
            .WithMessage("Timestamp cannot be older than 10 years.");
    }
}
