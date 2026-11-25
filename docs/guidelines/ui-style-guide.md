# NetWorthTracker - UI Style Guide & Component Standards

This guide defines the visual design system, UI architecture, styling conventions, and component patterns for the **NetWorthTracker** application (`NetWorthTracker.UI`). All AI agents and developers implementing or modifying UI components must adhere to these standards.

---

## 1. Design System & Theming Overview

- **Aesthetic:** Modern, dark-first, clean financial dashboard with high-contrast data visualization.
- **CSS Framework:** Tailwind CSS v4 (configured in `NetWorthTracker.UI/node/app.tailwind.css`).
- **Component Library:** Syncfusion Blazor (`Syncfusion.Blazor.Themes/tailwind3.css` + `e-dark-mode`).
- **Platform:** .NET MAUI Blazor Hybrid (desktop Windows/macOS and mobile responsiveness).

---

## 2. Color Palette & Semantic Tokens

Use the role-based utilities below in components. The tokens are defined in
`NetWorthTracker.UI/node/app.tailwind.css`; keep the underlying Tailwind shade
mapping there so components describe intent rather than palette internals.

### Layout, Text, and Borders
| Token utility | Maps to | Usage |
| :--- | :--- | :--- |
| `bg-background-page` | `slate-900` | Main page, modal surface, date pills |
| `bg-background-panel` | `slate-950` | Sidebar, cards, grids, and inputs |
| `bg-background-overlay` | `slate-950` | Full-screen modal backdrop |
| `bg-background-interactive` / `bg-background-hover` | `slate-800` / `slate-700` | Selected and hovered controls |
| `text-foreground` / `text-foreground-secondary` / `text-foreground-tertiary` | `slate-100` / `slate-200` / `slate-300` | Primary, secondary, and tertiary text |
| `text-foreground-muted` / `text-foreground-disabled` / `text-foreground-faint` | `slate-400` / `slate-500` / `slate-600` | Supporting, disabled, and faint text |
| `text-foreground-inverse` | `white` | Text on dark or branded controls |
| `border-outline` / `border-outline-strong` / `border-outline-muted` | `slate-800` / `slate-700` / `slate-600` | Default, emphasized, and subtle borders |

### Brand and Financial States
| Token utility | Maps to | Usage |
| :--- | :--- | :--- |
| `text-brand-accent`, `bg-brand-primary`, `hover:bg-brand-primary-hover`, `active:bg-brand-primary-active` | `indigo-400`, `indigo-600`, `indigo-500`, `indigo-700` | Brand text and primary actions |
| `text-financial-positive`, `bg-financial-positive-background`, `border-financial-positive-outline` | `emerald-400`, `emerald-950`, `emerald-800` | Positive values and account-asset badges |
| `text-account-liability`, `bg-account-liability-background`, `border-account-liability-outline` | `rose-400`, `rose-950`, `rose-800` | Liability values and account badges |
| `text-feedback-error`, `bg-feedback-error-background`, `border-feedback-error-outline` | `rose-400`, `rose-950`, `rose-800` | Validation and component errors |
| `text-trend-negative` | `red-400` | Negative change indicators; kept distinct from rose liability/error colors |

### Summary and Chart Colors
| Token utility / variable | Maps to | Usage |
| :--- | :--- | :--- |
| `text-metric-asset`, `bg-metric-asset-background`, `border-metric-asset-outline` | `teal-400`, `teal-800`, `teal-500` | Asset summary card and trend legend |
| `text-metric-liability`, `bg-metric-liability-background`, `border-metric-liability-outline` | `rose-400`, `rose-800`, `rose-500` | Liability summary card |
| `text-metric-net-worth`, `bg-metric-net-worth-background`, `border-metric-net-worth-outline` | `violet-400`, `violet-800`, `violet-500` | Net-worth summary card |
| `--color-chart-axis` / `--color-chart-grid` | `slate-700` / `slate-800` | Syncfusion axis and grid lines |
| `--color-chart-area-outline` / `--color-chart-area-fill` | Brand indigo / 15% indigo fill | Cumulative-area series |
| `--color-chart-positive` / `--color-chart-negative` | `emerald-500` / `rose-500` | Positive and negative chart points |

The summary-card colors intentionally differ from account badges: assets use
teal in summary cards and emerald in account lists; net worth uses violet in
summary cards and indigo for general brand actions.

---

## 3. Typography & Formatting Rules

- **Font Families:**
  - `font-sans`: Default for UI text, labels, headers, and controls.
  - `font-mono`: Required for all currency numbers, monetary amounts, balance inputs, and date pills.

- **Type Scale & Hierarchy:**
  - **Page Header:** `text-lg sm:text-xl font-semibold tracking-wide text-foreground-inverse truncate`
  - **Section / Modal Header:** `text-lg font-semibold text-foreground-inverse`
  - **Card Metric Value:** `text-2xl sm:text-3xl font-mono font-bold {CssColor}`
  - **Form / Card Label:** `text-xs font-semibold text-foreground-muted uppercase tracking-wider`
  - **Body / Table Cell:** `text-sm text-foreground-secondary`
  - **Pills / Badges / Micro Text:** `text-xs font-semibold` or `text-[10px] font-medium`

- **Currency & Date Formatting:**
  - Currency: `amount.ToString("C2")` in `font-mono`
  - Date Display: `date.ToString("MMM yyyy")` or `date.ToString("MMMM yyyy")`

---

## 4. UI Architecture & Folder Structure

UI components in `NetWorthTracker.UI` follow feature-driven organization and the Blazor code-behind pattern:

```
NetWorthTracker.UI/
├── Common/
│   ├── Layout/
│   │   ├── MainLayout.razor        # Root layout with sidebar and main content wrapper
│   │   └── NavMenu.razor           # Fixed sidebar navigation
│   └── Routes.razor                # App routing definition
├── Components/                     # Shared, domain-agnostic UI widgets
│   ├── EnumDropDownList.razor      # Generic enum dropdown bound to Syncfusion
│   ├── EnumDropDownList.razor.cs
│   ├── FinancialCardSummary.razor  # Individual metric summary card
│   ├── FinancialCardSummary.razor.cs
│   ├── FinancialSummaryContainer.razor # 3-card summary grid
│   ├── FinancialSummaryContainer.razor.cs
│   ├── MonthYearPicker.razor       # Syncfusion Month/Year date picker wrapper
│   ├── MonthYearPicker.razor.cs
│   └── Icons/                      # Dedicated SVG icon components
│       ├── AccountIcon.razor
│       ├── ChevronLeftIcon.razor
│       ├── ChevronRightIcon.razor
│       ├── CopyIcon.razor
│       └── DashboardIcon.razor
├── Feature/                        # Domain-specific feature modules
│   ├── Account/
│   │   ├── AccountList.razor       # Account management & data grid page
│   │   ├── AccountList.razor.cs
│   │   ├── AddAccountModal.razor   # Account creation dialog
│   │   └── AddAccountModal.razor.cs
│   ├── Dashboard/
│   │   ├── Dashboard.razor         # Main financial dashboard
│   │   └── Dashboard.razor.cs
│   └── MonthlyBalance/
│       ├── AddMonthlyBalanceModal.razor # Transaction / balance entry modal
│       └── AddMonthlyBalanceModal.razor.cs
└── Styles/                         # Global CSS & Tailwind build outputs
```

### Component Guidelines:
1. **Code-Behind Required:** Keep `.razor` for markup and `.razor.cs` for component logic (`partial class`).
2. **Feature Isolation:** Place feature-specific modals, pages, and subcomponents inside `Feature/{FeatureName}/`.
3. **Reusable Widgets:** Place shared, reusable components without business logic inside `Components/`.
4. **Service Injection:** Inject service contracts (`IAccountService`, `IFinancialSummaryService`), never direct DbContext or repositories.
5. **SVG Icons as Dedicated Components:** Every SVG icon must be its own reusable component in the `Components/Icons/` folder rather than inlined directly in pages, modals, or layout markup.

---

## 5. Standard Component Patterns & Templates

### 5.1 Page Layout Template
Every page should follow the sticky header + main scrollable content structure:

```razor
<div class="min-h-screen w-full flex flex-col bg-background-page text-foreground font-sans">
    <!-- Top Bar Header -->
    <header class="h-16 border-b border-outline px-4 sm:px-8 flex items-center justify-between bg-background-page/80 backdrop-blur sticky top-0 z-10">
        <h1 class="text-lg sm:text-xl font-semibold tracking-wide text-foreground-inverse truncate">Page Title</h1>
        
        <div class="flex items-center gap-3">
            <!-- Header controls (e.g. MonthYearPicker, Action buttons) -->
        </div>
    </header>

    <!-- Main Content Area -->
    <main class="flex-1 p-4 sm:p-8 overflow-x-hidden space-y-6">
        <!-- Page content, cards, grids -->
    </main>
</div>
```

---

### 5.2 Metric Summary Cards
Metric cards display key financial figures in a 3-column responsive layout:

```razor
<div class="p-5 rounded-xl border border-outline bg-background-panel/60 shadow-lg flex flex-col justify-between">
    <div class="flex items-center justify-between">
        <span class="text-xs font-medium text-foreground-muted uppercase tracking-wider">Total Assets</span>
        <span class="px-2 py-0.5 rounded-md text-xs font-semibold text-metric-asset bg-metric-asset-background/60 border border-metric-asset-outline/40">
            ▲ Asset
        </span>
    </div>
    <div class="mt-3">
        <span class="text-2xl sm:text-3xl font-mono font-bold text-metric-asset">
            @Amount.ToString("C2")
        </span>
    </div>
</div>
```

---

### 5.3 Segmented Filter Bar
For tabbed or status filtering:

```razor
<div class="inline-flex p-1 bg-background-panel rounded-lg border border-outline text-xs font-medium w-full sm:w-auto justify-stretch">
    <button @onclick='() => _filter = "Active"'
            class="flex-1 sm:flex-initial px-3 py-1.5 rounded-md transition text-center @(_filter == "Active" ? "bg-background-interactive text-foreground-inverse" : "text-foreground-muted hover:text-foreground-secondary")">
        Active
    </button>
    <button @onclick='() => _filter = "Closed"'
            class="flex-1 sm:flex-initial px-3 py-1.5 rounded-md transition text-center @(_filter == "Closed" ? "bg-background-interactive text-foreground-inverse" : "text-foreground-muted hover:text-foreground-secondary")">
        Closed
    </button>
</div>
```

---

### 5.4 Data Grids (Syncfusion SfGrid)
Tables should be wrapped inside a responsive card container with dark styling:

```razor
<div class="w-full overflow-x-auto rounded-xl border border-outline shadow-2xl bg-background-panel">
    <SfGrid DataSource="@Items" TValue="AccountResponseDTO" AllowPaging="true" AllowSorting="true" Width="100%">
        <GridPageSettings PageSize="10" PageSizes="true" />
        <GridColumns>
            <GridColumn Field="@nameof(AccountResponseDTO.Name)" HeaderText="Account Name" Width="30%">
                <Template>
                    @{
                        var item = (AccountResponseDTO)context;
                        <span class="truncate font-medium text-foreground-secondary">@item.Name</span>
                    }
                </Template>
            </GridColumn>
            <!-- Formatted columns -->
        </GridColumns>
    </SfGrid>
</div>
```

---

### 5.5 Modals & Dialogs
Modals must use either `SfDialog` with `CssClass="bg-background-page border border-outline rounded-xl shadow-2xl overflow-hidden p-0"` or standard backdrop overlays:

```razor
<div class="fixed inset-0 z-50 flex items-center justify-center bg-background-panel/80 backdrop-blur-sm p-4">
    <div class="w-full max-w-md rounded-xl border border-outline bg-background-page p-6 shadow-2xl space-y-5">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-outline pb-3">
            <h3 class="text-lg font-semibold text-foreground-inverse">Modal Title</h3>
            <button @onclick="CloseModal" class="text-foreground-muted hover:text-foreground-inverse transition">✕</button>
        </div>

        <!-- Body / Inputs -->
        <div class="space-y-4">
            <div>
                <label class="block text-xs font-semibold text-foreground-muted uppercase tracking-wider mb-2">Field Label</label>
                <input type="text" class="w-full bg-background-panel border border-outline text-foreground rounded-lg px-3 py-2 text-sm focus:outline-none focus:border-brand-primary-hover" />
            </div>
        </div>

        <!-- Actions -->
        <div class="flex items-center justify-end gap-3 pt-4 border-t border-outline">
            <button @onclick="CloseModal" class="px-4 py-2 text-sm font-medium text-foreground-muted hover:text-foreground-inverse transition">Cancel</button>
            <button @onclick="Save" class="px-5 py-2 text-sm font-medium bg-brand-primary hover:bg-brand-primary-hover text-foreground-inverse rounded-lg shadow transition">Save</button>
        </div>
    </div>
</div>
```

---

### 5.6 Buttons & Interactive Elements

- **Primary Action Button:**
  ```html
  <button class="px-4 py-2 bg-brand-primary hover:bg-brand-primary-hover text-foreground-inverse font-medium text-sm rounded-lg shadow transition flex items-center gap-1.5">
      <span>+ Add Account</span>
  </button>
  ```

- **Secondary / Ghost Button (Cancel):**
  ```html
  <button class="px-4 py-2 text-sm font-medium text-foreground-muted hover:text-foreground-inverse transition">
      Cancel
  </button>
  ```

- **Table Row Action Button:**
  ```html
  <button class="px-2.5 py-1 text-xs font-medium text-brand-accent bg-brand-background/60 border border-brand-outline/50 hover:bg-brand-background-hover/80 rounded-md transition inline-flex items-center gap-1">
      <span>+ Txn</span>
  </button>
  ```

- **Active Pulse Indicator:**
  ```html
  <span class="inline-flex items-center gap-1.5 text-xs text-financial-positive font-medium">
      <span class="h-1.5 w-1.5 rounded-full bg-financial-positive animate-pulse"></span> Active
  </span>
  ```

---

### 5.7 SVG Icon Components

Never embed raw `<svg>` tags inline within pages, layouts, or feature components. Instead:
- Create dedicated icon components in `NetWorthTracker.UI/Components/Icons/`.
- Name icon components in PascalCase ending with `Icon` (e.g. `AccountIcon.razor`, `ChevronLeftIcon.razor`, `ChevronRightIcon.razor`, `CopyIcon.razor`, `DashboardIcon.razor`).
- Standardize size and stroke classes (such as `h-4 w-4 shrink-0`, `fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"`).
- Consume via `@using NetWorthTracker.UI.Components.Icons` (included in `_Imports.razor`):
  ```razor
  <button type="button" aria-label="Previous month" class="...">
      <ChevronLeftIcon />
  </button>
  ```

---

## 6. Checklist for AI Agents & Developers

When creating or modifying UI components:
- [ ] Uses `.razor` for markup and `.razor.cs` for logic.
- [ ] Uses role-based theme utilities (`bg-background-page`, `bg-background-panel`, `border-outline`) instead of raw palette classes.
- [ ] Financial metrics use `font-mono`, `C2` formatting, and the documented `text-metric-asset`, `text-metric-liability`, or `text-metric-net-worth` tokens.
- [ ] Dates use `MonthYearPicker` with standard `MMM yyyy` or `MMMM yyyy` formatting.
- [ ] SVGs are encapsulated as dedicated components in the `Components/Icons/` folder (never inlined in templates).
- [ ] Input labels follow uppercase tracking (`text-xs font-semibold text-foreground-muted uppercase tracking-wider`).
- [ ] Buttons follow brand-primary, foreground-muted ghost, or brand-background table-action styles.
- [ ] Responsive design implemented with `sm:` breakpoints for desktop and mobile layouts.
- [ ] Razor markup and code-behind class strings are scanned by Tailwind CLI (`@source` includes both `.razor` and `.razor.cs`).
