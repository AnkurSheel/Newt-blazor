# NetWorthTracker - Project Overview & Guidelines

## 1. Overview & Architecture

**NetWorthTracker** is a personal finance management application built on .NET 9 using .NET MAUI with Blazor Hybrid. It allows users to track financial accounts, historical monthly balances, net worth trends, asset/liability breakdowns, and perform financial projections.

The solution follows clean architecture principles with distinct separation of concerns across projects:

```
NetWorthTracker/
├── src/
│   ├── NetWorthTracker.App/          # MAUI Blazor Hybrid Host Application (.NET 9 MAUI)
│   ├── NetWorthTracker.UI/           # Blazor UI Components, Pages, and Styles (Tailwind CSS, Syncfusion)
│   ├── NetWorthTracker.Services/     # Business logic & application services
│   ├── NetWorthTracker.Services.Api/ # Service abstractions and interfaces
│   ├── NetWorthTracker.Data/         # EF Core DbContext, SQLite repositories & migrations
│   ├── NetWorthTracker.Data.Api/     # Data access interfaces
│   ├── NetWorthTracker.Models/       # Domain entities, DTOs, Enums, and custom exceptions
│   ├── NetWorthTracker.Common/       # Service registration & DI helper extensions
│   └── NetWorthTracker.Seeder/       # Database seeder console application for CSV imports
├── tests/
│   └── NetWorthTracker.UI.Tests/     # Unit & component tests using bUnit and xUnit
└── docs/
    └── guidelines/                   # Shared developer and AI agent documentation
```

---

## 2. Technology Stack & Key Libraries

- **Language & Runtime:** C# 13, .NET 9 (`net9.0`, `net9.0-windows10.0.19041.0`, `net9.0-maccatalyst`)
- **App Platform:** .NET MAUI Blazor Hybrid (`Microsoft.AspNetCore.Components.WebView.Maui`)
- **UI Framework:** Blazor with code-behind pattern (`.razor` and `.razor.cs`), Tailwind CSS, Syncfusion Blazor Core
- **Data & Persistence:** Entity Framework Core with SQLite (`networth.db`)
- **Testing:** xUnit, bUnit (Blazor component testing framework), Moq/NSubstitute

---

## 3. Project Structure & Responsibilities

### `NetWorthTracker.Models`
Contains all core data models, DTOs (Data Transfer Objects), enums, and exceptions.
- `Features/Account/`: Account models, `AccountCreateDTO`, `AccountResponseDTO`, etc.
- `Features/MonthlyBalance/`: Monthly balance entries and DTOs.
- `Exceptions/`: Domain-specific exceptions.

### `NetWorthTracker.Data.Api` & `NetWorthTracker.Data`
- `NetWorthTracker.Data.Api`: Data contract interfaces (e.g. repository abstractions).
- `NetWorthTracker.Data`: `AppDbContext`, EF Core migrations, SQLite database connection management, and repository implementations.

### `NetWorthTracker.Services.Api` & `NetWorthTracker.Services`
- `NetWorthTracker.Services.Api`: Service layer contracts (e.g. `IAccountService`, `IMonthlyBalanceService`).
- `NetWorthTracker.Services`: Service implementations containing domain validation and business rules.

### `NetWorthTracker.Common`
- Aggregates dependency injection setups (e.g. `AddNetWorthTrackerServices()`) linking data access and business services for consumption by the UI host or seeders.

### `NetWorthTracker.UI`
- Razor Class Library (RCL) hosting reusable UI components and pages:
  - `Feature/Account/`: Account management components (list, create, edit, details).
  - `Feature/MonthlyBalance/`: Monthly balance tracking, grids, and modal forms.
  - `Feature/Home/`: Dashboard, summary charts, net worth cards.
  - `Components/`: Shared UI widgets (e.g. `MonthYearPicker`, modal dialogs).
  - `Styles/`: Tailwind CSS and global styling assets.

### `NetWorthTracker.App`
- The executable .NET MAUI Blazor Hybrid application for Windows and MacCatalyst.
- Boots MAUI, initializes Blazor WebView, loads embedded `appsettings.json`, and hosts `Main.razor` / `Routes.razor`.

### `NetWorthTracker.Seeder`
- Console utility used to seed initial account and transaction balance data from CSV files (`accounts.csv`, `monthly_balances.csv`) into SQLite.

### `tests/NetWorthTracker.UI.Tests`
- Component and unit tests for Blazor UI components using `bUnit` and `xUnit`.

---

## 4. Coding Standards & Conventions

1. **C# & .NET Conventions:**
   - Nullable reference types are enabled (`<Nullable>enable</Nullable>`). Always handle nullable states properly.
   - Use latest C# 13 features where appropriate (e.g., file-scoped namespaces, required properties, collection expressions).
   - Warnings are treated as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`). Ensure code compiles without warnings.

2. **Blazor Component Design & UI Guidelines:**
   - Use the code-behind pattern: place markup in `ComponentName.razor` and C# logic in `ComponentName.razor.cs` (as `partial class`).
   - Use parameter validation and event callbacks for child-to-parent communication.
   - For all UI development, visual tokens, color palette, dark mode classes, and component templates, strictly follow the [UI Style Guide & Component Standards](ui-style-guide.md).
   - Keep UI components responsive and styled according to project Tailwind utilities.

3. **Data & Service Access:**
   - Always inject interfaces (`IAccountService`, `IMonthlyBalanceService`), never concrete classes.
   - Keep business logic inside `Services`, leaving UI components focused purely on presentation and user interaction.
   - Use asynchronous programming (`async`/`await`) for all I/O and database operations.

---

## 5. Building and Testing

- **Build Solution:**
  ```powershell
  dotnet build NetWorthTracker.sln
  ```
- **Run Tests:**
  ```powershell
  dotnet test tests\NetWorthTracker.UI.Tests\NetWorthTracker.UI.Tests.csproj
  ```
- **Run Seeder (Optional):**
  ```powershell
  dotnet run --project src\NetWorthTracker.Seeder\NetWorthTracker.Seeder.csproj
  ```
