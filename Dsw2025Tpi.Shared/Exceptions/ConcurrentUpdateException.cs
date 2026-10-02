namespace Dsw2025Tpi.Shared.Exceptions;

public sealed class ConcurrentUpdateException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Conflict;

    public ConcurrentUpdateException(Exception innerException)
        : base("RESOURCE_CONCURRENTLY_MODIFIED", innerException)
    {
    }
}
