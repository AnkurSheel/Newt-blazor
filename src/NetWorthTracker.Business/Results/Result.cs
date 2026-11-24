namespace NetWorthTracker.Business.Results;

public sealed class Result
{
    private Result(Error error)
    {
        Error = error;
        IsSuccess = error == Error.None;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(Error.None);
    }

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error == Error.None)
        {
            throw new ArgumentException("A failure result must include an error.", nameof(error));
        }

        return new Result(error);
    }
}
