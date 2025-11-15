# Design Document - Journal Entry Management

## Overview

Journal Entry Management adalah modul untuk mencatat transaksi akuntansi menggunakan sistem double-entry bookkeeping. Sistem ini memastikan setiap transaksi balance (total debit = total credit) dan terintegrasi dengan Chart of Accounts.

### Key Features
- Double-entry bookkeeping dengan validasi balance
- Multi-line journal entries
- Status workflow: Draft → Posted → Reversed
- Auto-generate journal numbers
- Integration dengan Chart of Accounts
- Export to Excel
- Audit trail lengkap

## Architecture

### Database Schema

#### JournalEntry Table (Header)
```
- JournalId (Guid, PK)
- JournalNumber (string, unique, indexed) - Format: JE-YYYYMMDD-XXXX
- JournalDate (DateTime, indexed)
- Description (string, 500 chars)
- Status (string) - Draft, Posted, Reversed
- TotalDebit (decimal)
- TotalCredit (decimal)
- PostedDate (DateTime, nullable)
- PostedBy (Guid, nullable, FK to User)
- ReversalJournalId (Guid, nullable, FK to JournalEntry) - Link to reversal journal
- OriginalJournalId (Guid, nullable, FK to JournalEntry) - Link to original if this is reversal
- IsDeleted (bool)
- DeletedAt (DateTime, nullable)
- DeletedBy (Guid, nullable)
- CreatedAt (DateTime)
- CreatedBy (Guid, FK to User)
- UpdatedAt (DateTime, nullable)
- UpdatedBy (Guid, nullable)
```

#### JournalLine Table (Detail)
```
- JournalLineId (Guid, PK)
- JournalId (Guid, FK to JournalEntry)
- LineNumber (int) - Sequence number within journal
- AccountId (Guid, FK to ChartOfAccount)
- Description (string, 500 chars)
- DebitAmount (decimal)
- CreditAmount (decimal)
- IsDeleted (bool)
- CreatedAt (DateTime)
- CreatedBy (Guid)
```

### Entity Relationships
- JournalEntry 1:N JournalLine
- JournalEntry N:1 User (CreatedBy, UpdatedBy, PostedBy, DeletedBy)
- JournalEntry 1:1 JournalEntry (ReversalJournalId - self-reference)
- JournalLine N:1 ChartOfAccount

## Components and Interfaces

### 1. Entity Layer

**JournalEntryEntity.cs**
- Properties matching database schema
- Navigation properties: JournalLines, CreatedByUser, PostedByUser, ReversalJournal, OriginalJournal
- Computed properties: IsBalanced, CanEdit, CanDelete, CanPost, CanReverse

**JournalLineEntity.cs**
- Properties matching database schema
- Navigation properties: JournalEntry, Account
- Computed properties: Amount (returns DebitAmount or CreditAmount whichever is non-zero)

**JournalEntryEntityConfiguration.cs**
- Configure relationships
- Configure indexes on JournalNumber, JournalDate, Status
- Configure decimal precision (18,2)
- Configure cascade delete for JournalLines

### 2. Model Layer

**ViewModels:**
- `JournalEntryViewModel` - For displaying journal header with computed fields
- `JournalLineViewModel` - For displaying journal lines with account info
- `JournalEntryDetailViewModel` - Complete journal with header + lines

**Request Models:**
- `CreateJournalEntryRequest` - Contains header info + list of lines
- `UpdateJournalEntryRequest` - Same as create but includes JournalId
- `JournalLineRequest` - Single line data (AccountId, Description, DebitAmount, CreditAmount)
- `DataTableJournalEntryRequest` - Extends BaseDatatableRequest with filters (DateFrom, DateTo, Status, AccountId)
- `PostJournalRequest` - Contains JournalId and PostedDate
- `ReverseJournalRequest` - Contains JournalId and ReversalDate

### 3. Service Layer

**IJournalEntryService Interface:**

```csharp
// CRUD Operations
Task<BaseDatatableResponse> Datatable(DataTableJournalEntryRequest request);
Task<JournalEntryDetailViewModel?> GetByIdAsync(Guid id);
Task<JournalEntryDetailViewModel?> GetByNumberAsync(string journalNumber);
Task<Guid> CreateAsync(CreateJournalEntryRequest request, Guid userId);
Task UpdateAsync(UpdateJournalEntryRequest request, Guid userId);
Task DeleteAsync(Guid id, Guid userId);

// Status Operations
Task PostAsync(PostJournalRequest request, Guid userId);
Task ReverseAsync(ReverseJournalRequest request, Guid userId);

// Validation
Task<bool> IsBalancedAsync(Guid journalId);
Task<bool> CanEditAsync(Guid journalId);
Task<bool> CanDeleteAsync(Guid journalId);
Task<bool> CanPostAsync(Guid journalId);
Task<bool> CanReverseAsync(Guid journalId);
Task<List<string>> ValidateJournalAsync(CreateJournalEntryRequest request);

// Utility
Task<string> GenerateJournalNumberAsync(DateTime journalDate);
Task<List<JournalEntryViewModel>> GetByAccountAsync(Guid accountId);
Task<List<JournalEntryViewModel>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo);

// Export
Task<byte[]> ExportToExcelAsync(DateTime? dateFrom, DateTime? dateTo, string? status, Guid? accountId);
```

**JournalEntryService Implementation:**

Key business logic:
1. **CreateAsync**: 
   - Generate journal number
   - Validate balance (total debit = total credit)
   - Validate accounts (must be detail accounts, must be active)
   - Set status to Draft
   - Save header and lines in transaction

2. **UpdateAsync**:
   - Check status is Draft
   - Validate balance
   - Update header and lines
   - Delete removed lines, add new lines, update existing lines

3. **PostAsync**:
   - Validate status is Draft
   - Validate balance
   - Validate all accounts still exist and active
   - Set status to Posted
   - Set PostedDate and PostedBy
   - Cannot be edited or deleted after posting

4. **ReverseAsync**:
   - Validate status is Posted
   - Create new journal entry with reversed amounts (debit ↔ credit)
   - Link original and reversal journals
   - Auto-post reversal journal
   - Set original journal status to Reversed

5. **GenerateJournalNumberAsync**:
   - Format: JE-YYYYMMDD-XXXX
   - Get last number for the date
   - Increment sequence
   - Ensure uniqueness

### 4. Controller Layer

**JournalEntryController.cs**

Endpoints:
- `GET /JournalEntry/Index` - Main view
- `POST /JournalEntry/Datatable` - Get paginated list
- `GET /JournalEntry/GetById/{id}` - Get journal detail
- `GET /JournalEntry/GetByNumber/{number}` - Get by journal number
- `POST /JournalEntry/Create` - Create new journal
- `POST /JournalEntry/Edit` - Update journal
- `DELETE /JournalEntry/Delete/{id}` - Delete journal
- `POST /JournalEntry/Post` - Post journal
- `POST /JournalEntry/Reverse` - Reverse journal
- `GET /JournalEntry/GenerateNumber` - Generate next journal number
- `GET /JournalEntry/GetAccountDropdown` - Get active detail accounts for dropdown
- `GET /JournalEntry/ExportExcel` - Export to Excel

### 5. View Layer

**Index.cshtml**

Layout:
```
┌─────────────────────────────────────────────────────────┐
│ Journal Entry Management                    [Add] [Export]│
├─────────────────────────────────────────────────────────┤
│ Filters:                                                 │
│ [Date From] [Date To] [Status ▼] [Account ▼] [Apply]   │
├─────────────────────────────────────────────────────────┤
│ DataTable:                                               │
│ No | Journal# | Date | Description | Amount | Status | Actions│
│ 1  | JE-...   | ...  | ...         | 1,000  | Posted | [👁][✏][🗑]│
└─────────────────────────────────────────────────────────┘
```

**Create/Edit Modal**

Layout:
```
┌─────────────────────────────────────────────────────────┐
│ Add/Edit Journal Entry                          [X]      │
├─────────────────────────────────────────────────────────┤
│ Journal Number: [JE-20251115-0001] (auto)              │
│ Journal Date:   [📅 15/11/2025]                         │
│ Description:    [_____________________________]         │
│                                                          │
│ Journal Lines:                                  [+ Add Line]│
│ ┌────────────────────────────────────────────────────┐ │
│ │ Account | Description | Debit | Credit | [Actions] │ │
│ │ Cash    | Payment     | 1,000 | 0      | [🗑]      │ │
│ │ Revenue | Sales       | 0     | 1,000  | [🗑]      │ │
│ └────────────────────────────────────────────────────┘ │
│                                                          │
│ Total:                    1,000   1,000                 │
│ Balance: ✅ Balanced                                     │
│                                                          │
│ [Cancel] [Save as Draft]                                │
└─────────────────────────────────────────────────────────┘
```

**Detail Modal**

Shows read-only view with:
- Header information
- All journal lines in table
- Total debit/credit
- Status badge
- Audit information
- Action buttons based on status (Post, Reverse)

### 6. JavaScript Layer

**wwwroot/custom/features/journalentry/index.js**

Key functions:
- `initializeDataTable()` - Setup DataTable with server-side processing
- `initializeEventHandlers()` - Setup all event listeners
- `openModal(mode, journalId)` - Open create/edit modal
- `addJournalLine()` - Add new line to journal
- `removeJournalLine(index)` - Remove line from journal
- `calculateTotals()` - Calculate total debit/credit and check balance
- `validateJournal()` - Client-side validation
- `saveJournal()` - Submit create/update
- `postJournal(id)` - Post journal entry
- `reverseJournal(id)` - Reverse journal entry
- `viewDetail(id)` - Show detail modal
- `exportToExcel()` - Trigger export
- `loadAccountDropdown()` - Load active detail accounts

## Data Models

### JournalEntryViewModel
```csharp
{
    JournalId: Guid,
    JournalNumber: string,
    JournalDate: DateTime,
    Description: string,
    Status: string,
    TotalDebit: decimal,
    TotalCredit: decimal,
    IsBalanced: bool,
    PostedDate: DateTime?,
    PostedBy: string?,
    ReversalJournalNumber: string?,
    OriginalJournalNumber: string?,
    CreatedBy: string,
    CreatedAt: DateTime,
    UpdatedBy: string?,
    UpdatedAt: DateTime?,
    CanEdit: bool,
    CanDelete: bool,
    CanPost: bool,
    CanReverse: bool
}
```

### JournalLineViewModel
```csharp
{
    JournalLineId: Guid,
    LineNumber: int,
    AccountId: Guid,
    AccountCode: string,
    AccountName: string,
    Description: string,
    DebitAmount: decimal,
    CreditAmount: decimal
}
```

## Error Handling

### Validation Errors
- Journal date cannot be in future
- Must have at least 2 lines
- Total debit must equal total credit
- All amounts must be positive
- Accounts must be detail accounts (not header)
- Accounts must be active
- Cannot have empty account selection

### Business Rule Errors
- Cannot edit Posted or Reversed journals
- Cannot delete Posted journals
- Cannot post if not balanced
- Cannot reverse Draft journals
- Cannot reverse already Reversed journals

### Error Messages
- User-friendly messages for all validation errors
- Clear indication of which line has error
- Highlight invalid fields in red
- Show balance difference if not balanced

## Testing Strategy

### Unit Tests
- Service layer validation logic
- Journal number generation
- Balance calculation
- Status transition rules

### Integration Tests
- Create journal with multiple lines
- Post journal and verify status change
- Reverse journal and verify new journal created
- Delete journal and verify soft delete

### UI Tests
- Add/remove journal lines dynamically
- Real-time balance calculation
- Filter and search functionality
- Export to Excel

## Performance Considerations

1. **Database Indexes**
   - Index on JournalNumber for quick lookup
   - Index on JournalDate for date range queries
   - Index on Status for filtering
   - Composite index on (JournalDate, Status) for common queries

2. **Query Optimization**
   - Use Include() for eager loading JournalLines and Account
   - Pagination for large datasets
   - Async operations for all database calls

3. **Caching**
   - Cache active detail accounts dropdown (refresh on account changes)
   - Cache journal number sequence per day

4. **Transaction Management**
   - Use database transactions for create/update operations
   - Rollback on any error during multi-line save

## Security Considerations

1. **Authorization**
   - Only authorized users can create/edit journals
   - Only authorized users can post journals
   - Only authorized users can reverse journals
   - Audit trail for all operations

2. **Data Validation**
   - Server-side validation for all inputs
   - Prevent SQL injection with parameterized queries
   - Validate decimal precision and range

3. **Soft Delete**
   - Never hard delete journals
   - Maintain audit trail even for deleted records

## Integration Points

### Chart of Accounts
- Validate account exists and is active
- Validate account is detail account (not header)
- Display account code and name in dropdowns and tables

### General Ledger (Future)
- Posted journals will update general ledger balances
- Reversal journals will reverse general ledger entries

### Financial Reports (Future)
- Trial Balance will use posted journal data
- Income Statement will aggregate revenue/expense accounts
- Balance Sheet will aggregate asset/liability/equity accounts

## Migration Strategy

1. Create JournalEntry and JournalLine tables
2. Add foreign keys and indexes
3. Seed initial data (optional - sample journals for testing)
4. Add menu item for Journal Entry

## Future Enhancements

1. **Recurring Journals** - Auto-create journals on schedule
2. **Journal Templates** - Save common journal patterns
3. **Attachments** - Upload supporting documents
4. **Approval Workflow** - Multi-level approval before posting
5. **Batch Posting** - Post multiple journals at once
6. **Import from Excel** - Bulk create journals
7. **Journal Copying** - Duplicate existing journal
8. **Advanced Search** - Search by amount range, account combination
