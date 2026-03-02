namespace RealTimeFinancialMonitor.Exceptions;

public sealed class TransactionValidationException : Exception
{
    public TransactionValidationException()
        : base("Transaction validation failed.")
    {
    }

    public TransactionValidationException(string message)
        : base(message)
    {
    }

    public TransactionValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
