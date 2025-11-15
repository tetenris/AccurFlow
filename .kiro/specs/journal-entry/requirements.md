# Requirements Document - Journal Entry Management

## Introduction

Journal Entry adalah modul untuk mencatat transaksi akuntansi menggunakan sistem double-entry bookkeeping. Setiap transaksi harus balance (total debit = total credit) dan terhubung dengan Chart of Accounts yang sudah dibuat.

## Glossary

- **Journal Entry System**: Sistem pencatatan transaksi akuntansi
- **Double-Entry Bookkeeping**: Sistem pencatatan dimana setiap transaksi dicatat minimal 2 kali (debit dan credit) dan harus balance
- **Journal Header**: Data utama journal (tanggal, nomor, deskripsi, status)
- **Journal Line**: Detail baris journal (account, debit/credit amount, description)
- **Posting**: Proses finalisasi journal entry agar masuk ke general ledger
- **Reversal**: Proses membatalkan journal yang sudah diposting dengan membuat journal kebalikan
- **Balance**: Kondisi dimana total debit = total credit dalam satu journal entry

## Requirements

### Requirement 1: Create Journal Entry

**User Story:** As an accountant, I want to create journal entries with multiple lines, so that I can record financial transactions accurately

#### Acceptance Criteria

1. WHEN creating a journal entry, THE Journal Entry System SHALL generate a unique journal number with format "JE-YYYYMMDD-XXXX"
2. WHEN creating a journal entry, THE Journal Entry System SHALL require journal date, description, and at least 2 journal lines
3. WHEN adding journal lines, THE Journal Entry System SHALL validate that each line has account, amount, and debit/credit type
4. WHEN saving a journal entry, THE Journal Entry System SHALL validate that total debit equals total credit
5. WHEN saving a journal entry, THE Journal Entry System SHALL set initial status as "Draft"
6. WHEN selecting an account for journal line, THE Journal Entry System SHALL only show detail accounts (non-header accounts)
7. WHEN selecting an account for journal line, THE Journal Entry System SHALL only show active accounts

### Requirement 2: Edit Journal Entry

**User Story:** As an accountant, I want to edit draft journal entries, so that I can correct mistakes before posting

#### Acceptance Criteria

1. WHEN editing a journal entry, THE Journal Entry System SHALL only allow editing if status is "Draft"
2. WHEN editing a journal entry, THE Journal Entry System SHALL prevent editing if status is "Posted" or "Reversed"
3. WHEN updating journal lines, THE Journal Entry System SHALL re-validate that total debit equals total credit
4. WHEN updating journal entry, THE Journal Entry System SHALL record who updated and when

### Requirement 3: Delete Journal Entry

**User Story:** As an accountant, I want to delete draft journal entries, so that I can remove incorrect entries

#### Acceptance Criteria

1. WHEN deleting a journal entry, THE Journal Entry System SHALL only allow deletion if status is "Draft"
2. WHEN deleting a journal entry, THE Journal Entry System SHALL prevent deletion if status is "Posted"
3. WHEN deleting a journal entry, THE Journal Entry System SHALL use soft delete (set IsDeleted flag)
4. WHEN deleting a journal entry, THE Journal Entry System SHALL also soft delete all related journal lines

### Requirement 4: Post Journal Entry

**User Story:** As an accountant, I want to post journal entries, so that transactions are finalized and reflected in general ledger

#### Acceptance Criteria

1. WHEN posting a journal entry, THE Journal Entry System SHALL validate that status is "Draft"
2. WHEN posting a journal entry, THE Journal Entry System SHALL validate that total debit equals total credit
3. WHEN posting a journal entry, THE Journal Entry System SHALL validate that all accounts exist and are active
4. WHEN posting a journal entry, THE Journal Entry System SHALL change status to "Posted"
5. WHEN posting a journal entry, THE Journal Entry System SHALL record posted date and posted by user
6. WHEN posting a journal entry, THE Journal Entry System SHALL prevent any further editing or deletion

### Requirement 5: Reverse Journal Entry

**User Story:** As an accountant, I want to reverse posted journal entries, so that I can correct mistakes in posted transactions

#### Acceptance Criteria

1. WHEN reversing a journal entry, THE Journal Entry System SHALL only allow reversal if status is "Posted"
2. WHEN reversing a journal entry, THE Journal Entry System SHALL create a new journal entry with reversed debit/credit amounts
3. WHEN reversing a journal entry, THE Journal Entry System SHALL auto-post the reversal journal
4. WHEN reversing a journal entry, THE Journal Entry System SHALL change original journal status to "Reversed"
5. WHEN reversing a journal entry, THE Journal Entry System SHALL link original and reversal journals with reference IDs
6. WHEN reversing a journal entry, THE Journal Entry System SHALL use reversal date as journal date for new entry

### Requirement 6: View Journal Entry List

**User Story:** As an accountant, I want to view list of journal entries with filters, so that I can find specific transactions easily

#### Acceptance Criteria

1. WHEN viewing journal list, THE Journal Entry System SHALL display journal number, date, description, total amount, status, and actions
2. WHEN viewing journal list, THE Journal Entry System SHALL support filtering by date range
3. WHEN viewing journal list, THE Journal Entry System SHALL support filtering by status (Draft, Posted, Reversed)
4. WHEN viewing journal list, THE Journal Entry System SHALL support filtering by account
5. WHEN viewing journal list, THE Journal Entry System SHALL support search by journal number or description
6. WHEN viewing journal list, THE Journal Entry System SHALL support pagination with configurable page size
7. WHEN viewing journal list, THE Journal Entry System SHALL support sorting by any column

### Requirement 7: View Journal Entry Detail

**User Story:** As an accountant, I want to view journal entry details with all lines, so that I can review transaction information

#### Acceptance Criteria

1. WHEN viewing journal detail, THE Journal Entry System SHALL display header information (number, date, description, status, total)
2. WHEN viewing journal detail, THE Journal Entry System SHALL display all journal lines with account code, account name, description, debit, and credit
3. WHEN viewing journal detail, THE Journal Entry System SHALL display total debit and total credit at bottom
4. WHEN viewing journal detail, THE Journal Entry System SHALL display audit information (created by, created at, posted by, posted at)
5. WHEN viewing journal detail, THE Journal Entry System SHALL display reversal information if journal is reversed or is a reversal

### Requirement 8: Journal Entry Validation

**User Story:** As a system, I want to validate journal entries, so that data integrity is maintained

#### Acceptance Criteria

1. WHEN validating journal entry, THE Journal Entry System SHALL ensure journal date is not in future
2. WHEN validating journal entry, THE Journal Entry System SHALL ensure at least 2 journal lines exist
3. WHEN validating journal entry, THE Journal Entry System SHALL ensure total debit equals total credit
4. WHEN validating journal entry, THE Journal Entry System SHALL ensure all amounts are positive numbers
5. WHEN validating journal entry, THE Journal Entry System SHALL ensure all accounts are detail accounts (not header)
6. WHEN validating journal entry, THE Journal Entry System SHALL ensure all accounts are active
7. WHEN validating journal entry, THE Journal Entry System SHALL ensure no duplicate accounts in same journal (one account can appear multiple times but with different debit/credit)

### Requirement 9: Journal Entry Status Management

**User Story:** As an accountant, I want clear status indicators, so that I know which journals are finalized

#### Acceptance Criteria

1. WHEN journal is created, THE Journal Entry System SHALL set status as "Draft" with yellow badge
2. WHEN journal is posted, THE Journal Entry System SHALL set status as "Posted" with green badge
3. WHEN journal is reversed, THE Journal Entry System SHALL set status as "Reversed" with red badge
4. WHEN viewing journal list, THE Journal Entry System SHALL show status badge with appropriate color
5. WHEN viewing journal detail, THE Journal Entry System SHALL show status badge prominently

### Requirement 10: Journal Entry Export

**User Story:** As an accountant, I want to export journal entries to Excel, so that I can analyze data externally

#### Acceptance Criteria

1. WHEN exporting journals, THE Journal Entry System SHALL include all header and line information
2. WHEN exporting journals, THE Journal Entry System SHALL respect active filters (date range, status, account)
3. WHEN exporting journals, THE Journal Entry System SHALL format Excel with proper headers and styling
4. WHEN exporting journals, THE Journal Entry System SHALL include separate sheets for header and lines
5. WHEN exporting journals, THE Journal Entry System SHALL generate filename with timestamp

### Requirement 11: Journal Entry Number Generation

**User Story:** As a system, I want to auto-generate journal numbers, so that numbering is consistent and sequential

#### Acceptance Criteria

1. WHEN generating journal number, THE Journal Entry System SHALL use format "JE-YYYYMMDD-XXXX"
2. WHEN generating journal number, THE Journal Entry System SHALL ensure uniqueness
3. WHEN generating journal number, THE Journal Entry System SHALL increment sequence number per day
4. WHEN generating journal number, THE Journal Entry System SHALL reset sequence to 0001 each day

### Requirement 12: Journal Entry Audit Trail

**User Story:** As an auditor, I want to see who created, modified, and posted journal entries, so that I can track accountability

#### Acceptance Criteria

1. WHEN creating journal entry, THE Journal Entry System SHALL record created by user and created date
2. WHEN updating journal entry, THE Journal Entry System SHALL record updated by user and updated date
3. WHEN posting journal entry, THE Journal Entry System SHALL record posted by user and posted date
4. WHEN viewing journal detail, THE Journal Entry System SHALL display all audit information
5. WHEN viewing journal list, THE Journal Entry System SHALL show created by and posted by in tooltip or expandable section
