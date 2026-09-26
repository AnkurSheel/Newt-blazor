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

### Background Colors
| Token / Class | Usage |
| :--- | :--- |
| `bg-slate-900` | Base page background, dialog and modal container backgrounds |
| `bg-slate-950` | Sidebar background, card containers, data grids, text input backgrounds |
| `bg-slate-900/80 backdrop-blur` | Sticky top navigation and header bars |
| `bg-slate-950/80 backdrop-blur-sm` | Full-screen modal backdrops |
| `bg-slate-800` | Active segmented filters, dropdown hover items |
| `bg-slate-800/60` / `bg-slate-800/90` | Nav link hover and active background states |

### Border Colors
| Token / Class | Usage |
| :--- | :--- |
| `border-slate-800` | Standard card, table, header, and input borders |
| `border-slate-800/80` / `border-slate-800/50` | Dividers, subtle grid separators |
| `border-slate-700/50` | Active nav link border, focused element borders |

### Typography & Foreground Colors
| Token / Class | Usage |
| :--- | :--- |
| `text-white` | Primary headings, brand titles, primary button text |
| `text-slate-100` / `text-slate-200` | Body text, table values, input text, dialog titles |
| `text-slate-300` / `text-slate-400` | Subtitles, secondary labels, unselected filter tabs, nav icons |
| `text-slate-500` / `text-slate-600` | Muted captions, closed/inactive entity indicators, strikethrough text |

### Semantic Financial & Accent Colors
| Category | Text Color | Badge / Pill Background & Border | Usage |
| :--- | :--- | :--- | :--- |
| **Primary Accent** | `text-indigo-400` | `bg-indigo-950/60 border border-indigo-800/50` | Brand logo, net worth card, active nav links, primary action buttons (`bg-indigo-600 hover:bg-indigo-500`) |
| **Assets / Positive** | `text-emerald-400` | `bg-emerald-950/60 border border-emerald-800/50` | Total Assets card, Asset type badges, positive growth, active account indicator dot |
| **Liabilities / Debt** | `text-rose-400` | `bg-rose-950/60 border border-rose-800/50` | Total Liabilities card, Liability type badges, debt amounts |
| **Closed / Inactive** | `text-slate-500` | `bg-slate-900 border border-slate-800` | Closed accounts, past status tags |

---

## 3. Typography & Formatting Rules

- **Font Families:**
  - `font-sans`: Default for UI text, labels, headers, and controls.
  - `font-mono`: Required for all currency numbers, monetary amounts, balance inputs, and date pills.

- **Type Scale & Hierarchy:**
  - **Page Header:** `text-lg sm:text-xl font-semibold tracking-wide text-white truncate`
  - **Section / Modal Header:** `text-lg font-semibold text-white`
  - **Card Metric Value:** `text-2xl sm:text-3xl font-mono font-bold {CssColor}`
  - **Form / Card Label:** `text-xs font-semibold text-slate-400 uppercase tracking-wider`
  - **Body / Table Cell:** `text-sm text-slate-200`
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
│   └── MonthYearPicker.razor.cs
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

---

## 5. Standard Component Patterns & Templates

### 5.1 Page Layout Template
Every page should follow the sticky header + main scrollable content structure:

```razor
<div class="min-h-screen w-full flex flex-col bg-slate-900 text-slate-100 font-sans">
    <!-- Top Bar Header -->
    <header class="h-16 border-b border-slate-800 px-4 sm:px-8 flex items-center justify-between bg-slate-900/80 backdrop-blur sticky top-0 z-10">
        <h1 class="text-lg sm:text-xl font-semibold tracking-wide text-white truncate">Page Title</h1>
        
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
<div class="p-5 rounded-xl border border-slate-800 bg-slate-950/60 shadow-lg flex flex-col justify-between">
    <div class="flex items-center justify-between">
        <span class="text-xs font-medium text-slate-400 uppercase tracking-wider">Total Assets</span>
        <span class="px-2 py-0.5 rounded-md text-xs font-semibold text-emerald-400 bg-emerald-950/60 border border-emerald-800/40">
            ▲ Asset
        </span>
    </div>
    <div class="mt-3">
        <span class="text-2xl sm:text-3xl font-mono font-bold text-emerald-400">
            @Amount.ToString("C2")
        </span>
    </div>
</div>
```

---

### 5.3 Segmented Filter Bar
For tabbed or status filtering:

```razor
<div class="inline-flex p-1 bg-slate-950 rounded-lg border border-slate-800 text-xs font-medium w-full sm:w-auto justify-stretch">
    <button @onclick='() => _filter = "Active"'
            class="flex-1 sm:flex-initial px-3 py-1.5 rounded-md transition text-center @(_filter == "Active" ? "bg-slate-800 text-white" : "text-slate-400 hover:text-slate-200")">
        Active
    </button>
    <button @onclick='() => _filter = "Closed"'
            class="flex-1 sm:flex-initial px-3 py-1.5 rounded-md transition text-center @(_filter == "Closed" ? "bg-slate-800 text-white" : "text-slate-400 hover:text-slate-200")">
        Closed
    </button>
</div>
```

---

### 5.4 Data Grids (Syncfusion SfGrid)
Tables should be wrapped inside a responsive card container with dark styling:

```razor
<div class="w-full overflow-x-auto rounded-xl border border-slate-800 shadow-2xl bg-slate-950">
    <SfGrid DataSource="@Items" TValue="AccountResponseDTO" AllowPaging="true" AllowSorting="true" Width="100%">
        <GridPageSettings PageSize="10" PageSizes="true" />
        <GridColumns>
            <GridColumn Field="@nameof(AccountResponseDTO.Name)" HeaderText="Account Name" Width="30%">
                <Template>
                    @{
                        var item = (AccountResponseDTO)context;
                        <span class="truncate font-medium text-slate-200">@item.Name</span>
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
Modals must use either `SfDialog` with `CssClass="bg-slate-900 border border-slate-800 rounded-xl shadow-2xl overflow-hidden p-0"` or standard backdrop overlays:

```razor
<div class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4">
    <div class="w-full max-w-md rounded-xl border border-slate-800 bg-slate-900 p-6 shadow-2xl space-y-5">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-slate-800 pb-3">
            <h3 class="text-lg font-semibold text-white">Modal Title</h3>
            <button @onclick="CloseModal" class="text-slate-400 hover:text-white transition">✕</button>
        </div>

        <!-- Body / Inputs -->
        <div class="space-y-4">
            <div>
                <label class="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">Field Label</label>
                <input type="text" class="w-full bg-slate-950 border border-slate-800 text-slate-100 rounded-lg px-3 py-2 text-sm focus:outline-none focus:border-indigo-500" />
            </div>
        </div>

        <!-- Actions -->
        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-800">
            <button @onclick="CloseModal" class="px-4 py-2 text-sm font-medium text-slate-400 hover:text-white transition">Cancel</button>
            <button @onclick="Save" class="px-5 py-2 text-sm font-medium bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg shadow transition">Save</button>
        </div>
    </div>
</div>
```

---

### 5.6 Buttons & Interactive Elements

- **Primary Action Button:**
  ```html
  <button class="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm rounded-lg shadow transition flex items-center gap-1.5">
      <span>+ Add Account</span>
  </button>
  ```

- **Secondary / Ghost Button (Cancel):**
  ```html
  <button class="px-4 py-2 text-sm font-medium text-slate-400 hover:text-white transition">
      Cancel
  </button>
  ```

- **Table Row Action Button:**
  ```html
  <button class="px-2.5 py-1 text-xs font-medium text-indigo-400 bg-indigo-950/60 border border-indigo-800/50 hover:bg-indigo-900/80 rounded-md transition inline-flex items-center gap-1">
      <span>+ Txn</span>
  </button>
  ```

- **Active Pulse Indicator:**
  ```html
  <span class="inline-flex items-center gap-1.5 text-xs text-emerald-400 font-medium">
      <span class="h-1.5 w-1.5 rounded-full bg-emerald-400 animate-pulse"></span> Active
  </span>
  ```

---

## 6. Checklist for AI Agents & Developers

When creating or modifying UI components:
- [ ] Uses `.razor` for markup and `.razor.cs` for logic.
- [ ] Follows Tailwind dark-theme palette (`bg-slate-900`, `bg-slate-950`, `border-slate-800`).
- [ ] Financial metrics use `font-mono`, `C2` formatting, and semantic colors (`emerald-400` for assets, `rose-400` for liabilities, `indigo-400` for net worth).
- [ ] Dates use `MonthYearPicker` with standard `MMM yyyy` or `MMMM yyyy` formatting.
- [ ] Input labels follow uppercase tracking (`text-xs font-semibold text-slate-400 uppercase tracking-wider`).
- [ ] Buttons follow primary (`indigo-600`), ghost (`text-slate-400 hover:text-white`), or table action (`indigo-950/60`) styles.
- [ ] Responsive design implemented with `sm:` breakpoints for desktop and mobile layouts.
- [ ] All new markup is scanned by Tailwind CLI (referenced via `@source "../**/*.razor"`).
