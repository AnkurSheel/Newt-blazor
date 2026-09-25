using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Core.Features.FinancialSummary;
using NetWorthTracker.Services.Api.Features.Account;
using NetWorthTracker.Services.Api.Features.FinancialSummary;

namespace NetWorthTracker.Services.Features.FinancialSummary;

public class FinancialSummaryService : IFinancialSummaryService
{
    private readonly IAccountService _accountService;

    public FinancialSummaryService(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<FinancialSummaryDTO> GetFinancialSummaryAsync(DateOnly selectedDate)
    {
        IReadOnlyList<AccountResponseDTO> accounts = await _accountService.GetAccountsAsync(selectedDate);

        var totalAssets = accounts.Where(a => !a.IsClosedOn(selectedDate) && a.Type == AccountType.ASSET)
            .Sum(a => a.LatestBalance);

        var totalLiabilities = accounts.Where(a => !a.IsClosedOn(selectedDate) && a.Type == AccountType.LIABILITY)
            .Sum(a => a.LatestBalance);

        var netWorth = totalAssets - totalLiabilities;

        return new FinancialSummaryDTO(totalAssets, totalLiabilities, netWorth);
    }
}
