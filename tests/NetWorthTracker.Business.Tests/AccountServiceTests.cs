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
    public async Task AddAccountAsync_WhenNameIsBlank_ReturnsInvalidNameWithoutPersisting()
    {
        var account = new AccountCreateDTO(" ", AccountType.ASSET, new DateOnly(2025, 1, 1), null);

        var result = await _accountService.AddAccountAsync(account);

        Assert.Multiple(
            () => Assert.Equal(AccountErrors.InvalidName, result.Error),
            () => _accountRepository.Verify(
                repository => repository.AddAsync(It.IsAny<AccountCreateDTO>()),
                Times.Never));
    }

    [Fact]
    public async Task AddAccountAsync_WhenRepositoryThrowsException_ReturnsDuplicateNameError()
    {
        var account = new AccountCreateDTO("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        _accountRepository.Setup(repository => repository.AddAsync(account))
            .ThrowsAsync(new DuplicateAccountException(new Exception()));

        var result = await _accountService.AddAccountAsync(account);

        Assert.Equal(AccountErrors.DuplicateName, result.Error);
    }

    [Fact]
    public async Task GetActiveAccountsAsync_WithOpeningAndClosingBoundaries_ReturnsOnlyEligibleAccounts()
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
    public async Task GetClosedAccountsAsync_WithClosingBoundary_ReturnsOnlyAccountsClosedBySelectedDate()
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
    public async Task IsAccountActive_WhenAccountIsActiveOnSelectedDate_ReturnsTrue()
    {
        var selectedDate = new DateOnly(2026, 6, 15);
        SetupAccountsForDate(
            selectedDate,
            AccountTestHelpers.CreateAccountResponseDto(1, "Checking", new DateOnly(2026, 1, 1)),
            AccountTestHelpers.CreateAccountResponseDto(
                2,
                "Closed",
                new DateOnly(2026, 1, 1),
                selectedDate));

        var result = await _accountService.IsAccountActive(1, selectedDate);

        Assert.True(result);
    }

    [Fact]
    public async Task IsAccountActive_WhenAccountIsNotActiveOnSelectedDate_ReturnsFalse()
    {
        var selectedDate = new DateOnly(2026, 6, 15);
        SetupAccountsForDate(
            selectedDate,
            AccountTestHelpers.CreateAccountResponseDto(
                1,
                "Closed",
                new DateOnly(2026, 1, 1),
                selectedDate));

        var result = await _accountService.IsAccountActive(1, selectedDate);

        Assert.False(result);
    }

    private void SetupAccountsForDate(DateOnly selectedDate, params AccountResponseDTO[] accounts)
    {
        _accountRepository.Setup(repository => repository.GetAllAsync(selectedDate))
            .ReturnsAsync(accounts);
    }
}
