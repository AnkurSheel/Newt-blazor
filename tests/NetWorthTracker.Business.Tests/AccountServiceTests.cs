using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Internal;
using NetWorthTracker.Business.Features.Accounts.Persistence;

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
}
