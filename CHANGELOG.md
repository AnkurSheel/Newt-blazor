# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0.0] - 2026-11-28

### Changed

- The Accounts page now shows account status for the current month and no longer displays the dashboard's month selector or summary cards.

### Fixed

- Account creation now explains when the closing month is not after the opening month.
- Adding a monthly balance now shows an error when the account is inactive or a balance already exists for that month.
- Improved the precision of large values in net worth trend charts.

## [0.2.0.0] - 2026-11-25

### Changed

- Seperated monthly checkin into its own tab.

## [0.1.2.0] - 2026-11-24

### Fixed

- Adding an account with a duplicate name shows an error message.

## [0.1.1.0] - 2026-10-28

### Fixed
- Syncfusion license key error shows on all pages.

### Changed
- Updated `actions/cache` GitHub Actions dependency from v4 to v6 across CI workflows.
- Updated Rider IDE project settings.

## [0.1.0.0] - 2026-10-27

### Added
- **Net Worth Dashboard:** Real-time calculation and overview of total net worth, total assets, and total liabilities.
- **Account Management:** Create, edit, and organize accounts across various asset and liability categories (cash, savings, investments, crypto, loans, and credit cards).
- **Monthly Balance History:** Track monthly balance records per account with streamlined entry, editing, and historical review.
- **Interactive Visualizations & Analytics:**
  - Historical net worth growth trends over time.
  - Monthly change comparisons and breakdown charts.
  - Asset allocation and portfolio distribution views.
- **Offline & Private Data Storage:** Local-first SQLite database keeping all financial data privately stored on your device.
- **Modern Desktop Experience:** Native Windows application with dark mode support and responsive UI.
