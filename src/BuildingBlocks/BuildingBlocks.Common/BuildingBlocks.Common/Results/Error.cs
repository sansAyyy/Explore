namespace BuildingBlocks.Common.Results;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string message)
    {
        return new Error(ErrorCodes.ValidationFailed, message);
    }

    public static Error NotFound(string message)
    {
        return new Error(ErrorCodes.NotFound, message);
    }

    public static Error Unauthorized(string message)
    {
        return new Error(ErrorCodes.Unauthorized, message);
    }

    public static Error Forbidden(string message)
    {
        return new Error(ErrorCodes.Forbidden, message);
    }

    public static Error Conflict(string message)
    {
        return new Error(ErrorCodes.Conflict, message);
    }

    public static Error BadRequest(string message)
    {
        return new Error(ErrorCodes.BadRequest, message);
    }
}

