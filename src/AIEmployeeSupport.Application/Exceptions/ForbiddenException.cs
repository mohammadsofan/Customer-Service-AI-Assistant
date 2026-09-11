namespace AIEmployeeSupport.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Access is forbidden.") { }
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}
