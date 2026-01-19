# Design Document - Customer Management

## Overview

The Customer Management module provides a complete solution for managing customer data in the AccuFlow accounting system. The design follows the established patterns from Chart of Accounts and other modules, implementing a three-tier architecture with Entity Framework Core for data access, service layer for business logic, and ASP.NET Core MVC for presentation.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Index View   │  │  Controller  │  │  JavaScript      │  │
│  │ (Razor)      │  │  (API)       │  │  (DataTable)     │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Service Layer                           │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         CustomerService (Business Logic)             │   │
│  │  - CRUD Operations                                   │   │
│  │  - Validation                                        │   │
│  │  - Code Generation                                   │   │
│  │  - Export                                            │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                       Data Layer                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Entity Framework Core + SQL Server                  │   │
│  │  - CustomerEntity                                    │   │
│  │  - CustomerEntityConfiguration                       │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### 1. Data Layer

#### CustomerEntity
```csharp
public class CustomerEntity : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string CustomerType { get; set; } // Individual, Corporate, Government
    
    // Contact Information
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    
    // Address Information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Financial Information
    public decimal CreditLimit { get; set; }
    public int PaymentTerms { get; set; } // in days
    public decimal CurrentBalance { get; set; }
    
    // Tax Information
    public string? TaxId { get; set; }
    
    // Additional Information
    public string? Notes { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    
    // Audit fields inherited from BaseEntity:
    // - CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, IsDeleted
}
```

#### CustomerEntityConfiguration
- Configure primary key (CustomerId)
- Configure unique index on CustomerCode
- Configure required fields and max lengths
- Configure decimal precision for CreditLimit and CurrentBalance
- Configure default values (IsActive = true, CreditLimit = 0, PaymentTerms = 30)
- Configure indexes for performance (CustomerCode, CustomerName, IsActive)

### 2. Service Layer

#### ICustomerService Interface
```csharp
public interface ICustomerService : IBaseService
{
    // CRUD Operations
    Task<CustomerViewModel> GetByIdAsync(Guid customerId);
    Task<CustomerViewModel> GetByCodeAsync(string customerCode);
    Task CreateAsync(CreateCustomerRequest request, Guid userId);
    Task UpdateAsync(UpdateCustomerRequest request, Guid userId);
    Task DeleteAsync(Guid customerId, Guid userId);
    
    // DataTable
    Task<DataTableResponse<CustomerViewModel>> Datatable(DataTableCustomerRequest request);
    
    // Status Management
    Task ToggleStatusAsync(Guid customerId, Guid userId);
    Task<List<CustomerDropdownViewModel>> GetActiveCustomersAsync();
    
    // Validation
    Task<bool> IsCodeUniqueAsync(string customerCode, Guid? excludeId = null);
    Task<bool> CanDeleteAsync(Guid customerId);
    Task<bool> HasTransactionsAsync(Guid customerId);
    
    // Utility
    Task<string> GenerateCustomerCodeAsync();
    Task<byte[]> ExportToExcelAsync(string? customerType, bool? isActive);
}
```

#### CustomerService Implementation

**CRUD Operations:**
- CreateAsync: Validate uniqueness, create entity, save to database
- UpdateAsync: Validate existence, validate uniqueness, update entity
- DeleteAsync: Validate no transactions, soft delete
- GetByIdAsync: Retrieve by ID with all details
- GetByCodeAsync: Retrieve by customer code

**Validation Methods:**
- IsCodeUniqueAsync: Check if customer code exists (excluding current customer in edit mode)
- CanDeleteAsync: Check if customer can be deleted (no transactions, not deleted)
- HasTransactionsAsync: Check if customer has any invoices or transactions

**Status Management:**
- ToggleStatusAsync: Switch between active/inactive
- GetActiveCustomersAsync: Return list of active customers for dropdowns

**Utility Methods:**
- GenerateCustomerCodeAsync: Generate next sequential code (CUST-00001, CUST-00002, etc.)
- ExportToExcelAsync: Export filtered customer list to Excel using NPOI

**DataTable Method:**
- Support server-side processing
- Implement search across multiple fields (code, name, contact, phone, email)
- Support filtering by customer type and status
- Support sorting and pagination

### 3. Controller Layer

#### CustomerController
```csharp
[Authorize]
public class CustomerController : BaseController
{
    private readonly ICustomerService _customerService;
    
    // Actions:
    // - Index() : GET - Main view
    // - Datatable(DataTableCustomerRequest) : POST - DataTable data
    // - GetById(Guid) : GET - Customer details
    // - GetActiveCustomers() : GET - Dropdown list
    // - Create(CreateCustomerRequest) : POST - Create customer
    // - Edit(UpdateCustomerRequest) : POST - Update customer
    // - Delete(Guid) : DELETE - Delete customer
    // - ToggleStatus(Guid) : POST - Toggle active status
    // - ValidateCode(string, Guid?) : GET - Validate code uniqueness
    // - GenerateCode() : GET - Generate customer code
    // - ExportExcel(string?, bool?) : GET - Export to Excel
}
```

### 4. Presentation Layer

#### Views
- **Index.cshtml**: Main customer list view with DataTable, filters, and modals

#### Modals
- **Add/Edit Modal**: Form for creating and editing customers
- **Detail Modal**: Read-only view of customer details

#### JavaScript (index.js)
- DataTable initialization with server-side processing
- Filter functionality (customer type, status, search)
- CRUD operations (Create, Edit, Delete)
- Toggle status functionality
- Code validation and generation
- Export to Excel
- Form validation

## Data Models

### View Models

#### CustomerViewModel
```csharp
public class CustomerViewModel
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string CustomerType { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTerms { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### CustomerDropdownViewModel
```csharp
public class CustomerDropdownViewModel
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string DisplayText => $"{CustomerCode} - {CustomerName}";
}
```

### Request Models

#### CreateCustomerRequest
```csharp
public class CreateCustomerRequest
{
    [Required, MaxLength(20)]
    public string CustomerCode { get; set; }
    
    [Required, MaxLength(255)]
    public string CustomerName { get; set; }
    
    [Required]
    public string CustomerType { get; set; }
    
    [MaxLength(255)]
    public string? ContactPerson { get; set; }
    
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }
    
    [MaxLength(255)]
    public string? Website { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal CreditLimit { get; set; } = 0;
    
    [Range(1, 365)]
    public int PaymentTerms { get; set; } = 30;
    
    [MaxLength(50)]
    public string? TaxId { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
}
```

#### UpdateCustomerRequest
```csharp
public class UpdateCustomerRequest : CreateCustomerRequest
{
    [Required]
    public Guid CustomerId { get; set; }
}
```

#### DataTableCustomerRequest
```csharp
public class DataTableCustomerRequest : BaseDatatableRequest
{
    public string? CustomerType { get; set; }
    public bool? IsActive { get; set; }
}
```

## Error Handling

### Validation Errors
- Return 400 Bad Request with error message
- Client-side validation before submission
- Server-side validation for all inputs

### Business Rule Violations
- Duplicate customer code: "Customer code already exists"
- Delete with transactions: "Cannot delete customer with existing transactions"
- Invalid customer type: "Invalid customer type"

### System Errors
- Database errors: Log and return generic error message
- Export errors: Return error message to user

## Testing Strategy

### Unit Tests
- Service layer validation methods
- Code generation logic
- Business rule enforcement

### Integration Tests
- CRUD operations end-to-end
- DataTable filtering and pagination
- Export functionality

### UI Tests
- Form validation
- Modal interactions
- DataTable operations

## Performance Considerations

### Database Indexes
- Unique index on CustomerCode
- Index on CustomerName for search
- Index on IsActive for filtering
- Composite index on (IsActive, IsDeleted) for active customer queries

### Caching
- Cache active customer dropdown list (5 minutes)
- Invalidate cache on customer create/update/delete

### Query Optimization
- Use projection for DataTable (select only needed fields)
- Implement pagination at database level
- Use AsNoTracking for read-only queries

## Security

### Authentication
- All endpoints require authentication
- Use [Authorize] attribute on controller

### Authorization
- Check permissions: view, add, edit, delete
- Implement permission checks in controller actions

### Data Protection
- Sanitize user inputs
- Use parameterized queries (EF Core handles this)
- Validate all inputs server-side

## Migration Strategy

### Database Migration
1. Create CustomerEntity and configuration
2. Generate EF Core migration
3. Review migration script
4. Apply migration to database

### Seed Data
- Create sample customers for testing
- Include different customer types
- Set up in development environment only

## Integration Points

### Journal Entry Module
- Customer dropdown in journal entry forms
- Link journal entries to customers

### Invoice Module (Future)
- Customer selection in invoice creation
- Customer balance tracking
- Credit limit validation

### Reports Module (Future)
- Customer transaction history
- Customer aging report
- Customer balance report
