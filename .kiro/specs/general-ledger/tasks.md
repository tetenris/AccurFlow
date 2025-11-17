# Implementation Plan - General Ledger

## Overview

This implementation plan breaks down the General Ledger feature into discrete, manageable coding tasks. Each task builds incrementally, following a bottom-up approach: models → service → controller → views → UI.

---

## Tasks

- [x] 1. Create view models and request models


  - [x] 1.1 Create `LedgerEntryViewModel` for single transaction line


    - Include JournalLineId, JournalId, JournalNumber, JournalDate, Description, Debit, Credit, RunningBalance
    - Include PostedBy and PostedDate for audit trail
    - _Requirements: 1.3, 7.1, 7.2_


  
  - [x] 1.2 Create `AccountLedgerViewModel` for account detail
    - Include AccountId, AccountCode, AccountName, AccountType
    - Include OpeningBalance, ClosingBalance


    - Include List of LedgerEntryViewModel
    - _Requirements: 1.1, 1.4, 1.5, 1.6_
  

  - [-] 1.3 Create `LedgerSummaryViewModel` for all accounts summary

    - Include AccountId, AccountCode, AccountName, AccountType

    - Include TotalDebit, TotalCredit, Balance, TransactionCount


    - _Requirements: 3.1, 3.2_
  


  - [x] 1.4 Create `GetLedgerRequest` for account ledger query
    - Include AccountId, DateFrom, DateTo, Page, PageSize
    - _Requirements: 2.1, 10.1, 10.3_
  
  - [x] 1.5 Create `GetLedgerSummaryRequest` for summary query
    - Include AccountType, DateFrom, DateTo, Search
    - _Requirements: 3.3, 3.4, 3.5_



- [x] 2. Implement GeneralLedgerService with business logic

  - [x] 2.1 Create `IGeneralLedgerService` interface
    - Define GetAccountLedgerAsync method
    - Define GetLedgerSummaryAsync method

    - Define GetAccountBalanceAsync method
    - Define GetOpeningBalanceAsync method
    - Define ExportLedgerToExcelAsync method
    - Define ExportSummaryToExcelAsync method

    - _Requirements: All_
  
  - [x] 2.2 Implement GetAccountBalanceAsync method
    - Query JournalLine for specific account
    - Filter by Posted journals only
    - Filter by date if specified
    - Sum debit and credit amounts
    - Apply balance formula based on account type (Asset/Expense: Debit-Credit, others: Credit-Debit)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_
  

  - [x] 2.3 Implement GetOpeningBalanceAsync method
    - Calculate balance from all transactions before dateFrom
    - Use same balance formula as GetAccountBalanceAsync
    - _Requirements: 2.3_
  
  - [x] 2.4 Implement GetAccountLedgerAsync method
    - Query JournalLine for specific account with Posted journals
    - Join with JournalEntry and User (PostedBy)
    - Filter by date range if specified
    - Order by JournalDate, then JournalNumber

    - Calculate opening balance
    - Calculate running balance for each entry
    - Support pagination
    - Calculate closing balance
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 2.1, 2.2, 2.3, 2.4, 2.5, 10.1, 10.2, 10.3, 10.4_
  
  - [x] 2.5 Implement GetLedgerSummaryAsync method
    - Query all accounts with transactions
    - Join with JournalLine and JournalEntry (Posted only)
    - Group by account

    - Calculate total debit, credit, and balance per account
    - Filter by account type if specified
    - Filter by date range if specified
    - Filter by search term (account code or name)
    - Count transactions per account
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_
  


  - [x] 2.6 Implement ExportLedgerToExcelAsync method
    - Get ledger data using GetAccountLedgerAsync
    - Create Excel workbook with NPOI

    - Add account information in header
    - Add opening balance row


    - Add transaction rows with running balance
    - Add closing balance and totals
    - Format with proper styling

    - Generate filename with account code and date range
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  

  - [x] 2.7 Implement ExportSummaryToExcelAsync method
    - Get summary data using GetLedgerSummaryAsync
    - Create Excel workbook with NPOI
    - Add summary rows for all accounts

    - Add totals row
    - Format with proper styling
    - Generate filename with date range
    - _Requirements: 5.1, 5.3_


- [x] 3. Register GeneralLedgerService in dependency injection

  - Add service registration in `AppServiceCollection.cs`

  - _Requirements: All_

- [x] 4. Create GeneralLedgerController with API endpoints


  - [x] 4.1 Create `GeneralLedgerController` class
    - Inject `IGeneralLedgerService` and `IChartOfAccountService`
    - _Requirements: All_
  

  - [x] 4.2 Implement Index action (Summary view)
    - Return main view for ledger summary

    - _Requirements: 3.1_
  


  - [x] 4.3 Implement AccountLedger action (Detail view)
    - Accept accountId parameter
    - Return view for specific account ledger
    - _Requirements: 1.1_
  

  - [x] 4.4 Implement GetLedger endpoint
    - GET endpoint accepting GetLedgerRequest

    - Return AccountLedgerViewModel
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_


  
  - [x] 4.5 Implement GetSummary endpoint
    - GET endpoint accepting GetLedgerSummaryRequest
    - Return List of LedgerSummaryViewModel
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_
  

  - [x] 4.6 Implement GetBalance endpoint
    - GET endpoint accepting accountId and optional date
    - Return current balance

    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_
  


  - [x] 4.7 Implement ExportLedger endpoint
    - GET endpoint with accountId and date range parameters
    - Return Excel file

    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [x] 4.8 Implement ExportSummary endpoint
    - GET endpoint with filter parameters

    - Return Excel file
    - _Requirements: 5.1, 5.3_

- [x] 5. Create Index view (Views/GeneralLedger/Index.cshtml) for Summary


  - [x] 5.1 Create view file with page layout
    - Add page title "General Ledger"

    - Add "Export Summary" button
    - Add filter section (Date From, Date To, Account Type dropdown, Search)


    - _Requirements: 3.1, 3.3, 3.4, 3.5_
  
  - [x] 5.2 Add DataTable HTML structure
    - Create table with columns: Account Code, Account Name, Type, Total Debit, Total Credit, Balance, Transactions
    - Add click handler to drill-down to account detail

    - _Requirements: 3.1, 3.2, 3.6_

- [x] 6. Create AccountLedger view (Views/GeneralLedger/AccountLedger.cshtml) for Detail


  - [x] 6.1 Create view file with page layout
    - Add page title with account code and name
    - Add "Back to Summary" and "Export" buttons
    - Add filter section (Date From, Date To)
    - Display opening balance

    - _Requirements: 1.1, 1.5, 2.1, 2.2_
  
  - [x] 6.2 Add ledger table HTML structure
    - Create table with columns: Date, Journal Number, Description, Debit, Credit, Running Balance

    - Add click handler on journal number to view journal detail
    - Display closing balance and totals at bottom
    - _Requirements: 1.2, 1.3, 1.4, 1.6, 7.4_



- [x] 7. Create JavaScript file for Summary (wwwroot/custom/features/generalledger/summary.js)



  - [-] 7.1 Initialize DataTable for summary

    - Configure columns with custom rendering for balance
    - Implement click handler for drill-down to account detail
    - _Requirements: 3.1, 3.2, 3.6_
  
  - [x] 7.2 Implement filter functionality
    - Add event handlers for date range, account type, and search filters
    - Reload data when filters change
    - _Requirements: 3.3, 3.4, 3.5_
  
  - [x] 7.3 Implement export summary functionality
    - Trigger export with active filters
    - Download Excel file
    - _Requirements: 5.1, 5.3_
  
  - [x] 7.4 Implement drill-down navigation
    - Navigate to account ledger detail view
    - Pass accountId and current filters
    - _Requirements: 3.6, 6.1_

- [x] 8. Create JavaScript file for Account Ledger (wwwroot/custom/features/generalledger/ledger.js)

  - [x] 8.1 Load and display ledger data
    - Fetch ledger data from API
    - Display opening balance
    - Render transaction rows with running balance
    - Display closing balance and totals
    - _Requirements: 1.2, 1.3, 1.4, 1.5, 1.6_
  
  - [x] 8.2 Implement date filter functionality
    - Add event handlers for date range filter
    - Reload ledger data when filter changes
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_
  
  - [x] 8.3 Implement pagination
    - Support page navigation
    - Maintain running balance across pages
    - Display total transaction count
    - _Requirements: 10.1, 10.2, 10.3, 10.4_
  
  - [x] 8.4 Implement export ledger functionality
    - Trigger export with active filters
    - Download Excel file
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [x] 8.5 Implement journal entry navigation
    - Add click handler on journal number
    - Navigate to journal entry detail
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 7.4_

- [x] 9. Add General Ledger menu to MenuSeed

  - Update `MenuSeed.cs` to add "General Ledger" menu item under Accounting menu
  - _Requirements: All_

- [x] 10. Add database index for performance

  - Add composite index on JournalLine (AccountId, JournalDate) for faster ledger queries
  - _Requirements: 8.1, 8.2, 8.3, 8.4_

- [ ]* 11. Test the complete General Ledger feature

  - [ ]* 11.1 Test ledger summary view
    - View summary for all accounts
    - Test filters (date range, account type, search)
    - Test drill-down to account detail
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6_
  
  - [ ]* 11.2 Test account ledger detail
    - View ledger for specific account
    - Test date range filter
    - Verify opening and closing balance
    - Verify running balance calculation
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 2.1, 2.2, 2.3, 2.4_
  
  - [ ]* 11.3 Test balance calculation
    - Test balance for Asset accounts (Debit - Credit)
    - Test balance for Liability accounts (Credit - Debit)
    - Test balance for Revenue accounts (Credit - Debit)
    - Test balance for Expense accounts (Debit - Credit)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_
  
  - [ ]* 11.4 Test export functionality
    - Export ledger detail to Excel
    - Export summary to Excel
    - Verify Excel format and content
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [ ]* 11.5 Test data integrity
    - Verify only Posted journals appear in ledger
    - Verify Reversed journals are included
    - Verify Draft journals are excluded
    - _Requirements: 1.7, 9.1, 9.2, 9.3, 9.4, 9.5_
  
  - [ ]* 11.6 Test performance
    - Test ledger query performance for accounts with many transactions
    - Test summary query performance
    - Verify pagination works correctly
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 10.1, 10.2, 10.3, 10.4_

---

## Notes

- Each task should be completed and tested before moving to the next
- General Ledger is read-only, no create/edit/delete operations
- Use existing JournalEntry and JournalLine data, no new tables needed
- Balance calculation formula depends on account type
- Running balance must be calculated in correct order (by date, then journal number)
- Only include Posted journal entries in ledger
- NPOI library is already installed for Excel export
- Follow existing patterns from Journal Entry implementation


