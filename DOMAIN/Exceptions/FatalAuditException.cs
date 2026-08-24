namespace Tweet_Audit.DOMAIN.Exceptions;

public class FatalAuditException : Exception
{
    public int? StatusCode { get; }

    public FatalAuditException(string message, int? statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}