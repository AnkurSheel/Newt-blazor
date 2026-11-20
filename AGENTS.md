<!-- bmad:context -->
<!-- Verified 2026-11-20 against 5230d63. Managed by bmad-project-context; edits inside this block are replaced on refresh. Keep anything you want preserved outside the markers. -->

## NetWorthTracker

NetWorthTracker is a personal-finance application built with .NET MAUI Blazor Hybrid. Keep detailed architecture, UI, and testing guidance in `docs/guidelines/`; use the relevant source document rather than duplicating its full rules here.

## Where things are

- Architecture, project responsibilities, and general coding standards: `docs/guidelines/project-overview.md`
- UI styling and component standards: `docs/guidelines/ui-style-guide.md`
- Testing practices: `docs/guidelines/testing-guidelines.md`
- MAUI startup and platform-specific code: `src/NetWorthTracker.App/MauiProgram.cs` and `src/NetWorthTracker.App/Platforms/`

## Running and verifying

- Use the .NET 10 SDK; shared projects target `net10.0`, and CI installs .NET `10.0.x`.
- Run CI's test selection with `dotnet test NetWorthTracker.Tests.slnf --configuration Release --verbosity normal`; it excludes the MAUI app host.

<!-- /bmad:context -->
