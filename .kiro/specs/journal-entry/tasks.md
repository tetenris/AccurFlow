# Implementation Plan - Journal Entry Management

## Overview

This implementation plan breaks down the Journal Entry Management feature into discrete, manageable coding tasks. Each task builds incrementally on previous tasks, following a bottom-up approach: data layer → business logic → API → UI.

---

## Tasks

- [x] 1. Create JournalEntry and JournalLine entities with database migration



  - Create `JournalEntryEntity` class with all required properties (JournalNumber, JournalDate, Description, Status, TotalDebit, TotalCredit, PostedDate, PostedBy, ReversalJournalId, OriginalJournalId)
  - Create `JournalLineEntity` class with properties (JournalLineId, JournalId, LineNumber, AccountId, Description, DebitAmount, CreditAmount)
  - Create entity configuration classes with proper constraints, indexes, and relationships
  - Add navigation properties (JournalEntry.JournalLines, JournalLine.JournalEntry, JournalLine.Account)
  - Generate and apply EF Core migration
  - _Requirements: 1.1, 1.2, 1.3, 4.1, 5.1, 11.1, 12.1_

- [x] 2. Create view models and request/response DTOs




  - [x] 2.1 Create `JournalEntryViewModel` for displaying journal header

    - Include computed fields (IsBalanced, CanEdit, CanDelete, CanPost, CanReverse)
    - _Requirements: 6.1, 7.1, 9.1_
  

  - [x] 2.2 Create `JournalLineViewModel` for displaying journal lines



    - Include account code and name from related account
    - _Requirements: 7.2_

  
  - [x] 2.3 Create `JournalEntryDetailViewModel` combining header and lines

    - Include list of JournalLineViewModel

    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_
  

  - [x] 2.4 Create `CreateJournalEntryRequest` with validation
    - Include header fields and list of JournalLineRequest

    - Add data annotations for required fields
    - _Requirements: 1.1, 1.2, 1.3, 1.4_
  
  - [x] 2.5 Create `UpdateJournalEntryRequest` with validation

    - Similar to create but includes JournalId
    - _Requirements: 2.1, 2.2, 2.3_
  


  - [x] 2.6 Create `JournalLineRequest` for line data
    - Include AccountId, Description, DebitAmount, CreditAmount
    - _Requirements: 1.3_
  
  - [x] 2.7 Create `DataTableJournalEntryRequest` for filtering

    - Extend BaseDatatableRequest with DateFrom, DateTo, Status, AccountId filters

    - _Requirements: 6.2, 6.3, 6.4, 6.5_
  
  - [x] 2.8 Create `PostJournalRequest` and `ReverseJournalRequest`
    - Include JournalId and date fields
    - _Requirements: 4.1, 5.1_

- [x] 3. Implement JournalEntryService with core business logic



  - [x] 3.1 Create `IJournalEntryService` interface

    - Define all service methods (CRUD, Post, Reverse, Validation, Export)
    - _Requirements: All_
  

  - [x] 3.2 Implement CRUD operations

    - Implement `CreateAsync` with balance validation and journal number generation
    - Implement `UpdateAsync` with status check and balance validation
    - Implement `DeleteAsync` with status check and soft delete
    - Implement `GetByIdAsync` and `GetByNumberAsync`
    - _Requirements: 1.1-1.7, 2.1-2.4, 3.1-3.4, 8.1-8.7_

  

  - [x] 3.3 Implement Post operation
    - Implement `PostAsync` with validation (status, balance, accounts)
    - Update status to Posted
    - Record PostedDate and PostedBy
    - Prevent further editing

    - _Requirements: 4.1-4.6_

  
  - [x] 3.4 Implement Reverse operation
    - Implement `ReverseAsync` with status validation
    - Create new journal with reversed amounts
    - Link original and reversal journals
    - Auto-post reversal journal

    - Update original status to Reversed

    - _Requirements: 5.1-5.6_
  
  - [x] 3.5 Implement validation methods
    - Implement `IsBalancedAsync` to check debit = credit

    - Implement `CanEditAsync`, `CanDeleteAsync`, `CanPostAsync`, `CanReverseAsync`
    - Implement `ValidateJournalAsync` with all business rules
    - _Requirements: 1.4, 2.1, 3.1, 4.2, 5.1, 8.1-8.7_
  
  - [x] 3.6 Implement Datatable method with filtering

    - Support date range, status, and account filters
    - Support search by journal number or description
    - Support sorting and pagination

    - _Requirements: 6.1-6.7_

  
  - [x] 3.7 Implement utility methods

    - Implement `GenerateJournalNumberAsync` with format JE-YYYYMMDD-XXXX
    - Implement `GetByAccountAsync` and `GetByDateRangeAsync`
    - _Requirements: 11.1-11.4_
  
  - [x] 3.8 Implement export method
    - Implement `ExportToExcelAsync` with NPOI
    - Include header and lines in separate sheets
    - Apply filters
    - _Requirements: 10.1-10.5_

- [x] 4. Register JournalEntryService in dependency injection


  - Add service registration in `AppServiceCollection.cs`
  - _Requirements: All_

- [x] 5. Create JournalEntryController with API endpoints



  - [x] 5.1 Create `JournalEntryController` class

    - Inject `IJournalEntryService` and `IChartOfAccountService`
    - _Requirements: All_
  


  - [x] 5.2 Implement Index action
    - Return main view
    - _Requirements: 6.1_


  
  - [x] 5.3 Implement Datatable endpoint
    - POST endpoint accepting DataTableJournalEntryRequest
    - Return paginated and filtered results

    - _Requirements: 6.1-6.7_
  
  - [x] 5.4 Implement GetById and GetByNumber endpoints

    - Return journal detail with lines
    - _Requirements: 7.1-7.5_
  
  - [x] 5.5 Implement Create endpoint

    - POST endpoint accepting CreateJournalEntryRequest
    - Validate and create journal

    - _Requirements: 1.1-1.7_
  

  - [x] 5.6 Implement Edit endpoint
    - POST endpoint accepting UpdateJournalEntryRequest
    - Validate status and update journal

    - _Requirements: 2.1-2.4_
  
  - [x] 5.7 Implement Delete endpoint


    - DELETE endpoint with status validation
    - _Requirements: 3.1-3.4_
  


  - [x] 5.8 Implement Post endpoint
    - POST endpoint to finalize journal
    - _Requirements: 4.1-4.6_


  
  - [x] 5.9 Implement Reverse endpoint
    - POST endpoint to reverse posted journal


    - _Requirements: 5.1-5.6_
  

  - [x] 5.10 Implement GenerateNumber endpoint
    - GET endpoint to generate next journal number
    - _Requirements: 11.1-11.4_
  
  - [x] 5.11 Implement GetAccountDropdown endpoint
    - GET endpoint returning active detail accounts
    - _Requirements: 1.6, 1.7_
  
  - [x] 5.12 Implement ExportExcel endpoint
    - GET endpoint with filter parameters
    - Return Excel file
    - _Requirements: 10.1-10.5_

- [x] 6. Create Index view (Views/JournalEntry/Index.cshtml)



  - [x] 6.1 Create view file with page layout

    - Add page title and "Add New Journal" button
    - Add "Export Excel" button
    - Add filter section (Date From, Date To, Status dropdown, Account dropdown)
    - _Requirements: 6.1, 6.2, 6.3, 6.4_
  

  - [x] 6.2 Add DataTable HTML structure

    - Create table with columns: No, Journal Number, Date, Description, Total Amount, Status, Actions
    - Add action buttons (View, Edit, Delete, Post, Reverse) based on status
    - _Requirements: 6.1, 6.7_

  

  - [x] 6.3 Add Create/Edit modal HTML
    - Create modal with journal header fields (Number, Date, Description)
    - Add journal lines section with dynamic add/remove
    - Add total debit/credit display with balance indicator
    - Add Save button

    - _Requirements: 1.1, 1.2, 1.3, 2.1_

  
  - [x] 6.4 Add Detail modal HTML
    - Create read-only modal showing journal header and lines
    - Display audit information
    - Add Post and Reverse buttons based on status
    - _Requirements: 7.1-7.5_

- [x] 7. Create JavaScript file (wwwroot/custom/features/journalentry/index.js)



  - [x] 7.1 Initialize DataTable with server-side processing

    - Configure columns with custom rendering for status badges
    - Implement conditional action buttons based on status
    - _Requirements: 6.1, 6.7, 9.1-9.5_

  

  - [x] 7.2 Implement filter functionality
    - Add event handlers for date range, status, and account filters
    - Reload DataTable when filters change

    - _Requirements: 6.2, 6.3, 6.4, 6.5_

  
  - [x] 7.3 Implement Create modal functionality
    - Show modal on "Add New Journal" button click
    - Load account dropdown
    - Generate journal number
    - Implement dynamic add/remove journal lines
    - Implement real-time balance calculation

    - Handle form submission with validation

    - _Requirements: 1.1-1.7, 8.1-8.7_
  
  - [x] 7.4 Implement Edit modal functionality
    - Load journal data on Edit button click
    - Pre-fill form fields
    - Load existing journal lines
    - Allow add/remove lines

    - Real-time balance calculation

    - Handle form submission
    - _Requirements: 2.1-2.4_
  
  - [x] 7.5 Implement Delete functionality
    - Show confirmation dialog

    - Validate status before delete
    - Send DELETE request
    - _Requirements: 3.1-3.4_
  
  - [x] 7.6 Implement Post functionality


    - Show confirmation dialog
    - Send POST request to Post endpoint
    - Update DataTable on success
    - _Requirements: 4.1-4.6_
  


  - [x] 7.7 Implement Reverse functionality
    - Show confirmation dialog with reversal date input
    - Send POST request to Reverse endpoint
    - Update DataTable on success
    - _Requirements: 5.1-5.6_


  
  - [x] 7.8 Implement Detail modal functionality
    - Load and display journal header and lines
    - Show audit information
    - Display Post/Reverse buttons based on status
    - _Requirements: 7.1-7.5_
  


  - [x] 7.9 Implement form validation
    - Client-side validation for required fields
    - Validate at least 2 lines
    - Validate balance (debit = credit)
    - Validate positive amounts
    - Display validation errors
    - _Requirements: 8.1-8.7, 12.1-12.5_
  
  - [x] 7.10 Implement Export functionality
    - Trigger export with active filters
    - Download Excel file
    - _Requirements: 10.1-10.5_

- [x] 8. Add Journal Entry menu to MenuSeed




  - Update `MenuSeed.cs` to add "Journal Entry" menu item under Accounting menu
  - _Requirements: All_

- [x] 9. Test the complete Journal Entry feature

  - [x] 9.1 Test journal creation
    - Create journal with multiple lines
    - Test balance validation
    - Test account validation
    - _Requirements: 1.1-1.7, 8.1-8.7_
  
  - [x] 9.2 Test journal editing
    - Edit draft journal
    - Add/remove lines
    - Test status validation (cannot edit posted)
    - _Requirements: 2.1-2.4_
  
  - [x] 9.3 Test journal posting
    - Post valid journal
    - Verify status change
    - Verify cannot edit after posting
    - _Requirements: 4.1-4.6_
  
  - [x] 9.4 Test journal reversal
    - Reverse posted journal
    - Verify new reversal journal created
    - Verify original status changed to Reversed
    - Verify amounts reversed
    - _Requirements: 5.1-5.6_
  
  - [x] 9.5 Test filters and search
    - Filter by date range
    - Filter by status
    - Filter by account
    - Search by journal number
    - _Requirements: 6.2-6.5_
  
  - [x] 9.6 Test export
    - Export with filters
    - Verify Excel format
    - _Requirements: 10.1-10.5_

---

## Notes

- Each task should be completed and tested before moving to the next
- Use Chart of Accounts service for account validation and dropdown
- Follow existing patterns from Chart of Accounts implementation
- Ensure all validation rules are implemented both client-side and server-side
- Test edge cases thoroughly (unbalanced journals, invalid accounts, status transitions)
- NPOI library is already installed for Excel export
