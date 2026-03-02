namespace RealTimeFinancialMonitor.Exceptions;

public sealed class DuplicateTransactionException : Exception
{
    public DuplicateTransactionException()
        : base("A transaction with this ID already exists.")
    {
    }

    public DuplicateTransactionException(string message)
        : base(message)
    {
    }

    public DuplicateTransactionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
