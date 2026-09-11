namespace AIEmployeeSupport.Application.Exceptions;

public enum AIErrorCategory
{
    RateLimitError,
    QuotaExceededError,
    AuthenticationError,
    GeneralError
}

public class AIProviderException : Exception
{
    public AIErrorCategory Category { get; }
    public string? RawResponse { get; }

    public AIProviderException(AIErrorCategory category, string message, string? rawResponse = null, Exception? innerException = null) 
        : base(message, innerException)
    {
        Category = category;
        RawResponse = rawResponse;
    }
}
