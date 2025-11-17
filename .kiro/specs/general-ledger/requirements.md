# Requirements Document - General Ledger

## Introduction

General Ledger adalah modul untuk menampilkan history transaksi dan balance per account dari semua journal entries yang sudah di-post. Modul ini menjadi jembatan antara Journal Entry dengan laporan keuangan.

## Glossary

- **General Ledger System**: Sistem untuk tracking balance dan history transaksi per account
- **Ledger Entry**: Baris transaksi di general ledger yang berasal dari journal line yang sudah di-post
- **Running Balance**: Balance kumulatif setelah setiap transaksi
- **Opening Balance**: Balance awal periode untuk account
- **Closing Balance**: Balance akhir periode untuk account
- **Debit Balance**: Balance positif untuk Asset dan Expense accounts
- **Credit Balance**: Balance positif untuk Liability, Equity, dan Revenue accounts

## Requirements

### Requirement 1: View General Ledger by Account

**User Story:** As an accountant, I want to view general ledger for a specific account, so that I can see all transactions and running balance

#### Acceptance Criteria

1. WHEN viewing general ledger, THE General Ledger System SHALL display account code, name, and type
2. WHEN viewing general ledger, THE General Ledger System SHALL display all posted journal entries for the account
3. WHEN viewing general ledger, THE General Ledger System SHALL display transaction date, journal number, description, debit, credit, and running balance
4. WHEN viewing general ledger, THE General Ledger System SHALL calculate running balance after each transaction
5. WHEN viewing general ledger, THE General Ledger System SHALL display opening balance at the start
6. WHEN viewing general ledger, THE General Ledger System SHALL display closing balance at the end
7. WHEN viewing general ledger, THE General Ledger System SHALL only include Posted journal entries (exclude Draft and Reversed)

### Requirement 2: Filter General Ledger by Date Range

**User Story:** As an accountant, I want to filter general ledger by date range, so that I can view transactions for specific period

#### Acceptance Criteria

1. WHEN filtering by date range, THE General Ledger System SHALL accept date from and date to parameters
2. WHEN filtering by date range, THE General Ledger System SHALL include transactions within the date range
3. WHEN filtering by date range, THE General Ledger System SHALL calculate opening balance from transactions before date from
4. WHEN filtering by date range, THE General Ledger System SHALL calculate closing balance including all transactions up to date to
5. WHEN no date range specified, THE General Ledger System SHALL show all transactions from beginning

### Requirement 3: View All Accounts Ledger Summary

**User Story:** As an accountant, I want to view ledger summary for all accounts, so that I can see balance overview

#### Acceptance Criteria

1. WHEN viewing ledger summary, THE General Ledger System SHALL display all accounts with transactions
2. WHEN viewing ledger summary, THE General Ledger System SHALL display account code, name, type, and current balance
3. WHEN viewing ledger summary, THE General Ledger System SHALL support filtering by account type
4. WHEN viewing ledger summary, THE General Ledger System SHALL support filtering by date range
5. WHEN viewing ledger summary, THE General Ledger System SHALL support search by account code or name
6. WHEN viewing ledger summary, THE General Ledger System SHALL allow drill-down to account detail

### Requirement 4: Calculate Account Balance

**User Story:** As a system, I want to calculate account balance accurately, so that financial reports are correct

#### Acceptance Criteria

1. WHEN calculating balance for Asset accounts, THE General Ledger System SHALL use formula: Debit - Credit
2. WHEN calculating balance for Liability accounts, THE General Ledger System SHALL use formula: Credit - Debit
3. WHEN calculating balance for Equity accounts, THE General Ledger System SHALL use formula: Credit - Debit
4. WHEN calculating balance for Revenue accounts, THE General Ledger System SHALL use formula: Credit - Debit
5. WHEN calculating balance for Expense accounts, THE General Ledger System SHALL use formula: Debit - Credit
6. WHEN calculating balance for Other Income accounts, THE General Ledger System SHALL use formula: Credit - Debit
7. WHEN calculating balance for Other Expense accounts, THE General Ledger System SHALL use formula: Debit - Credit

### Requirement 5: Export General Ledger to Excel

**User Story:** As an accountant, I want to export general ledger to Excel, so that I can analyze data externally

#### Acceptance Criteria

1. WHEN exporting general ledger, THE General Ledger System SHALL include all filtered transactions
2. WHEN exporting general ledger, THE General Ledger System SHALL include opening and closing balance
3. WHEN exporting general ledger, THE General Ledger System SHALL format Excel with proper headers and styling
4. WHEN exporting general ledger, THE General Ledger System SHALL include account information in header
5. WHEN exporting general ledger, THE General Ledger System SHALL generate filename with account code and date range

### Requirement 6: View Ledger by Journal Entry

**User Story:** As an accountant, I want to view which accounts are affected by a journal entry, so that I can trace transactions

#### Acceptance Criteria

1. WHEN viewing ledger by journal, THE General Ledger System SHALL display all accounts affected by the journal
2. WHEN viewing ledger by journal, THE General Ledger System SHALL display debit and credit amounts per account
3. WHEN viewing ledger by journal, THE General Ledger System SHALL display journal header information
4. WHEN viewing ledger by journal, THE General Ledger System SHALL allow navigation to account detail ledger

### Requirement 7: Ledger Audit Trail

**User Story:** As an auditor, I want to see audit information in ledger, so that I can track accountability

#### Acceptance Criteria

1. WHEN viewing ledger entry, THE General Ledger System SHALL display who posted the journal
2. WHEN viewing ledger entry, THE General Ledger System SHALL display when the journal was posted
3. WHEN viewing ledger entry, THE General Ledger System SHALL display who created the original journal
4. WHEN viewing ledger entry, THE General Ledger System SHALL allow navigation to original journal entry

### Requirement 8: Ledger Performance

**User Story:** As a system, I want ledger queries to be fast, so that users have good experience

#### Acceptance Criteria

1. WHEN querying ledger for single account, THE General Ledger System SHALL return results within 2 seconds
2. WHEN querying ledger summary for all accounts, THE General Ledger System SHALL return results within 3 seconds
3. WHEN calculating running balance, THE General Ledger System SHALL use efficient query methods
4. WHEN filtering by date range, THE General Ledger System SHALL use indexed columns

### Requirement 9: Ledger Data Integrity

**User Story:** As a system, I want to ensure ledger data integrity, so that financial data is accurate

#### Acceptance Criteria

1. WHEN journal is posted, THE General Ledger System SHALL automatically create ledger entries
2. WHEN journal is reversed, THE General Ledger System SHALL include reversal entries in ledger
3. WHEN calculating balance, THE General Ledger System SHALL ensure debit equals credit for each journal
4. WHEN displaying ledger, THE General Ledger System SHALL only show entries from Posted journals
5. WHEN journal is deleted (soft delete), THE General Ledger System SHALL not include it in ledger

### Requirement 10: Ledger Pagination

**User Story:** As an accountant, I want ledger to be paginated, so that I can view large transaction history

#### Acceptance Criteria

1. WHEN viewing ledger with many transactions, THE General Ledger System SHALL support pagination
2. WHEN paginating ledger, THE General Ledger System SHALL maintain running balance calculation
3. WHEN paginating ledger, THE General Ledger System SHALL support configurable page size
4. WHEN paginating ledger, THE General Ledger System SHALL display total transaction count
