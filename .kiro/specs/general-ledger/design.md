# Design Document - General Ledger

## Overview

General Ledger adalah modul read-only yang menampilkan history transaksi dan balance per account. Data ledger diambil langsung dari JournalEntry dan JournalLine yang sudah di-post, tanpa perlu tabel terpisah.

### Key Features
- View ledger per account dengan running balance
- Filter by date range dengan opening/closing balance
- Ledger summary untuk all accounts
- Balance calculation sesuai account type (Asset, Liability, etc.)
- Export to Excel
- Drill-down dari summary ke detail
- Audit trail integration

## Architecture

### Database Schema

**No New Tables Required!** General Ledger menggunakan data dari:
- `JournalEntry` table (Status = 'Posted')
- `JournalLine` table
- `ChartOfAccount` table

### Data Flow

```
JournalEntry (Posted) 
    ↓
JournalLine
    ↓
General Ledger View (Calculated)
    ↓
Financial Reports
```

## Components and Interfaces

### 1. Model Layer

**ViewModels:**

```csharp
// Ledger entry (single transaction line)
public class LedgerEntryViewModel
{
    public Guid JournalLineId { get; set; }
    public Guid JournalId { get; set; }
    public string JournalNumber { get; set; }
    public DateTime JournalDate { get; set; }
    public string Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
    public string PostedBy { get; set; }
    public DateTime PostedDate { get; set; }
}

// Ledger detail for single account
public class AccountLedgerViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string AccountType { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<LedgerEntryViewModel> Entries { get; set; }
}

// Ledger summary (all accounts)
public class LedgerSummaryViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string AccountType { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; }
    public int TransactionCount { get; set; }
}
```

**Request Models:**

```csharp
public class GetLedgerRequest
{
    public Guid AccountId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetLedgerSummaryRequest
{
    public string? AccountType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
}
```

### 2. Service Layer

**IGeneralLedgerService Interface:**

```csharp
public interface IGeneralLedgerService
{
    // Get ledger for single account
    Task<AccountLedgerViewModel> GetAccountLedgerAsync(GetLedgerRequest request);
    
    // Get ledger summary for all accounts
    Task<List<LedgerSummaryViewModel>> GetLedgerSummaryAsync(GetLedgerSummaryRequest request);
    
    // Get balance for account at specific date
    Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null);
    
    // Get opening balance (before date from)
    Task<decimal> GetOpeningBalanceAsync(Guid accountId, DateTime dateFrom);
    
    // Export ledger to Excel
    Task<byte[]> ExportLedgerToExcelAsync(GetLedgerRequest request);
    
    // Export summary to Excel
    Task<byte[]> ExportSummaryToExcelAsync(GetLedgerSummaryRequest request);
}
```

**GeneralLedgerService Implementation:**

Key business logic:

1. **GetAccountLedgerAsync**:
   - Query JournalLine for specific account
   - Join with JournalEntry (Status = 'Posted')
   - Filter by date range if specified
   - Calculate opening balance from transactions before dateFrom
   - Calculate running balance for each entry
   - Order by JournalDate, then JournalNumber
   - Support pagination

2. **GetLedgerSummaryAsync**:
   - Query all accounts with transactions
   - Group by account
   - Calculate total debit, credit, and balance per account
   - Filter by account type if specified
   - Filter by date range if specified
   - Support search by account code/name

3. **GetAccountBalanceAsync**:
   - Sum all debit and credit for account up to specified date
   - Apply balance formula based on account type:
     - Asset/Expense: Debit - Credit
     - Liability/Equity/Revenue/Other Income: Credit - Debit
     - Other Expense: Debit - Credit

4. **GetOpeningBalanceAsync**:
   - Calculate balance from all transactions before dateFrom
   - Use same balance formula as GetAccountBalanceAsync

### 3. Controller Layer

**GeneralLedgerController.cs**

Endpoints:
- `GET /GeneralLedger/Index` - Main view (ledger summary)
- `GET /GeneralLedger/AccountLedger/{accountId}` - View for specific account
- `GET /GeneralLedger/GetLedger` - API to get ledger data
- `GET /GeneralLedger/GetSummary` - API to get summary data
- `GET /GeneralLedger/GetBalance/{accountId}` - Get current balance
- `GET /GeneralLedger/ExportLedger` - Export account ledger to Excel
- `GET /GeneralLedger/ExportSummary` - Export summary to Excel

### 4. View Layer

**Index.cshtml (Ledger Summary)**

Layout:
```
┌─────────────────────────────────────────────────────────┐
│ General Ledger                          [Export Summary]│
├─────────────────────────────────────────────────────────┤
│ Filters:                                                 │
│ [Date From] [Date To] [Account Type ▼] [Search] [Apply]│
├─────────────────────────────────────────────────────────┤
│ DataTable:                                               │
│ Account Code | Account Name | Type | Debit | Credit | Balance│
│ 1000         | Cash         | Asset| 5,000 | 3,000  | 2,000  │
│ 4000         | Revenue      | Rev  | 0     | 10,000 | 10,000 │
└─────────────────────────────────────────────────────────┘
```

**AccountLedger.cshtml (Account Detail)**

Layout:
```
┌─────────────────────────────────────────────────────────┐
│ General Ledger - 1000 Cash                [Export]      │
├─────────────────────────────────────────────────────────┤
│ Filters:                                                 │
│ [Date From] [Date To] [Apply]                           │
├─────────────────────────────────────────────────────────┤
│ Opening Balance: 1,000                                  │
├─────────────────────────────────────────────────────────┤
│ Date       | Journal# | Description | Debit | Credit | Balance│
│ 2025-11-01 | JE-001   | Initial     | 1,000 | 0      | 1,000  │
│ 2025-11-02 | JE-002   | Payment     | 0     | 500    | 500    │
│ 2025-11-03 | JE-003   | Receipt     | 1,500 | 0      | 2,000  │
├─────────────────────────────────────────────────────────┤
│ Closing Balance: 2,000                                  │
│ Total Debit: 2,500 | Total Credit: 500                 │
└─────────────────────────────────────────────────────────┘
```

### 5. JavaScript Layer

**wwwroot/custom/features/generalledger/summary.js**

Key functions:
- `initializeSummaryTable()` - Setup DataTable for summary
- `applyFilters()` - Apply date range, account type, search filters
- `viewAccountLedger(accountId)` - Navigate to account detail
- `exportSummary()` - Export summary to Excel

**wwwroot/custom/features/generalledger/ledger.js**

Key functions:
- `initializeLedgerTable()` - Setup table for account ledger
- `loadLedgerData()` - Load ledger entries with running balance
- `applyDateFilter()` - Filter by date range
- `exportLedger()` - Export to Excel
- `viewJournalEntry(journalId)` - Navigate to journal detail

## Data Models

### Balance Calculation Formula

```csharp
public decimal CalculateBalance(string accountType, decimal totalDebit, decimal totalCredit)
{
    return accountType switch
    {
        "Asset" => totalDebit - totalCredit,
        "Expense" => totalDebit - totalCredit,
        "Other Expense" => totalDebit - totalCredit,
        "Liability" => totalCredit - totalDebit,
        "Equity" => totalCredit - totalDebit,
        "Revenue" => totalCredit - totalDebit,
        "Other Income" => totalCredit - totalDebit,
        _ => 0
    };
}
```

### Running Balance Calculation

```csharp
// Calculate running balance for each entry
decimal runningBalance = openingBalance;
foreach (var entry in entries)
{
    if (accountType is "Asset" or "Expense" or "Other Expense")
    {
        runningBalance += entry.DebitAmount - entry.CreditAmount;
    }
    else
    {
        runningBalance += entry.CreditAmount - entry.DebitAmount;
    }
    entry.RunningBalance = runningBalance;
}
```

## Error Handling

### Validation Errors
- Account not found
- Invalid date range (dateFrom > dateTo)
- No transactions found for account

### Error Messages
- "Account not found"
- "Invalid date range"
- "No transactions found for the selected period"

## Testing Strategy

### Unit Tests
- Balance calculation for different account types
- Opening balance calculation
- Running balance calculation
- Date range filtering

### Integration Tests
- Get ledger for account with transactions
- Get ledger summary for all accounts
- Filter by date range
- Export to Excel

### UI Tests
- View ledger summary
- Drill-down to account detail
- Apply filters
- Export functionality

## Performance Considerations

1. **Database Indexes**
   - Existing indexes on JournalEntry (JournalDate, Status)
   - Existing indexes on JournalLine (AccountId, JournalId)
   - Composite index on (AccountId, JournalDate) for faster ledger queries

2. **Query Optimization**
   - Use Include() for eager loading JournalEntry and Account
   - Filter by Status = 'Posted' early in query
   - Use pagination for large result sets
   - Calculate running balance in memory (not in database)

3. **Caching**
   - Cache account balance for current date (refresh on new journal post)
   - Cache ledger summary (refresh on new journal post)

4. **Transaction Management**
   - Read-only queries, no transactions needed

## Security Considerations

1. **Authorization**
   - Only authorized users can view general ledger
   - Audit trail for who viewed which ledger

2. **Data Validation**
   - Validate account exists
   - Validate date range
   - Prevent SQL injection with parameterized queries

## Integration Points

### Journal Entry
- Read Posted journal entries
- Display journal number as link to journal detail
- Show who posted and when

### Chart of Accounts
- Read account information (code, name, type)
- Use account type for balance calculation
- Link to account detail from summary

### Financial Reports (Future)
- Trial Balance will use ledger summary data
- Income Statement will use Revenue/Expense ledger data
- Balance Sheet will use Asset/Liability/Equity ledger data

## Migration Strategy

1. No new tables needed
2. Add composite index on JournalLine (AccountId, JournalDate) for performance
3. Add menu item for General Ledger

## Future Enhancements

1. **Ledger Comparison** - Compare two periods side by side
2. **Budget vs Actual** - Show budget and variance
3. **Ledger Notes** - Add notes to specific ledger entries
4. **Ledger Reconciliation** - Mark entries as reconciled
5. **Multi-Currency** - Support foreign currency transactions
6. **Ledger Charts** - Visualize balance trends over time
