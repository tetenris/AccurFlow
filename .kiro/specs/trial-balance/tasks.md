# Implementation Plan - Trial Balance

## Overview

This implementation plan breaks down the Trial Balance feature into discrete, manageable coding tasks. Trial Balance will reuse General Ledger Service for balance calculation, making implementation faster.

---

## Tasks

- [x] 1. Create view models and request models


  - [x] 1.1 Create `TrialBalanceLineViewModel` for single account line


    - Include AccountId, AccountCode, AccountName, AccountType
    - Include DebitBalance, CreditBalance
    - _Requirements: 1.2_

  
  - [x] 1.2 Create `TrialBalanceGroupViewModel` for grouping by account type
    - Include AccountType, List of TrialBalanceLineViewModel
    - Include SubtotalDebit, SubtotalCredit

    - _Requirements: 4.1, 4.2_
  
  - [x] 1.3 Create `TrialBalanceViewModel` for complete report
    - Include AsOfDate, List of TrialBalanceGroupViewModel

    - Include TotalDebit, TotalCredit, Difference, IsBalanced
    - _Requirements: 1.3, 1.4, 1.5, 8.1, 8.2, 8.3, 8.4_
  
  - [x] 1.4 Create `GetTrialBalanceRequest` for filtering


    - Include AsOfDate, AccountType, ShowZeroBalance
    - _Requirements: 2.1, 3.1, 7.1_


- [x] 2. Implement TrialBalanceService with business logic

  - [x] 2.1 Create `ITrialBalanceService` interface
    - Define GetTrialBalanceAsync method
    - Define ExportToExcelAsync method
    - _Requirements: All_
  
  - [x] 2.2 Implement GetTrialBalanceAsync method
    - Get all accounts from ChartOfAccount
    - For each account, get balance using GeneralLedgerService.GetAccountBalanceAsync()
    - Filter zero balance accounts if ShowZeroBalance = false

    - Separate balance into Debit or Credit column based on account type
    - Group accounts by AccountType
    - Calculate subtotal per group
    - Calculate grand total debit and credit
    - Verify balance (debit = credit)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 2.1, 2.2, 2.3, 3.1, 3.2, 3.3, 3.4, 4.1, 4.2, 4.3, 7.1, 7.2, 7.3, 8.1, 8.2, 8.3, 8.4_
  
  - [x] 2.3 Implement ExportToExcelAsync method
    - Get trial balance data using GetTrialBalanceAsync

    - Create Excel workbook with NPOI
    - Add report header with company name and date
    - Add account groups with subtotals
    - Add grand total
    - Format with proper styling
    - Generate filename with date
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 3. Register TrialBalanceService in dependency injection


  - Add service registration in `AppServiceCollection.cs`
  - _Requirements: All_

- [x] 4. Create TrialBalanceController with API endpoints


  - [x] 4.1 Create `TrialBalanceController` class


    - Inject `ITrialBalanceService`
    - _Requirements: All_
  
  - [x] 4.2 Implement Index action

    - Return main view
    - _Requirements: 1.1_
  
  - [x] 4.3 Implement GetTrialBalance endpoint

    - GET endpoint accepting GetTrialBalanceRequest
    - Return TrialBalanceViewModel
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_
  
  - [x] 4.4 Implement ExportExcel endpoint

    - GET endpoint with filter parameters

    - Return Excel file
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_



- [x] 5. Create Index view (Views/TrialBalance/Index.cshtml)

  - [x] 5.1 Create view file with page layout
    - Add page title "Trial Balance"
    - Add "Export Excel" button

    - Add filter section (As of Date, Account Type, Show Zero Balance checkbox)
    - Add "Generate" button
    - _Requirements: 1.1, 2.1, 2.4, 3.1, 7.1_
  
  - [x] 5.2 Add trial balance table HTML structure
    - Create table with columns: Account Code, Account Name, Debit, Credit

    - Add grouping by account type with subtotals
    - Add grand total row

    - Add balance indicator (balanced/not balanced)

    - Add click handler on account row to drill-down to general ledger
    - _Requirements: 1.2, 1.3, 1.4, 1.5, 4.1, 4.2, 6.1, 6.2, 8.1, 8.2, 8.3, 8.4_

- [x] 6. Create JavaScript file (wwwroot/custom/features/trialbalance/index.js)

  - [x] 6.1 Implement generateTrialBalance function

    - Fetch trial balance data from API
    - Display account groups with subtotals
    - Display grand total
    - Display balance indicator
    - Handle empty result
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 4.1, 4.2, 4.3, 8.1, 8.2, 8.3, 8.4_

  
  - [x] 6.2 Implement filter functionality
    - Add event handler for "Generate" button
    - Apply date, account type, and show zero balance filters


    - Reload trial balance data

    - _Requirements: 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 3.3, 3.4, 7.1, 7.2, 7.3, 7.4_

  
  - [x] 6.3 Implement export functionality
    - Trigger export with active filters
    - Download Excel file
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [x] 6.4 Implement drill-down to general ledger
    - Add click handler on account row
    - Navigate to general ledger with accountId and date filter
    - _Requirements: 6.1, 6.2, 6.3_

- [x] 7. Add Trial Balance menu to MenuSeed

  - Update `MenuSeed.cs` to add "Trial Balance" menu item under Accounting menu
  - _Requirements: All_

- [ ]* 8. Test the complete Trial Balance feature

  - [ ]* 8.1 Test trial balance generation
    - Generate trial balance with default settings
    - Verify all accounts displayed
    - Verify grouping by account type
    - Verify subtotals and grand total
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 4.1, 4.2, 4.3_
  
  - [ ]* 8.2 Test filters
    - Filter by as of date
    - Filter by account type
    - Toggle show zero balance
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 3.3, 3.4, 7.1, 7.2, 7.3, 7.4_
  
  - [ ]* 8.3 Test balance validation
    - Verify total debit equals total credit
    - Test with unbalanced data (if possible)
    - Verify balance indicator
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
  
  - [ ]* 8.4 Test export
    - Export to Excel
    - Verify Excel format and content
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [ ]* 8.5 Test drill-down
    - Click on account to drill-down to general ledger
    - Verify correct account and date filter passed
    - _Requirements: 6.1, 6.2, 6.3_

---

## Notes

- Each task should be completed and tested before moving to the next
- Trial Balance reuses GeneralLedgerService for balance calculation - no need to duplicate logic
- Balance is displayed in Debit or Credit column based on account type and balance sign
- Grouping by account type makes report more readable
- Balance validation (debit = credit) is critical for data integrity
- NPOI library is already installed for Excel export
- Follow existing patterns from General Ledger implementation

