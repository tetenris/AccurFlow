# Implementation Plan - Financial Statements

## Overview

This implementation plan breaks down the Financial Statements feature into discrete, manageable coding tasks. The feature will include Income Statement, Balance Sheet, and Cash Flow Statement, reusing GeneralLedgerService for balance calculations.

---

## Tasks

- [x] 1. Create view models and request models





  - [x] 1.1 Create Income Statement models

    - Create `IncomeStatementViewModel` with sections, lines, and totals
    - Create `IncomeStatementSectionViewModel` for grouping by account type
    - Create `IncomeStatementLineViewModel` for individual account lines
    - Create `GetIncomeStatementRequest` with date range and filters


    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_


  - [ ] 1.2 Create Balance Sheet models
    - Create `BalanceSheetViewModel` with sections, totals, and balance validation
    - Create `BalanceSheetSectionViewModel` for Asset/Liability/Equity grouping


    - Create `BalanceSheetLineViewModel` for individual account lines
    - Create `GetBalanceSheetRequest` with as-of date and filters
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8_



  - [x] 1.3 Create Cash Flow Statement models


    - Create `CashFlowStatementViewModel` with sections and cash balances
    - Create `CashFlowSectionViewModel` for Operating/Investing/Financing
    - Create `CashFlowLineViewModel` for individual cash movements
    - Create `GetCashFlowStatementRequest` with date range
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7_


- [-] 2. Implement FinancialStatementService with business logic





  - [ ] 2.1 Create `IFinancialStatementService` interface
    - Define GetIncomeStatementAsync method
    - Define GetBalanceSheetAsync method
    - Define GetCashFlowStatementAsync method
    - Define ExportIncomeStatementAsync method
    - Define ExportBalanceSheetAsync method
    - Define ExportCashFlowAsync method
    - Extend IBaseService interface
    - _Requirements: All_




  - [ ] 2.2 Implement GetIncomeStatementAsync method
    - Get all Revenue and Other Income accounts from ChartOfAccount
    - Get all Expense and Other Expense accounts from ChartOfAccount
    - For each account, get balance using GeneralLedgerService.GetAccountBalanceAsync()
    - Filter zero balance accounts if ShowZeroBalance = false
    - Group accounts by AccountType (Revenue, Other Income, Expense, Other Expense)
    - Calculate subtotal per group
    - Calculate total revenue (Revenue + Other Income)
    - Calculate total expense (Expense + Other Expense)
    - Calculate net income (total revenue - total expense)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 4.1, 4.4, 7.1, 7.2_

  - [x] 2.3 Implement GetBalanceSheetAsync method


    - Get all Asset accounts from ChartOfAccount
    - Get all Liability accounts from ChartOfAccount
    - Get all Equity accounts from ChartOfAccount
    - For each account, get balance using GeneralLedgerService.GetAccountBalanceAsync()
    - Filter zero balance accounts if ShowZeroBalance = false
    - Group accounts by AccountType (Asset, Liability, Equity)
    - Calculate subtotal per group
    - Calculate total assets
    - Calculate total liabilities
    - Calculate total equity
    - Verify Assets = Liabilities + Equity



    - Set IsBalanced flag and calculate difference
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 4.2, 4.4, 7.1, 7.2, 7.3, 7.4_

  - [ ] 2.4 Implement GetCashFlowStatementAsync method
    - Get all cash accounts (accounts with "Cash" or "Bank" in name or specific account codes)
    - Calculate beginning cash balance (balance before DateFrom)
    - Get all cash account transactions in date range
    - Categorize transactions by activity type (Operating, Investing, Financing)
    - Calculate net cash from operating activities
    - Calculate net cash from investing activities
    - Calculate net cash from financing activities



    - Calculate net increase/decrease in cash
    - Calculate ending cash balance
    - Verify beginning + net change = ending
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1, 4.4, 7.1, 7.2_

  - [ ] 2.5 Implement ExportIncomeStatementAsync method
    - Get income statement data using GetIncomeStatementAsync
    - Create Excel workbook with NPOI
    - Add report header with company name and date range
    - Add revenue section with accounts and subtotal
    - Add other income section with accounts and subtotal


    - Add expense section with accounts and subtotal

    - Add other expense section with accounts and subtotal
    - Add total revenue, total expense, and net income
    - Format with proper styling (bold headers, number formatting)
    - Generate filename with date
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [ ] 2.6 Implement ExportBalanceSheetAsync method
    - Get balance sheet data using GetBalanceSheetAsync
    - Create Excel workbook with NPOI
    - Add report header with company name and as-of date


    - Add assets section with accounts and subtotal
    - Add liabilities section with accounts and subtotal
    - Add equity section with accounts and subtotal


    - Add total assets, total liabilities + equity


    - Add balance validation indicator
    - Format with proper styling
    - Generate filename with date
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_


  - [ ] 2.7 Implement ExportCashFlowAsync method
    - Get cash flow data using GetCashFlowStatementAsync

    - Create Excel workbook with NPOI
    - Add report header with company name and date range
    - Add beginning cash balance
    - Add operating activities section with subtotal
    - Add investing activities section with subtotal


    - Add financing activities section with subtotal
    - Add net increase/decrease
    - Add ending cash balance
    - Format with proper styling
    - Generate filename with date

    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 3. Register FinancialStatementService in dependency injection

  - Add service registration in `AppServiceCollection.cs`

  - _Requirements: All_

- [ ] 4. Create FinancialStatementController with API endpoints

  - [x] 4.1 Create `FinancialStatementController` class

    - Extend BaseController
    - Inject `IFinancialStatementService`
    - _Requirements: All_

  - [x] 4.2 Implement Index action

    - Return main view with tabs
    - _Requirements: 6.1, 6.2_

  - [x] 4.3 Implement GetIncomeStatement endpoint


    - GET endpoint accepting GetIncomeStatementRequest

    - Return IncomeStatementViewModel as JSON
    - Handle errors with try-catch
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 4.1, 4.4_

  - [x] 4.4 Implement GetBalanceSheet endpoint

    - GET endpoint accepting GetBalanceSheetRequest
    - Return BalanceSheetViewModel as JSON
    - Handle errors with try-catch

    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 4.2, 4.4_

  - [x] 4.5 Implement GetCashFlow endpoint

    - GET endpoint accepting GetCashFlowStatementRequest
    - Return CashFlowStatementViewModel as JSON
    - Handle errors with try-catch
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1, 4.4_

  - [ ] 4.6 Implement ExportIncomeStatement endpoint
    - GET endpoint with filter parameters

    - Return Excel file
    - Set proper content type and filename
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [x] 4.7 Implement ExportBalanceSheet endpoint


    - GET endpoint with filter parameters

    - Return Excel file
    - Set proper content type and filename
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_


  - [x] 4.8 Implement ExportCashFlow endpoint

    - GET endpoint with filter parameters
    - Return Excel file
    - Set proper content type and filename

    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 5. Create Index view (Views/FinancialStatement/Index.cshtml)

  - [ ] 5.1 Create view file with page layout
    - Add page title "Financial Statements"


    - Add Bootstrap tab navigation (Income Statement, Balance Sheet, Cash Flow)
    - Add tab content containers
    - _Requirements: 6.1, 6.2, 6.3, 8.1_

  - [ ] 5.2 Create Income Statement tab content
    - Add filter section (Date From, Date To, Show Zero Balance checkbox)
    - Add "Generate" button
    - Add "Export Excel" button

    - Add table container for report display

    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 4.1, 4.4, 5.1_

  - [ ] 5.3 Create Balance Sheet tab content
    - Add filter section (As of Date, Show Zero Balance checkbox)
    - Add "Generate" button
    - Add "Export Excel" button
    - Add table container for report display
    - Add balance validation indicator
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 4.2, 4.4, 5.1_



  - [ ] 5.4 Create Cash Flow tab content
    - Add filter section (Date From, Date To)
    - Add "Generate" button
    - Add "Export Excel" button


    - Add table container for report display
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1, 4.4, 5.1_

- [x] 6. Create JavaScript file (wwwroot/custom/features/financialstatement/index.js)





  - [ ] 6.1 Implement tab initialization and navigation
    - Initialize Bootstrap tabs
    - Handle tab switching events
    - Set default active tab
    - _Requirements: 6.1, 6.2, 6.3_

  - [ ] 6.2 Implement Income Statement functionality
    - Add event handler for "Generate" button
    - Fetch data from API with filters

    - Render table with sections (Revenue, Other Income, Expense, Other Expense)
    - Display subtotals and net income
    - Add click handlers for drill-down to General Ledger
    - Handle empty result
    - Display loading indicator
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 4.1, 4.4, 6.4, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 9.1, 9.2, 9.4_

  - [ ] 6.3 Implement Balance Sheet functionality
    - Add event handler for "Generate" button

    - Fetch data from API with filters
    - Render table with sections (Assets, Liabilities, Equity)
    - Display subtotals and balance validation
    - Add click handlers for drill-down to General Ledger
    - Handle empty result

    - Display loading indicator
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 4.2, 4.4, 6.4, 7.3, 7.4, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 9.1, 9.2, 9.4_

  - [ ] 6.4 Implement Cash Flow functionality
    - Add event handler for "Generate" button

    - Fetch data from API with filters
    - Render table with sections (Operating, Investing, Financing)
    - Display beginning balance, net change, and ending balance
    - Add click handlers for drill-down to General Ledger
    - Handle empty result
    - Display loading indicator
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1, 4.4, 6.4, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 9.1, 9.2, 9.4_

  - [ ] 6.5 Implement export functionality for all reports
    - Add event handlers for "Export Excel" buttons
    - Build query string with active filters
    - Trigger file download
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 9.3_

  - [ ] 6.6 Implement drill-down to General Ledger
    - Add click handler on account rows
    - Navigate to General Ledger with accountId and date filter
    - Pass appropriate date range based on report type
    - _Requirements: 6.4_

- [ ] 7. Add Financial Statements menu to MenuSeed

  - Update `MenuSeed.cs` to add "Financial Statements" menu item under Accounting menu
  - Add submenu items for Income Statement, Balance Sheet, and Cash Flow (or use tabs in single page)
  - _Requirements: 6.1, 6.2_

- [ ]* 8. Test the complete Financial Statements feature

  - [ ]* 8.1 Test Income Statement generation
    - Generate report with various date ranges
    - Verify revenue and expense grouping
    - Verify subtotals and net income calculation
    - Test with zero balance filter
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_

  - [ ]* 8.2 Test Balance Sheet generation
    - Generate report with various as-of dates
    - Verify asset, liability, and equity grouping
    - Verify balance validation (Assets = Liabilities + Equity)
    - Test with zero balance filter
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8_

  - [ ]* 8.3 Test Cash Flow generation
    - Generate report with various date ranges
    - Verify activity categorization
    - Verify beginning and ending balance calculation
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7_

  - [ ]* 8.4 Test Excel export for all reports
    - Export Income Statement to Excel
    - Export Balance Sheet to Excel
    - Export Cash Flow to Excel
    - Verify Excel format and content
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [ ]* 8.5 Test drill-down functionality
    - Click on accounts in each report
    - Verify navigation to General Ledger
    - Verify correct date filter passed
    - _Requirements: 6.4_

  - [ ]* 8.6 Test data accuracy
    - Compare Income Statement with Trial Balance
    - Compare Balance Sheet with Trial Balance
    - Verify only Posted journals are included
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

---

## Notes

- Each task should be completed and tested before moving to the next
- Financial Statements reuse GeneralLedgerService for balance calculation - no need to duplicate logic
- All three reports follow similar patterns - implement Income Statement first as template
- Use Bootstrap tabs for navigation between reports in single page
- NPOI library is already installed for Excel export
- Follow existing patterns from General Ledger and Trial Balance implementation
- Ensure consistent styling and user experience across all reports
