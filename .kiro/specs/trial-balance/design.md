# Design Document - Trial Balance

## Overview

Trial Balance adalah laporan yang menampilkan balance semua accounts untuk verify bahwa total debit = total credit. Module ini akan reuse logic dari General Ledger Service untuk calculate balances.

### Key Features
- Generate trial balance report dengan grouping by account type
- Filter by date (as of date)
- Filter by account type
- Show/hide zero balance accounts
- Export to Excel
- Drill-down to General Ledger
- Balance validation (debit = credit)

## Architecture

### Database Schema

**No New Tables Required!** Trial Balance menggunakan data dari General Ledger Service yang sudah ada.

### Data Flow

```
General Ledger Service
    ↓
Trial Balance Service (Calculate & Group)
    ↓
Trial Balance Report
```

## Components and Interfaces

### 1. Model Layer

**ViewModels:**

```csharp
// Single account line in trial balance
public class TrialBalanceLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string AccountType { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}

// Trial balance report with grouping
public class TrialBalanceViewModel
{
    public DateTime AsOfDate { get; set; }
    public List<TrialBalanceGroupViewModel> Groups { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Difference { get; set; }
    public bool IsBalanced { get; set; }
}

// Group by account type
public class TrialBalanceGroupViewModel
{
    public string AccountType { get; set; }
    public List<TrialBalanceLineViewModel> Accounts { get; set; }
    public decimal SubtotalDebit { get; set; }
    public decimal SubtotalCredit { get; set; }
}
```

**Request Models:**

```csharp
public class GetTrialBalanceRequest
{
    public DateTime? AsOfDate { get; set; }
    public string? AccountType { get; set; }
    public bool ShowZeroBalance { get; set; } = false;
}
```

### 2. Service Layer

**ITrialBalanceService Interface:**

```csharp
public interface ITrialBalanceService
{
    Task<TrialBalanceViewModel> GetTrialBalanceAsync(GetTrialBalanceRequest request);
    Task<byte[]> ExportToExcelAsync(GetTrialBalanceRequest request);
}
```

**TrialBalanceService Implementation:**

Key business logic:

1. **GetTrialBalanceAsync**:
   - Get all accounts from ChartOfAccount
   - For each account, get balance from GeneralLedgerService.GetAccountBalanceAsync()
   - Filter zero balance accounts if ShowZeroBalance = false
   - Group accounts by AccountType
   - Calculate subtotal per group
   - Calculate grand total debit and credit
   - Verify balance (debit = credit)
   - Return TrialBalanceViewModel

2. **ExportToExcelAsync**:
   - Get trial balance data
   - Create Excel with NPOI
   - Format with grouping and subtotals
   - Return byte array

### 3. Controller Layer

**TrialBalanceController.cs**

Endpoints:
- `GET /TrialBalance/Index` - Main view
- `GET /TrialBalance/GetTrialBalance` - API to get trial balance data
- `GET /TrialBalance/ExportExcel` - Export to Excel

### 4. View Layer

**Index.cshtml**

Layout:
```
┌─────────────────────────────────────────────────────────┐
│ Trial Balance                              [Export Excel]│
├─────────────────────────────────────────────────────────┤
│ Filters:                                                 │
│ [As of Date] [Account Type ▼] [☐ Show Zero Balance] [Generate]│
├─────────────────────────────────────────────────────────┤
│ As of: 16/11/2025                                       │
├─────────────────────────────────────────────────────────┤
│ Account Type: ASSET                                     │
│ Code | Account Name           | Debit    | Credit      │
│ 1000 | Cash                   | 10,000   | 0           │
│ 1100 | Accounts Receivable    | 5,000    | 0           │
│                        Subtotal: 15,000   | 0           │
├─────────────────────────────────────────────────────────┤
│ Account Type: LIABILITY                                 │
│ Code | Account Name           | Debit    | Credit      │
│ 2000 | Accounts Payable       | 0        | 8,000       │
│                        Subtotal: 0        | 8,000       │
├─────────────────────────────────────────────────────────┤
│ TOTAL:                          15,000   | 15,000      │
│ Difference: 0 ✓ BALANCED                                │
└─────────────────────────────────────────────────────────┘
```

### 5. JavaScript Layer

**wwwroot/custom/features/trialbalance/index.js**

Key functions:
- `generateTrialBalance()` - Fetch and display trial balance
- `applyFilters()` - Apply date and account type filters
- `exportToExcel()` - Trigger export
- `drillDownToLedger(accountId)` - Navigate to general ledger

## Data Models

### Balance Calculation

Trial Balance akan reuse `GeneralLedgerService.GetAccountBalanceAsync()` untuk calculate balance per account.

Balance ditampilkan di kolom Debit atau Credit tergantung account type:
- **Asset, Expense**: Jika balance positif → Debit, jika negatif → Credit
- **Liability, Equity, Revenue**: Jika balance positif → Credit, jika negatif → Debit

### Grouping Logic

```csharp
var groups = accounts
    .GroupBy(a => a.AccountType)
    .OrderBy(g => GetAccountTypeOrder(g.Key))
    .Select(g => new TrialBalanceGroupViewModel
    {
        AccountType = g.Key,
        Accounts = g.OrderBy(a => a.AccountCode).ToList(),
        SubtotalDebit = g.Sum(a => a.DebitBalance),
        SubtotalCredit = g.Sum(a => a.CreditBalance)
    })
    .ToList();
```

Account Type Order:
1. Asset
2. Liability
3. Equity
4. Revenue
5. Expense
6. Other Income
7. Other Expense

## Error Handling

### Validation Errors
- Invalid date (future date)
- No accounts found

### Business Rule Errors
- Trial balance not balanced (debit ≠ credit)

### Error Messages
- "Trial balance is not balanced. Difference: XXX"
- "No accounts found for the selected criteria"

## Testing Strategy

### Unit Tests
- Balance calculation for different account types
- Grouping logic
- Zero balance filtering
- Balance validation

### Integration Tests
- Generate trial balance with filters
- Export to Excel
- Drill-down to general ledger

### UI Tests
- Apply filters
- Show/hide zero balance
- Export functionality

## Performance Considerations

1. **Reuse General Ledger Service**
   - Don't duplicate balance calculation logic
   - Use existing GetAccountBalanceAsync method

2. **Caching**
   - Cache trial balance result for same parameters
   - Invalidate cache when new journal is posted

3. **Query Optimization**
   - Get all accounts in single query
   - Calculate balances in parallel (optional)

## Security Considerations

1. **Authorization**
   - Only authorized users can view trial balance
   - Audit trail for who viewed report

2. **Data Validation**
   - Validate date range
   - Prevent SQL injection

## Integration Points

### General Ledger Service
- Use GetAccountBalanceAsync for balance calculation
- Drill-down to account ledger detail

### Chart of Accounts
- Get all accounts
- Use account type for grouping

### Financial Reports (Future)
- Trial Balance is foundation for Income Statement and Balance Sheet

## Migration Strategy

No database changes needed - reuse existing tables and services.

## Future Enhancements

1. **Comparative Trial Balance** - Compare two periods side by side
2. **Trial Balance with Opening Balance** - Show opening, movement, closing
3. **Adjusted Trial Balance** - Include adjusting entries
4. **Trial Balance by Department** - If multi-department accounting
5. **Trial Balance Drill-down to Transactions** - Show transaction details inline
