namespace NetWorthTracker.Business.Features.Accounts.Api;

public sealed class DuplicateAccountException : Exception
{
    public DuplicateAccountException(Exception? innerException = null) : base(
        "This account has already been added.",
        innerException)
    {
    }
}
