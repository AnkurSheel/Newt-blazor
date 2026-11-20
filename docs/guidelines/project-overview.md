# NetWorthTracker - Project Overview & Guidelines

## 1. Overview & Architecture

**NetWorthTracker** is a personal finance management application built on .NET 10 using .NET MAUI with Blazor Hybrid. It allows users to track financial accounts, historical monthly balances, net worth trends, asset/liability breakdowns, and perform financial projections.

The solution follows clean architecture principles with distinct separation of concerns across projects:

```
NetWorthTracker/
├── src/
│   ├── NetWorthTracker.App/          # MAUI Blazor Hybrid Host Application (.NET 10 MAUI)
│   ├── NetWorthTracker.UI/           # Blazor UI Components, Pages, and Styles (Tailwind CSS, Syncfusion)
│   ├── NetWorthTracker.Business/     # Feature modules, public operations, models, and persistence ports
│   ├── NetWorthTracker.Data/         # EF Core DbContext, SQLite repositories & migrations
│   ├── NetWorthTracker.Data.Api/     # Shared data infrastructure contracts
│   ├── NetWorthTracker.Common/       # Service registration & DI helper extensions
│   └── NetWorthTracker.Seeder/       # Database seeder console application for CSV imports
├── tests/
│   ├── NetWorthTracker.Architecture.Tests/ # Project and module boundary tests
│   ├── NetWorthTracker.Business.Tests/     # Business module tests
│   ├── NetWorthTracker.Common.Tests/       # Composition tests
│   ├── NetWorthTracker.Data.Tests/         # Data-layer tests
│   └── NetWorthTracker.UI.Tests/           # UI tests using bUnit and xUnit
└── docs/
    └── guidelines/                   # Shared developer and AI agent documentation
```

---

## 2. Technology Stack & Key Libraries

- **Language & Runtime:** Latest C# version supported by the .NET 10 SDK; .NET 10 (`net10.0` and platform-specific app target frameworks)
- **App Platform:** .NET MAUI Blazor Hybrid (`Microsoft.AspNetCore.Components.WebView.Maui`)
- **UI Framework:** Blazor with code-behind pattern (`.razor` and `.razor.cs`), Tailwind CSS, Syncfusion Blazor Core
- **Data & Persistence:** Entity Framework Core with SQLite (`networth.db`)
- **Testing:** xUnit, bUnit (Blazor component testing framework), Moq/NSubstitute

---

## 3. Project Structure & Responsibilities

### `NetWorthTracker.Business`
Contains the Accounts, Monthly Balances, and Financial Summary feature modules. Each module owns its public API and business operations; persistence ports live under `Features/{Accounts,MonthlyBalances,FinancialSummary}/Persistence`. Business-owned contracts, result types, and validation exceptions live in this project.

### `NetWorthTracker.Data.Api` & `NetWorthTracker.Data`
- `NetWorthTracker.Data.Api`: Shared data infrastructure contracts, including application path access.
- `NetWorthTracker.Data`: `AppDbContext`, EF Core migrations, SQLite database connection management, and adapters for the Business-owned persistence ports.

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

### Test projects
- `tests/NetWorthTracker.Architecture.Tests`: Enforces the permitted project graph and Business module boundaries.
- `tests/NetWorthTracker.Business.Tests`: Tests business module operations and calculations.
- `tests/NetWorthTracker.Common.Tests`: Verifies service composition and dependency injection.
- `tests/NetWorthTracker.Data.Tests`: Tests EF Core adapters and SQLite persistence behavior.
- `tests/NetWorthTracker.UI.Tests`: Tests Blazor UI components using `bUnit` and `xUnit`.

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
   - Inject feature public-operation interfaces, never concrete classes. All feature APIs and business logic are in Business.
   - Keep business logic in the owning feature module; Data implements its persistence ports.
   - Use asynchronous programming (`async`/`await`) for all I/O and database operations.

4. **Testing Standards:**
   - Follow the [Testing Guidelines for AI Agents](testing-guidelines.md) for test naming conventions, Arrange-Act-Assert structure, minimal setup, property-level assertions, and bUnit/xUnit practices.

---

## 5. Building and Testing

- **Build Solution:**
  ```powershell
  dotnet build NetWorthTracker.sln
  ```
- **Run Tests:**
  ```powershell
  dotnet test NetWorthTracker.Tests.slnf --configuration Release --verbosity normal
  ```
- **Run Seeder (Optional):**
  ```powershell
  dotnet run --project src\NetWorthTracker.Seeder\NetWorthTracker.Seeder.csproj
  ```
