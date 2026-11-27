using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Internal;
using NetWorthTracker.Business.Features.Accounts.Persistence;
using NetWorthTracker.TestHelpers;

namespace NetWorthTracker.Business.Tests;

public sealed class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _accountService = new AccountService(_accountRepository.Object);
    }

    [Fact]
    public async Task Creating_closed_account_is_rejected_when_closing_month_is_not_after_opening_month()
    {
        var account = new AccountCreateDTO(
            "Checking", AccountType.ASSET, new DateOnly(2026, 6, 5), new DateOnly(2026, 6, 25));

        var result = await _accountService.AddAccountAsync(account);

        Assert.Multiple(
            () => Assert.Equal(AccountErrors.InvalidDates.Code, result.Error.Code),
            () => Assert.Equal(AccountErrors.InvalidDates.Message, result.Error.Message),
            () => _accountRepository.Verify(repository => repository.AddAsync(It.IsAny<AccountCreateDTO>()), Times.Never));
    }

    [Fact]
    public async Task Creating_account_is_rejected_without_saving_when_name_is_blank()
    {
        var account = new AccountCreateDTO(" ", AccountType.ASSET, new DateOnly(2025, 1, 1), null);

        var result = await _accountService.AddAccountAsync(account);

        Assert.Multiple(
            () => Assert.Equal(AccountErrors.InvalidName.Code, result.Error.Code),
            () => Assert.Equal(AccountErrors.InvalidName.Message, result.Error.Message),
            () => _accountRepository.Verify(
                repository => repository.AddAsync(It.IsAny<AccountCreateDTO>()),
                Times.Never));
    }

    [Fact]
    public async Task Creating_account_returns_duplicate_name_error_when_name_already_exists()
    {
        var account = new AccountCreateDTO("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        _accountRepository.Setup(repository => repository.AddAsync(account))
            .ThrowsAsync(new DuplicateAccountException(new Exception()));

        var result = await _accountService.AddAccountAsync(account);

        Assert.Multiple(
            () => Assert.Equal(AccountErrors.DuplicateName.Code, result.Error.Code),
            () => Assert.Equal(AccountErrors.DuplicateName.Message, result.Error.Message));
    }

    [Fact]
    public async Task Only_eligible_accounts_are_returned_at_opening_and_closing_boundaries()
    {
        var selectedDate = new DateOnly(2026, 6, 15);

        SetupAccountsForDate(
            selectedDate,
            AccountTestHelpers.CreateAccountResponseDto(1, "Opened today", selectedDate),
            AccountTestHelpers.CreateAccountResponseDto(2, "Opened before", new DateOnly(2026, 6, 14)),
            AccountTestHelpers.CreateAccountResponseDto(3, "Future opening", new DateOnly(2026, 6, 16)),
            AccountTestHelpers.CreateAccountResponseDto(4, "Closed today", new DateOnly(2026, 1, 1), selectedDate),
            AccountTestHelpers.CreateAccountResponseDto(
                5,
                "Closed before",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 6, 14)),
            AccountTestHelpers.CreateAccountResponseDto(
                6,
                "Closed after",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 6, 16)));

        IReadOnlyList<AccountResponseDTO> result = await _accountService.GetActiveAccountsAsync(selectedDate);

        Assert.Equal([1, 2, 6], result.Select(account => account.Id));
    }

    [Fact]
    public async Task Closed_accounts_are_returned_when_closed_on_or_before_selected_date()
    {
        var selectedDate = new DateOnly(2026, 6, 15);
        SetupAccountsForDate(
            selectedDate,
            AccountTestHelpers.CreateAccountResponseDto(1, "No close date"),
            AccountTestHelpers.CreateAccountResponseDto(2, "Closed today", closedDate: selectedDate),
            AccountTestHelpers.CreateAccountResponseDto(3, "Closed before", closedDate: new DateOnly(2026, 6, 14)),
            AccountTestHelpers.CreateAccountResponseDto(4, "Closed after", closedDate: new DateOnly(2026, 6, 16)));

        IReadOnlyList<AccountResponseDTO> result = await _accountService.GetClosedAccountsAsync(selectedDate);

        Assert.Equal([2, 3], result.Select(account => account.Id));
    }

    [Fact]
    public async Task Account_is_active_when_open_on_selected_date()
    {
        var selectedDate = new DateOnly(2026, 6, 15);
        SetupAccountById(
            1,
            selectedDate,
            AccountTestHelpers.CreateAccountResponseDto(1, "Checking", new DateOnly(2026, 1, 1)));

        var result = await _accountService.IsAccountActive(1, selectedDate);

        Assert.True(result);
    }

    [Fact]
    public async Task Account_is_inactive_when_closed_on_selected_date()
    {
        var selectedDate = new DateOnly(2026, 6, 15);
        SetupAccountById(
            1,
            selectedDate,
            AccountTestHelpers.CreateAccountResponseDto(
                1,
                "Closed",
                new DateOnly(2026, 1, 1),
                selectedDate));

        var result = await _accountService.IsAccountActive(1, selectedDate);

        Assert.False(result);
    }

    [Fact]
    public async Task Account_is_inactive_when_it_does_not_exist()
    {
        var selectedDate = new DateOnly(2026, 6, 15);
        SetupAccountById(1, selectedDate, null);

        var result = await _accountService.IsAccountActive(1, selectedDate);

        Assert.False(result);
    }

    private void SetupAccountsForDate(DateOnly selectedDate, params AccountResponseDTO[] accounts)
    {
        _accountRepository.Setup(repository => repository.GetAllAsync(selectedDate))
            .ReturnsAsync(accounts);
    }

    private void SetupAccountById(int accountId, DateOnly selectedDate, AccountResponseDTO? account)
    {
        _accountRepository.Setup(repository => repository.GetByIdAsync(accountId, selectedDate))
            .ReturnsAsync(account);
    }
}
