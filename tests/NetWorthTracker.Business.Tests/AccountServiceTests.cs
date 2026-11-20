using Microsoft.Extensions.DependencyInjection;
using Moq;

using NetWorthTracker.Business;
using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Persistence;
using NetWorthTracker.Core.Exceptions;

namespace NetWorthTracker.Business.Tests;

public sealed class AccountServiceTests
{
    [Fact]
    public async Task AddAccountAsync_WhenNameIsBlank_ThrowsValidationExceptionWithoutWriting()
    {
        // Arrange
        var repository = new Mock<IAccountRepository>();
        var services = new ServiceCollection();
        services.AddSingleton(repository.Object);
        ServiceRegistry.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var accountService = provider.GetRequiredService<IAccountService>();
        var account = new AccountCreateDTO("  ", AccountType.ASSET, new DateOnly(2026, 1, 1), null);

        // Act
        await Assert.ThrowsAsync<ValidationException>(() => accountService.AddAccountAsync(account));

        // Assert
        repository.Verify(
            repository => repository.AddAsync(It.IsAny<AccountCreateDTO>()),
            Times.Never);
    }
}
