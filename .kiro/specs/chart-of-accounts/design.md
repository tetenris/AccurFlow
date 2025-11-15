# Chart of Accounts Management - Design Document

## Overview

The Chart of Accounts (COA) Management module is the foundational component of the AccuFlow accounting system. It provides a comprehensive interface for creating, organizing, and managing the hierarchical structure of accounts used to record all financial transactions. This design follows a three-tier architecture pattern with clear separation between presentation, business logic, and data access layers.

## Architecture

### System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Index View   │  │ Create Modal │  │ Edit Modal   │     │
│  │ (List/Tree)  │  │              │  │              │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│         │                  │                  │             │
│         └──────────────────┴──────────────────┘             │
│                            │                                │
└────────────────────────────┼────────────────────────────────┘
                             │
┌────────────────────────────┼────────────────────────────────┐
│                     Controller Layer                         │
│              ┌──────────────────────────┐                   │
│              │ ChartOfAccountController │                   │
│              └──────────────────────────┘                   │
│                            │                                │
└────────────────────────────┼────────────────────────────────┘
                             │
┌────────────────────────────┼────────────────────────────────┐
│                     Business Logic Layer                     │
│         ┌──────────────────────────────────┐               │
│         │ ChartOfAccountService            │               │
│         │ - CRUD Operations                │               │
│         │ - Validation Logic               │               │
│         │ - Business Rules                 │               │
│         │ - Hierarchy Management           │               │
│         └──────────────────────────────────┘               │
│                            │                                │
└────────────────────────────┼────────────────────────────────┘
                             │
┌────────────────────────────┼────────────────────────────────┐
│                     Data Access Layer                        │
│              ┌──────────────────────────┐                   │
│              │ AppDbContext             │                   │
│              │ ChartOfAccountEntity     │                   │
│              └──────────────────────────┘                   │
│                            │                                │
└────────────────────────────┼────────────────────────────────┘
                             │
                      ┌──────┴──────┐
                      │   Database   │
                      │  SQL Server  │
                      └──────────────┘
```

## Data Models

### Entity Model

**ChartOfAccountEntity** (Already exists, needs enhancement)

```csharp
public class ChartOfAccountEntity : BaseEntity, IEntity
{
    // Primary Key
    public Guid AccountId { get; set; } = Guid.NewGuid();
    
    // Account Information
    public string AccountCode { get; set; } = string.Empty;  // e.g., "1-10000"
    public string AccountName { get; set; } = string.Empty;  // e.g., "Cash in Bank"
    public string AccountType { get; set; } = string.Empty;  // Asset, Liability, Equity, Revenue, Expense
    public string Description { get; set; } = string.Empty;
    
    // Hierarchy
    public Guid? ParentAccountId { get; set; }
    public ChartOfAccountEntity? ParentAccount { get; set; }
    public ICollection<ChartOfAccountEntity> ChildAccounts { get; set; } = new List<ChartOfAccountEntity>();
    
    // Account Properties
    public bool IsHeader { get; set; } = false;  // Header accounts cannot have transactions
    public bool IsActive { get; set; } = true;
    public decimal OpeningBalance { get; set; } = 0;
    public string NormalBalance { get; set; } = "Debit";  // Debit or Credit
    
    // Additional Properties
    public string Currency { get; set; } = "IDR";
    public int Level { get; set; } = 0;  // Hierarchy level (0 = root)
    
    // Inherited from BaseEntity:
    // DateTime CreatedAt
    // string CreatedBy
    // DateTime? UpdatedAt
    // string? UpdatedBy
    // bool IsDeleted
    // DateTime? DeletedAt
    // string? DeletedBy
}
```

### View Models

**ChartOfAccountViewModel** (for display)

```csharp
public class ChartOfAccountViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string AccountType { get; set; }
    public string Description { get; set; }
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public bool IsHeader { get; set; }
    public bool IsActive { get; set; }
    public decimal OpeningBalance { get; set; }
    public string NormalBalance { get; set; }
    public string Currency { get; set; }
    public int Level { get; set; }
    public bool HasChildren { get; set; }
    public int ChildCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}
```

**CreateChartOfAccountRequest**

```csharp
public class CreateChartOfAccountRequest
{
    [Required]
    [StringLength(20)]
    public string AccountCode { get; set; }
    
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string AccountName { get; set; }
    
    [Required]
    public string AccountType { get; set; }  // Asset, Liability, Equity, Revenue, Expense
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public Guid? ParentAccountId { get; set; }
    
    public bool IsHeader { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public decimal OpeningBalance { get; set; } = 0;
    
    [StringLength(10)]
    public string Currency { get; set; } = "IDR";
}
```

**UpdateChartOfAccountRequest**

```csharp
public class UpdateChartOfAccountRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string AccountCode { get; set; }
    
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string AccountName { get; set; }
    
    [Required]
    public string AccountType { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public Guid? ParentAccountId { get; set; }
    
    public bool IsHeader { get; set; }
    
    public bool IsActive { get; set; }
    
    public decimal OpeningBalance { get; set; }
    
    [StringLength(10)]
    public string Currency { get; set; }
}
```

**DataTableChartOfAccountRequest**

```csharp
public class DataTableChartOfAccountRequest : BaseDatatableRequest
{
    public string? AccountType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsHeader { get; set; }
    public Guid? ParentAccountId { get; set; }
}
```

## Business Rules

### Account Code Format Rules

1. **Format Pattern by Account Type:**
   - Asset: `1-XXXXX` (e.g., 1-10000, 1-10100)
   - Liability: `2-XXXXX` (e.g., 2-10000, 2-20100)
   - Equity: `3-XXXXX` (e.g., 3-10000)
   - Revenue: `4-XXXXX` (e.g., 4-10000, 4-20100)
   - Expense: `5-XXXXX` (e.g., 5-10000, 5-30100)

2. **Sub-Account Numbering:**
   - Sub-accounts should follow parent code pattern
   - Example: Parent `1-10000` → Child `1-10100`, `1-10200`

3. **Validation Rules:**
   - Must be unique across all accounts
   - Maximum 20 characters
   - Alphanumeric and hyphens only
   - Must start with account type prefix (1-5)

### Account Type Rules

1. **Normal Balance by Type:**
   - Asset: Debit
   - Expense: Debit
   - Liability: Credit
   - Equity: Credit
   - Revenue: Credit

2. **Parent-Child Type Compatibility:**
   - Sub-accounts must have the same account type as parent
   - Cannot create Asset sub-account under Liability parent

### Hierarchy Rules

1. **Unlimited Levels:** System supports unlimited hierarchy depth
2. **Circular Reference Prevention:** Cannot set an account's descendant as its parent
3. **Header Account Rules:**
   - Accounts with children are automatically header accounts
   - Header accounts cannot have transactions posted
   - Can manually mark account as header even without children

### Deletion Rules

1. **Cannot Delete If:**
   - Account has been used in any transactions
   - Account has active sub-accounts
   - Account is referenced by other entities

2. **Soft Delete:** Use IsDeleted flag instead of physical deletion

### Opening Balance Rules

1. **Validation:**
   - Must be valid decimal number
   - Can be positive or negative
   - Should align with normal balance (optional warning)

2. **Application:**
   - Applied at fiscal year start
   - Used in balance calculations
   - Included in financial reports

## Components and Interfaces

### Controller: ChartOfAccountController

```csharp
public class ChartOfAccountController : BaseController
{
    private readonly IChartOfAccountService _chartOfAccountService;
    
    // GET: /ChartOfAccount/Index
    public IActionResult Index()
    
    // POST: /ChartOfAccount/Datatable
    [HttpPost]
    public async Task<IActionResult> Datatable([FromBody] DataTableChartOfAccountRequest request)
    
    // GET: /ChartOfAccount/GetById/{id}
    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    
    // GET: /ChartOfAccount/GetHierarchy
    [HttpGet]
    public async Task<IActionResult> GetHierarchy()
    
    // GET: /ChartOfAccount/GetParentAccounts
    [HttpGet]
    public async Task<IActionResult> GetParentAccounts(string accountType)
    
    // POST: /ChartOfAccount/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountRequest request)
    
    // POST: /ChartOfAccount/Edit
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] UpdateChartOfAccountRequest request)
    
    // DELETE: /ChartOfAccount/Delete
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] Guid id)
    
    // POST: /ChartOfAccount/ToggleStatus
    [HttpPost]
    public async Task<IActionResult> ToggleStatus([FromBody] Guid id)
    
    // GET: /ChartOfAccount/ValidateCode
    [HttpGet]
    public async Task<IActionResult> ValidateCode(string code, Guid? excludeId)
    
    // POST: /ChartOfAccount/Import
    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    
    // GET: /ChartOfAccount/Export
    [HttpGet]
    public async Task<IActionResult> Export()
}
```

### Service Interface: IChartOfAccountService

```csharp
public interface IChartOfAccountService : IBaseService
{
    // CRUD Operations
    Task<BaseDatatableResponse> Datatable(DataTableChartOfAccountRequest request);
    Task<ChartOfAccountViewModel?> GetByIdAsync(Guid id);
    Task<ChartOfAccountViewModel?> GetByCodeAsync(string code);
    Task CreateAsync(CreateChartOfAccountRequest request, Guid userId);
    Task UpdateAsync(UpdateChartOfAccountRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    
    // Hierarchy Operations
    Task<List<ChartOfAccountViewModel>> GetHierarchyAsync();
    Task<List<ChartOfAccountViewModel>> GetChildAccountsAsync(Guid parentId);
    Task<List<ChartOfAccountViewModel>> GetParentAccountsAsync(string accountType);
    Task<string> GetAccountPathAsync(Guid accountId);
    
    // Validation
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<bool> CanDeleteAsync(Guid accountId);
    Task<bool> HasTransactionsAsync(Guid accountId);
    Task<bool> HasChildrenAsync(Guid accountId);
    Task<bool> IsCircularReferenceAsync(Guid accountId, Guid? newParentId);
    
    // Status Management
    Task ToggleStatusAsync(Guid accountId, Guid userId);
    Task<List<ChartOfAccountViewModel>> GetActiveAccountsAsync();
    Task<List<ChartOfAccountViewModel>> GetDetailAccountsAsync();
    
    // Import/Export
    Task<ImportResult> ImportFromFileAsync(Stream fileStream, string fileName, Guid userId);
    Task<byte[]> ExportToExcelAsync();
    
    // Utility
    Task<string> GenerateAccountCodeAsync(Guid? parentId, string accountType);
    Task RecalculateHierarchyLevelsAsync();
}
```

## User Interface Design

### Index Page (List View)

**Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ Chart of Accounts                                  [+ New]  │
├─────────────────────────────────────────────────────────────┤
│ Filters:                                                     │
│ [Account Type ▼] [Status ▼] [Search...]        [Export]    │
├─────────────────────────────────────────────────────────────┤
│ Code      │ Account Name        │ Type    │ Status │ Action│
├───────────┼────────────────────┼─────────┼────────┼───────┤
│ 1-10000   │ ► Current Assets   │ Asset   │ Active │ [...]│
│   1-10100 │   Cash & Bank      │ Asset   │ Active │ [...]│
│   1-10200 │   Accounts Receiv. │ Asset   │ Active │ [...]│
│ 2-10000   │ ► Current Liab.    │ Liabil. │ Active │ [...]│
│   2-10100 │   Accounts Payable │ Liabil. │ Active │ [...]│
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- Tree view with expand/collapse for hierarchy
- Inline actions: Edit, Delete, Toggle Status
- Drag-and-drop for reordering (future enhancement)
- Color coding by account type
- Icons for header vs detail accounts

### Create/Edit Modal

**Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ Create Account                                         [X]  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ Account Code *        [1-10000____________]                 │
│                       Format: X-XXXXX                        │
│                                                              │
│ Account Name *        [Cash in Bank_______]                 │
│                                                              │
│ Account Type *        [Asset ▼]                             │
│                                                              │
│ Parent Account        [None ▼]                              │
│                       (Optional - for sub-accounts)          │
│                                                              │
│ Description           [________________________]            │
│                       [________________________]            │
│                                                              │
│ ☐ Header Account (Cannot post transactions)                │
│ ☑ Active                                                    │
│                                                              │
│ Opening Balance       [0.00___________] IDR                 │
│                                                              │
│                                    [Cancel]  [Save]         │
└─────────────────────────────────────────────────────────────┘
```

**Validation:**
- Real-time code format validation
- Duplicate code check
- Parent account compatibility check
- Required field indicators

## Database Schema

### Table: ChartOfAccounts

```sql
CREATE TABLE ChartOfAccounts (
    AccountId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountCode NVARCHAR(20) NOT NULL UNIQUE,
    AccountName NVARCHAR(255) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    ParentAccountId UNIQUEIDENTIFIER NULL,
    IsHeader BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    NormalBalance NVARCHAR(10) NOT NULL DEFAULT 'Debit',
    Currency NVARCHAR(10) NOT NULL DEFAULT 'IDR',
    Level INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(MAX) NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(MAX) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_ChartOfAccounts_Parent 
        FOREIGN KEY (ParentAccountId) 
        REFERENCES ChartOfAccounts(AccountId)
        ON DELETE NO ACTION,
    
    CONSTRAINT CK_AccountType 
        CHECK (AccountType IN ('Asset', 'Liability', 'Equity', 'Revenue', 'Expense')),
    
    CONSTRAINT CK_NormalBalance 
        CHECK (NormalBalance IN ('Debit', 'Credit'))
);

CREATE INDEX IX_ChartOfAccounts_AccountCode ON ChartOfAccounts(AccountCode);
CREATE INDEX IX_ChartOfAccounts_AccountType ON ChartOfAccounts(AccountType);
CREATE INDEX IX_ChartOfAccounts_ParentAccountId ON ChartOfAccounts(ParentAccountId);
CREATE INDEX IX_ChartOfAccounts_IsActive ON ChartOfAccounts(IsActive);
CREATE INDEX IX_ChartOfAccounts_IsDeleted ON ChartOfAccounts(IsDeleted);
```

## Error Handling

### Validation Errors

1. **Duplicate Account Code**
   - Message: "Account code '{code}' already exists"
   - HTTP Status: 400 Bad Request

2. **Invalid Account Code Format**
   - Message: "Account code must follow format: X-XXXXX where X is 1-5"
   - HTTP Status: 400 Bad Request

3. **Parent Account Not Found**
   - Message: "Parent account not found"
   - HTTP Status: 404 Not Found

4. **Incompatible Parent Type**
   - Message: "Sub-account type must match parent account type"
   - HTTP Status: 400 Bad Request

5. **Circular Reference**
   - Message: "Cannot set descendant account as parent"
   - HTTP Status: 400 Bad Request

### Business Rule Errors

1. **Cannot Delete Account with Transactions**
   - Message: "Cannot delete account that has been used in transactions. Consider deactivating instead."
   - HTTP Status: 400 Bad Request

2. **Cannot Delete Account with Children**
   - Message: "Cannot delete account that has sub-accounts. Delete or move sub-accounts first."
   - HTTP Status: 400 Bad Request

3. **Cannot Post to Header Account**
   - Message: "Cannot post transactions to header accounts"
   - HTTP Status: 400 Bad Request

## Testing Strategy

### Unit Tests

1. **Service Layer Tests:**
   - Account creation with valid data
   - Account creation with duplicate code (should fail)
   - Account creation with invalid parent (should fail)
   - Hierarchy level calculation
   - Circular reference detection
   - Code uniqueness validation
   - Account deletion validation

2. **Validation Tests:**
   - Account code format validation
   - Account name length validation
   - Account type validation
   - Parent-child type compatibility

### Integration Tests

1. **Controller Tests:**
   - Create account endpoint
   - Update account endpoint
   - Delete account endpoint
   - Get hierarchy endpoint
   - Datatable with filters

2. **Database Tests:**
   - Foreign key constraints
   - Unique constraints
   - Cascade behavior
   - Index performance

### UI Tests (Manual)

1. **User Workflow Tests:**
   - Create root account
   - Create sub-account
   - Edit account
   - Delete account
   - Search and filter
   - Import accounts
   - Export accounts

2. **Validation Tests:**
   - Form validation messages
   - Real-time code validation
   - Parent account dropdown filtering

## Security Considerations

1. **Authorization:**
   - Only Administrators and Accountants can manage COA
   - Role-based access control on all endpoints

2. **Input Validation:**
   - Server-side validation for all inputs
   - SQL injection prevention via parameterized queries
   - XSS prevention via input sanitization

3. **Audit Trail:**
   - Track who created/modified/deleted accounts
   - Timestamp all changes
   - Maintain soft-deleted records

4. **Data Integrity:**
   - Foreign key constraints
   - Check constraints for enums
   - Transaction support for multi-step operations

## Performance Considerations

1. **Database Indexes:**
   - Index on AccountCode for quick lookups
   - Index on ParentAccountId for hierarchy queries
   - Index on AccountType for filtering
   - Index on IsActive and IsDeleted for common filters

2. **Caching Strategy:**
   - Cache active accounts list (5 minute TTL)
   - Cache account hierarchy (10 minute TTL)
   - Invalidate cache on create/update/delete

3. **Query Optimization:**
   - Use eager loading for parent/child relationships
   - Implement pagination for large datasets
   - Use projection to select only needed fields

4. **Hierarchy Performance:**
   - Store hierarchy level for quick filtering
   - Use recursive CTE for deep hierarchy queries
   - Limit hierarchy depth display in UI (e.g., 5 levels)

## Future Enhancements

1. **Advanced Features:**
   - Account templates for quick setup
   - Bulk operations (activate/deactivate multiple)
   - Account merging
   - Historical balance tracking
   - Multi-currency support enhancement

2. **Reporting:**
   - Account usage report
   - Inactive accounts report
   - Account hierarchy diagram
   - Balance sheet preview

3. **Integration:**
   - Integration with journal entry module
   - Integration with financial reports
   - API for external systems

4. **UI Improvements:**
   - Drag-and-drop hierarchy reorganization
   - Visual hierarchy tree diagram
   - Account balance preview
   - Quick edit inline
