# Design Document - Financial Statements

## Overview

The Financial Statements feature provides comprehensive financial reporting capabilities including Income Statement, Balance Sheet, and Cash Flow Statement. This feature will reuse the existing GeneralLedgerService for balance calculations to ensure consistency across all reports. The implementation follows the established patterns used in General Ledger and Trial Balance modules.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Presentation Layer                   │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐│
│  │ Income Statement │  │  Balance Sheet   │  │ Cash Flow  ││
│  │      View        │  │      View        │  │    View    ││
│  └──────────────────┘  └──────────────────┘  └────────────┘│
│           │                     │                    │       │
│           └─────────────────────┴────────────────────┘       │
│                              │                               │
└──────────────────────────────┼───────────────────────────────┘
                               │
┌──────────────────────────────┼───────────────────────────────┐
│                         Controller Layer                      │
│                  FinancialStatementController                 │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐│
│  │  IncomeStatement │  │  BalanceSheet    │  │  CashFlow  ││
│  │     Actions      │  │     Actions      │  │   Actions  ││
│  └──────────────────┘  └──────────────────┘  └────────────┘│
└──────────────────────────────┼───────────────────────────────┘
                               │
┌──────────────────────────────┼───────────────────────────────┐
│                         Service Layer                         │
│              FinancialStatementService (extends BaseService)  │
│  ┌──────────────────────────────────────────────────────────┐│
│  │  • GetIncomeStatementAsync()                             ││
│  │  • GetBalanceSheetAsync()                                ││
│  │  • GetCashFlowStatementAsync()                           ││
│  │  • ExportIncomeStatementAsync()                          ││
│  │  • ExportBalanceSheetAsync()                             ││
│  │  • ExportCashFlowAsync()                                 ││
│  └──────────────────────────────────────────────────────────┘│
│                              │                                │
│                   Uses GeneralLedgerService                   │
│                   for balance calculations                    │
└───────────────────────────────────────────────────────────────┘
```

### Component Interaction Flow

1. **User Request** → View (Index.cshtml)
2. **View** → JavaScript (index.js) handles UI interactions
3. **JavaScript** → AJAX call to Controller
4. **Controller** → FinancialStatementService
5. **Service** → GeneralLedgerService (for balance calculations)
6. **Service** → Database (ChartOfAccount, JournalEntry, JournalLine)
7. **Service** → Returns ViewModel
8. **Controller** → Returns JSON/Excel
9. **JavaScript** → Renders data in View

## Components and Interfaces

### 1. Models (View Models and Request Models)

#### IncomeStatementViewModel
```csharp
public class IncomeStatementViewModel
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<IncomeStatementSectionViewModel> Sections { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetIncome { get; set; }
}

public class IncomeStatementSectionViewModel
{
    public string SectionName { get; set; } // Revenue, Other Income, Expense, Other Expense
    public List<IncomeStatementLineViewModel> Lines { get; set; }
    public decimal Subtotal { get; set; }
}

public class IncomeStatementLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public decimal Amount { get; set; }
}

public class GetIncomeStatementRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool ShowZeroBalance { get; set; } = false;
}
```

#### BalanceSheetViewModel
```csharp
public class BalanceSheetViewModel
{
    public DateTime AsOfDate { get; set; }
    public List<BalanceSheetSectionViewModel> Sections { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public bool IsBalanced { get; set; }
    public decimal Difference { get; set; }
}

public class BalanceSheetSectionViewModel
{
    public string SectionName { get; set; } // Asset, Liability, Equity
    public List<BalanceSheetLineViewModel> Lines { get; set; }
    public decimal Subtotal { get; set; }
}

public class BalanceSheetLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public decimal Amount { get; set; }
}

public class GetBalanceSheetRequest
{
    public DateTime AsOfDate { get; set; }
    public bool ShowZeroBalance { get; set; } = false;
}
```

#### CashFlowStatementViewModel
```csharp
public class CashFlowStatementViewModel
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal BeginningCashBalance { get; set; }
    public List<CashFlowSectionViewModel> Sections { get; set; }
    public decimal NetCashFromOperating { get; set; }
    public decimal NetCashFromInvesting { get; set; }
    public decimal NetCashFromFinancing { get; set; }
    public decimal NetIncreaseDecrease { get; set; }
    public decimal EndingCashBalance { get; set; }
}

public class CashFlowSectionViewModel
{
    public string SectionName { get; set; } // Operating, Investing, Financing
    public List<CashFlowLineViewModel> Lines { get; set; }
    public decimal Subtotal { get; set; }
}

public class CashFlowLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public decimal Amount { get; set; }
}

public class GetCashFlowStatementRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}
```

### 2. Service Layer

#### IFinancialStatementService Interface
```csharp
public interface IFinancialStatementService : IBaseService
{
    Task<IncomeStatementViewModel> GetIncomeStatementAsync(GetIncomeStatementRequest request);
    Task<BalanceSheetViewModel> GetBalanceSheetAsync(GetBalanceSheetRequest request);
    Task<CashFlowStatementViewModel> GetCashFlowStatementAsync(GetCashFlowStatementRequest request);
    Task<byte[]> ExportIncomeStatementAsync(GetIncomeStatementRequest request);
    Task<byte[]> ExportBalanceSheetAsync(GetBalanceSheetRequest request);
    Task<byte[]> ExportCashFlowAsync(GetCashFlowStatementRequest request);
}
```

#### FinancialStatementService Implementation
```csharp
public class FinancialStatementService : BaseService, IFinancialStatementService
{
    private readonly IGeneralLedgerService _generalLedgerService;
    
    public FinancialStatementService(
        AppDbContext dbContext,
        IGeneralLedgerService generalLedgerService) : base(dbContext)
    {
        _generalLedgerService = generalLedgerService;
    }
    
    // Implementation methods...
}
```

**Key Methods:**

1. **GetIncomeStatementAsync()**
   - Get all Revenue and Other Income accounts
   - Get all Expense and Other Expense accounts
   - For each account, get balance using GeneralLedgerService
   - Group by account type (Revenue, Other Income, Expense, Other Expense)
   - Calculate subtotals and net income
   - Filter zero balance if requested

2. **GetBalanceSheetAsync()**
   - Get all Asset, Liability, and Equity accounts
   - For each account, get balance using GeneralLedgerService
   - Group by account type
   - Calculate subtotals
   - Verify Assets = Liabilities + Equity
   - Filter zero balance if requested

3. **GetCashFlowStatementAsync()**
   - Get beginning cash balance (balance before DateFrom)
   - Get all cash account transactions in period
   - Categorize by activity type (Operating, Investing, Financing)
   - Calculate net change per category
   - Calculate ending cash balance
   - Verify beginning + net change = ending

4. **Export Methods**
   - Use NPOI library (already installed)
   - Generate Excel with proper formatting
   - Include headers, subtotals, and grand totals
   - Return byte array for download

### 3. Controller Layer

#### FinancialStatementController
```csharp
public class FinancialStatementController : BaseController
{
    private readonly IFinancialStatementService _financialStatementService;
    
    public FinancialStatementController(
        IFinancialStatementService financialStatementService) : base(financialStatementService)
    {
        _financialStatementService = financialStatementService;
    }
    
    // Action methods for each report type
}
```

**Actions:**

1. **Index()** - Main view with tabs for each report
2. **GetIncomeStatement([FromQuery] GetIncomeStatementRequest)** - API endpoint
3. **GetBalanceSheet([FromQuery] GetBalanceSheetRequest)** - API endpoint
4. **GetCashFlow([FromQuery] GetCashFlowStatementRequest)** - API endpoint
5. **ExportIncomeStatement([FromQuery] GetIncomeStatementRequest)** - Excel export
6. **ExportBalanceSheet([FromQuery] GetBalanceSheetRequest)** - Excel export
7. **ExportCashFlow([FromQuery] GetCashFlowStatementRequest)** - Excel export

### 4. View Layer

#### Views/FinancialStatement/Index.cshtml

**Structure:**
```html
<div class="card">
    <div class="card-header">
        <h3>Financial Statements</h3>
    </div>
    <div class="card-body">
        <!-- Tab Navigation -->
        <ul class="nav nav-tabs">
            <li><a href="#income-statement">Income Statement</a></li>
            <li><a href="#balance-sheet">Balance Sheet</a></li>
            <li><a href="#cash-flow">Cash Flow</a></li>
        </ul>
        
        <!-- Tab Content -->
        <div class="tab-content">
            <!-- Income Statement Tab -->
            <div id="income-statement" class="tab-pane">
                <!-- Filters -->
                <div class="filter-section">
                    <input type="date" id="is-date-from" />
                    <input type="date" id="is-date-to" />
                    <input type="checkbox" id="is-show-zero" />
                    <button id="btn-generate-is">Generate</button>
                    <button id="btn-export-is">Export Excel</button>
                </div>
                <!-- Report Table -->
                <div id="income-statement-table"></div>
            </div>
            
            <!-- Balance Sheet Tab -->
            <div id="balance-sheet" class="tab-pane">
                <!-- Similar structure -->
            </div>
            
            <!-- Cash Flow Tab -->
            <div id="cash-flow" class="tab-pane">
                <!-- Similar structure -->
            </div>
        </div>
    </div>
</div>
```

**Features:**
- Bootstrap tabs for navigation between reports
- Filter section for each report type
- Generate button to fetch data
- Export button for Excel download
- Responsive table layout
- Drill-down links to General Ledger

### 5. JavaScript Layer

#### wwwroot/custom/features/financialstatement/index.js

**Structure:**
```javascript
$(document).ready(function() {
    // Initialize tabs
    initializeTabs();
    
    // Income Statement handlers
    $('#btn-generate-is').on('click', generateIncomeStatement);
    $('#btn-export-is').on('click', exportIncomeStatement);
    
    // Balance Sheet handlers
    $('#btn-generate-bs').on('click', generateBalanceSheet);
    $('#btn-export-bs').on('click', exportBalanceSheet);
    
    // Cash Flow handlers
    $('#btn-generate-cf').on('click', generateCashFlow);
    $('#btn-export-cf').on('click', exportCashFlow);
});

function generateIncomeStatement() {
    // Get filter values
    // AJAX call to /FinancialStatement/GetIncomeStatement
    // Render table with sections and subtotals
}

function generateBalanceSheet() {
    // Similar pattern
}

function generateCashFlow() {
    // Similar pattern
}

function exportIncomeStatement() {
    // Build query string
    // Trigger download via window.location
}

// Helper functions for rendering tables
function renderIncomeStatementTable(data) { }
function renderBalanceSheetTable(data) { }
function renderCashFlowTable(data) { }
```

**Key Functions:**

1. **Tab Management** - Handle tab switching
2. **Data Fetching** - AJAX calls to controller
3. **Table Rendering** - Dynamic HTML generation
4. **Export Handling** - Trigger file download
5. **Drill-down** - Navigate to General Ledger
6. **Error Handling** - Display user-friendly messages

## Data Models

### Database Tables Used

1. **ChartOfAccountEntity** - Account master data
2. **JournalEntryEntity** - Journal header
3. **JournalLineEntity** - Journal details

### Data Flow

```
ChartOfAccount (All accounts)
    ↓
Filter by AccountType
    ↓
For each account:
    ↓
GeneralLedgerService.GetAccountBalanceAsync()
    ↓
JournalLineEntity (Status = Posted)
    ↓
Calculate balance
    ↓
Group by AccountType
    ↓
Calculate subtotals
    ↓
Return ViewModel
```

## Error Handling

### Service Layer
- Validate date ranges (DateFrom <= DateTo)
- Handle empty result sets gracefully
- Log errors for debugging
- Return meaningful error messages

### Controller Layer
- Try-catch blocks around service calls
- Return appropriate HTTP status codes
- Return JSON error responses

### JavaScript Layer
- Display loading indicators
- Show error messages in UI
- Handle network failures
- Validate user input before API calls

## Testing Strategy

### Unit Tests (Optional)
- Test balance calculation logic
- Test grouping and subtotal calculations
- Test Excel export generation
- Test validation rules

### Integration Tests (Optional)
- Test end-to-end report generation
- Test with various date ranges
- Test with empty data
- Test export functionality

### Manual Testing
- Generate reports with real data
- Verify calculations against Trial Balance
- Test all filter combinations
- Test Excel export format
- Test drill-down navigation
- Test responsive layout

## Performance Considerations

1. **Reuse GeneralLedgerService** - Avoid duplicate calculation logic
2. **Efficient Queries** - Use existing indexes on JournalEntry and JournalLine
3. **Caching** - Consider caching account list (rarely changes)
4. **Pagination** - Not needed for financial statements (typically < 100 accounts)
5. **Async Operations** - All database calls are async

## Security Considerations

1. **Authorization** - Use [Authorize] attribute on controller
2. **Input Validation** - Validate all request parameters
3. **SQL Injection** - Use EF Core parameterized queries
4. **XSS Prevention** - Encode all output in views

## Deployment Considerations

1. **Database Changes** - No schema changes required
2. **Menu Integration** - Add Financial Statements to MenuSeed
3. **Service Registration** - Register IFinancialStatementService in DI
4. **Static Files** - Deploy JavaScript file to wwwroot
5. **Testing** - Test with production-like data volume

## Future Enhancements

1. **Comparative Reports** - Show multiple periods side-by-side
2. **Variance Analysis** - Calculate period-over-period changes
3. **Budget vs Actual** - Compare with budget data
4. **Graphical Reports** - Add charts and visualizations
5. **PDF Export** - Export to PDF format
6. **Scheduled Reports** - Email reports automatically
7. **Custom Grouping** - Allow user-defined account groupings
