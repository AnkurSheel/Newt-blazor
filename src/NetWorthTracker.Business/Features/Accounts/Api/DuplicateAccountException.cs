namespace NetWorthTracker.Business.Features.Accounts.Api;

public sealed class DuplicateAccountException : Exception
{
    public DuplicateAccountException(Exception innerException)
        : base("This account has already been added.", innerException)
    {
    }
}
