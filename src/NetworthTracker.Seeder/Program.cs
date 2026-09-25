using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetWorthTracker.Common;
using NetWorthTracker.Data.Api;
using NetWorthTracker.Seeder;
using NetWorthTracker.Services.Api.Features.Account;
using NetWorthTracker.Services.Api.Features.MonthlyBalance;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configure options and IAppPaths for AppDbContext
builder.Services.Configure<AppPathOptions>(builder.Configuration.GetSection("AppPaths"));
builder.Services.AddSingleton<IAppPaths, SeederAppPaths>();

// 2. Register Data and Services layers
NetWorthTracker.Data.ServiceRegistry.ConfigureServices(builder.Services);
NetWorthTracker.Services.ServiceRegistry.ConfigureServices(builder.Services);

// 3. Register any seeder-specific services if needed
// builder.Services.AddTransient<DataSeeder>();

using var host = builder.Build();

// 4. Create scope and resolve services to seed data
using (var scope = host.Services.CreateScope())
{
    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
    var monthlyBalanceService = scope.ServiceProvider.GetRequiredService<IMonthlyBalanceService>();

    Console.WriteLine("Starting data seeding...");

    // Seed using accountService, monthlyBalanceService, or CSV parsers

     Console.WriteLine("Data seeding completed.");
}