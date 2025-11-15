# Implementation Plan - Chart of Accounts Management

## Overview

This implementation plan breaks down the Chart of Accounts Management feature into discrete, manageable coding tasks. Each task builds incrementally on previous tasks, ensuring a systematic approach to development. The plan follows a bottom-up approach: data layer → business logic → API → UI.

---

## Tasks

- [x] 1. Enhance ChartOfAccountEntity and create database migration


  - Update `ChartOfAccountEntity` class with all required properties (IsHeader, OpeningBalance, NormalBalance, Currency, Level)
  - Create entity configuration class `ChartOfAccountEntityConfiguration` with proper constraints and indexes
  - Generate and apply EF Core migration to update database schema
  - _Requirements: 1.1, 1.4, 2.1, 3.1, 9.1, 10.1_

- [x] 2. Create view models and request/response DTOs


  - [x] 2.1 Create `ChartOfAccountViewModel` for displaying account data


    - Include all account properties plus computed fields (HasChildren, ChildCount, ParentAccountName)
    - _Requirements: 5.5_
  
  - [x] 2.2 Create `CreateChartOfAccountRequest` with validation attributes


    - Add data annotations for required fields, string lengths, and format validation
    - _Requirements: 1.1, 1.2, 12.1, 12.2, 12.3_
  
  - [x] 2.3 Create `UpdateChartOfAccountRequest` with validation attributes


    - Similar to create request but includes AccountId
    - _Requirements: 6.1, 6.2_
  
  - [x] 2.4 Create `DataTableChartOfAccountRequest` for filtering and pagination


    - Extend `BaseDatatableRequest` with COA-specific filters (AccountType, IsActive, IsHeader, ParentAccountId)
    - _Requirements: 5.2, 5.3, 5.4_

- [x] 3. Implement ChartOfAccountService with core business logic


  - [x] 3.1 Create `IChartOfAccountService` interface

    - Define all service methods (CRUD, hierarchy, validation, import/export)
    - _Requirements: All_
  
  - [x] 3.2 Implement `ChartOfAccountService` class with CRUD operations

    - Implement `CreateAsync` method with validation (code uniqueness, parent validation, type compatibility)
    - Implement `UpdateAsync` method with validation (prevent circular reference, maintain hierarchy)
    - Implement `DeleteAsync` method with business rules (check transactions, check children)
    - Implement `GetByIdAsync` and `GetByCodeAsync` methods
    - _Requirements: 1.1, 1.2, 1.3, 1.5, 6.1, 6.2, 6.3, 6.4, 8.1, 8.2, 8.3, 8.4_
  
  - [x] 3.3 Implement hierarchy management methods

    - Implement `GetHierarchyAsync` to return full account tree structure
    - Implement `GetChildAccountsAsync` to get sub-accounts of a parent
    - Implement `GetParentAccountsAsync` filtered by account type
    - Implement `GetAccountPathAsync` to get full hierarchy path (e.g., "Assets > Current Assets > Cash")
    - Implement `RecalculateHierarchyLevelsAsync` to update Level field for all accounts
    - _Requirements: 2.1, 2.2, 2.3, 2.4_
  
  - [x] 3.4 Implement validation methods

    - Implement `IsCodeUniqueAsync` to check account code uniqueness
    - Implement `CanDeleteAsync` to validate deletion rules
    - Implement `HasTransactionsAsync` to check if account is used in transactions
    - Implement `HasChildrenAsync` to check if account has sub-accounts
    - Implement `IsCircularReferenceAsync` to prevent circular parent-child relationships
    - _Requirements: 1.3, 4.2, 6.4, 8.2, 8.3_
  
  - [x] 3.5 Implement status management methods

    - Implement `ToggleStatusAsync` to activate/deactivate accounts
    - Implement `GetActiveAccountsAsync` to get only active accounts
    - Implement `GetDetailAccountsAsync` to get only detail (non-header) accounts
    - _Requirements: 7.1, 7.2, 7.3, 10.4_
  
  - [x] 3.6 Implement datatable method with filtering and pagination

    - Implement `Datatable` method with support for search, filters (type, status, header), sorting, and pagination
    - Use dynamic LINQ for flexible sorting
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [x] 3.7 Implement utility methods

    - Implement `GenerateAccountCodeAsync` to suggest next available code based on parent and type
    - Implement helper methods for normal balance determination based on account type
    - _Requirements: 1.4, 3.4, 4.4_

- [x] 4. Register ChartOfAccountService in dependency injection


  - Add service registration in `AppServiceCollection.cs`
  - _Requirements: All_

- [x] 5. Create ChartOfAccountController with API endpoints


  - [x] 5.1 Create `ChartOfAccountController` class extending `BaseController`

    - Inject `IChartOfAccountService` dependency
    - _Requirements: All_
  

  - [ ] 5.2 Implement Index action for main view
    - Return view with ViewData title
    - _Requirements: 5.1_

  
  - [ ] 5.3 Implement Datatable endpoint for listing accounts
    - POST endpoint accepting `DataTableChartOfAccountRequest`
    - Return JSON with paginated and filtered results
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  
  - [ ] 5.4 Implement GetById endpoint
    - GET endpoint accepting account ID
    - Return account details as JSON

    - _Requirements: 6.1_
  
  - [ ] 5.5 Implement GetHierarchy endpoint
    - GET endpoint returning full account tree structure
    - Used for tree view and parent account dropdown

    - _Requirements: 2.1, 2.2, 2.3_
  
  - [ ] 5.6 Implement GetParentAccounts endpoint
    - GET endpoint accepting account type parameter

    - Return filtered list of potential parent accounts
    - _Requirements: 1.5, 2.1_
  
  - [ ] 5.7 Implement Create endpoint
    - POST endpoint accepting `CreateChartOfAccountRequest`
    - Return success/error response with appropriate HTTP status codes
    - Include try-catch for error handling

    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_
  
  - [ ] 5.8 Implement Edit endpoint
    - POST endpoint accepting `UpdateChartOfAccountRequest`

    - Return success/error response
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  
  - [x] 5.9 Implement Delete endpoint

    - DELETE endpoint accepting account ID
    - Return success/error response with validation messages
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_
  
  - [ ] 5.10 Implement ToggleStatus endpoint
    - POST endpoint accepting account ID

    - Toggle between active and inactive status
    - _Requirements: 7.1, 7.2, 7.3_
  
  - [ ] 5.11 Implement ValidateCode endpoint
    - GET endpoint for real-time code validation
    - Accept code and optional excludeId parameters

    - Return boolean indicating if code is available
    - _Requirements: 4.2, 4.3_

- [ ] 6. Create Index view (Views/ChartOfAccount/Index.cshtml)
  - [x] 6.1 Create view file with page layout

    - Add page title and "Add New Account" button
    - Add filter section (Account Type dropdown, Status dropdown, Search input)
    - Add Export button
    - _Requirements: 5.1, 5.2, 5.3, 5.4_

  
  - [ ] 6.2 Add DataTable HTML structure
    - Create table with columns: Code, Account Name, Type, Parent, Status, Actions
    - Add action buttons (Edit, Delete, Toggle Status) for each row
    - _Requirements: 5.5_
  

  - [ ] 6.3 Add Create/Edit modal HTML
    - Create modal dialog with form fields (Account Code, Name, Type, Parent, Description, IsHeader checkbox, IsActive checkbox, Opening Balance)
    - Add validation message placeholders
    - Add Cancel and Save buttons
    - _Requirements: 1.1, 6.1_


- [ ] 7. Create JavaScript file for Index page (wwwroot/custom/features/chartofaccount/index.js)
  - [ ] 7.1 Initialize DataTable with server-side processing
    - Configure DataTable with AJAX source pointing to Datatable endpoint

    - Define columns with custom rendering for hierarchy (indentation), status badges, and action buttons
    - Implement tree view with expand/collapse functionality
    - _Requirements: 5.1, 5.5, 2.2_
  
  - [ ] 7.2 Implement filter functionality
    - Add event handlers for Account Type, Status, and Search filters
    - Reload DataTable when filters change
    - _Requirements: 5.2, 5.3, 5.4_

  
  - [ ] 7.3 Implement Create modal functionality
    - Show modal on "Add New Account" button click
    - Load parent account dropdown based on selected account type
    - Implement real-time account code validation
    - Handle form submission with AJAX POST to Create endpoint
    - Show success/error messages using SweetAlert or Toastr
    - Reload DataTable on successful creation
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_
  
  - [ ] 7.4 Implement Edit modal functionality
    - Load account data on Edit button click
    - Pre-fill form fields with existing data
    - Load parent account dropdown
    - Handle form submission with AJAX POST to Edit endpoint

    - Reload DataTable on successful update
    - _Requirements: 6.1, 6.2, 6.3, 6.4_
  
  - [ ] 7.5 Implement Delete functionality
    - Show confirmation dialog on Delete button click
    - Send AJAX DELETE request to Delete endpoint

    - Handle validation errors (account has transactions, has children)
    - Reload DataTable on successful deletion
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
  
  - [ ] 7.6 Implement Toggle Status functionality
    - Send AJAX POST request to ToggleStatus endpoint on status button click
    - Update row status badge without full page reload

    - _Requirements: 7.1, 7.2_
  
  - [ ] 7.7 Implement form validation
    - Add client-side validation for required fields
    - Add format validation for account code
    - Add length validation for account name

    - Display validation error messages
    - _Requirements: 12.1, 12.2, 12.3, 12.4_
  
  - [ ] 7.8 Implement account code auto-suggestion
    - Call GenerateAccountCode endpoint when parent account or type changes
    - Pre-fill account code input with suggested code

    - Allow user to override suggested code
    - _Requirements: 4.4_

- [ ] 8. Create seed data for Chart of Accounts
  - [x] 8.1 Create `ChartOfAccountSeed.cs` with standard COA structure

    - Define seed data for basic account structure (Assets, Liabilities, Equity, Revenue, Expenses)
    - Include common sub-accounts (Cash, Bank, Accounts Receivable, Accounts Payable, etc.)
    - Set proper hierarchy relationships
    - _Requirements: All_
  

  - [ ] 8.2 Add SeedChartOfAccounts method to `Seeder.cs`
    - Implement seeding logic with duplicate check
    - _Requirements: All_

  
  - [ ] 8.3 Update Program.cs to call ChartOfAccounts seeder
    - Add ChartOfAccounts seeding to auto-seed section
    - _Requirements: All_

- [x] 9. Add Chart of Accounts menu to MenuSeed



  - Update `MenuSeed.cs` to add "Chart of Accounts" menu item under Master menu
  - _Requirements: All_

- [ ] 10. Test the complete Chart of Accounts feature
  - [ ] 10.1 Test account creation with various scenarios
    - Create root account (Asset, Liability, Equity, Revenue, Expense)
    - Create sub-account under existing parent
    - Test validation errors (duplicate code, invalid format, incompatible parent type)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_
  
  - [ ] 10.2 Test hierarchy functionality
    - Verify tree view displays correctly with proper indentation
    - Test expand/collapse functionality
    - Verify parent account dropdown filters correctly
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_
  
  - [ ] 10.3 Test account editing
    - Edit account name, description, opening balance
    - Change parent account
    - Test validation errors
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  
  - [ ] 10.4 Test account deletion
    - Delete account without children or transactions
    - Attempt to delete account with children (should fail)
    - Attempt to delete account with transactions (should fail)
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_
  
  - [ ] 10.5 Test status toggle
    - Deactivate account and verify it doesn't appear in dropdowns
    - Reactivate account
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_
  
  - [ ] 10.6 Test search and filters
    - Search by account code and name
    - Filter by account type
    - Filter by status
    - _Requirements: 5.1, 5.2, 5.3, 5.4_

---

## Notes

- Each task should be completed and tested before moving to the next
- Use existing patterns from Role and User management as reference
- Follow the established coding standards and naming conventions
- Ensure all validation rules are implemented both client-side and server-side
- Test edge cases and error scenarios thoroughly
