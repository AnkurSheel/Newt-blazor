using System.Globalization;

using CsvHelper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetWorthTracker.Common;
using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Core.Features.MonthlyBalance;
using NetWorthTracker.Data;
using NetWorthTracker.Data.Api;
using NetWorthTracker.Seeder;
using NetWorthTracker.Services.Api.Features.Account;
using NetWorthTracker.Services.Api.Features.MonthlyBalance;

using ServiceRegistry = NetWorthTracker.Data.ServiceRegistry;
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", false);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.json", true);
#endif

LocalEnvFile.AddToConfiguration(builder.Configuration);

// 1. Configure options and IAppPaths for AppDbContext
builder.Services.Configure<AppPathOptions>(builder.Configuration.GetSection("AppPaths"));
builder.Services.AddSingleton<IAppPaths, DefaultAppPaths>();

// 2. Register Data and Services layers
ServiceRegistry.ConfigureServices(builder.Services);
NetWorthTracker.Services.ServiceRegistry.ConfigureServices(builder.Services);

// 3. Register any seeder-specific services if needed
// builder.Services.AddTransient<DataSeeder>();

using var host = builder.Build();

// 4. Create scope and resolve services to seed data
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
    var monthlyBalanceService = scope.ServiceProvider.GetRequiredService<IMonthlyBalanceService>();

    var csvDirectory = @"C:\Projects\NetWorthTracker\";

    Console.WriteLine("Starting data seeding...");

    await SeedAccountsFromCsv(csvDirectory, accountService);
    await SeedMonthlyBalancesFromCsv(csvDirectory, monthlyBalanceService);

    Console.WriteLine("Data seeding completed.");
}

async Task SeedAccountsFromCsv(string csvDirectory, IAccountService accountService)
{
    var acccountCsvPath = $@"{csvDirectory}accounts.csv";

    using var reader = new StreamReader(acccountCsvPath);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    List<AccountCsvRecord> records = csv.GetRecords<AccountCsvRecord>()
        .ToList();

    foreach (var record in records)
    {
        var accountDto = new AccountCreateDTO(
            record.Name,
            (AccountType)record.Type,
            record.OpenDate.AddMonths(1),
            record.ClosedDate?.AddMonths(1));

        await accountService.AddAccountAsync(accountDto);
    }
    Console.WriteLine($"Seeded {records.Count} accounts.");
}

async Task SeedMonthlyBalancesFromCsv(string csvDirectory, IMonthlyBalanceService monthlyBalanceService)
{
    var monthlyBalancesCsv = $@"{csvDirectory}monthly_balances.csv";

    using var reader = new StreamReader(monthlyBalancesCsv);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    List<MonthlyBalancesCsvRecord> records = csv.GetRecords<MonthlyBalancesCsvRecord>()
        .ToList();

    foreach (var record in records)
    {
        var monthlyBalanceDto = new MonthlyBalanceCreateDTO(
            record.AccountId,
            record.MonthDate.AddMonths(1),
            record.Amount);

        await monthlyBalanceService.AddMonthlyBalanceAsync(monthlyBalanceDto);
    }
    Console.WriteLine($"Seeded {records.Count} monthly balances.");
}
