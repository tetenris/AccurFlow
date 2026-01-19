-- Fix Audit Trail: Convert User IDs to User Names
-- Run this script to update existing audit trail data

-- 1. Update Customers table
PRINT 'Updating Customers audit trail...'

-- Update CreatedBy
UPDATE c
SET c.CreatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM Customers c
LEFT JOIN Users u ON TRY_CAST(c.CreatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE c.CreatedBy IS NOT NULL 
  AND (LEN(c.CreatedBy) = 36 OR c.CreatedBy = '00000000-0000-0000-0000-000000000000')
  AND c.CreatedBy != 'system';

-- Update UpdatedBy
UPDATE c
SET c.UpdatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM Customers c
LEFT JOIN Users u ON TRY_CAST(c.UpdatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE c.UpdatedBy IS NOT NULL 
  AND (LEN(c.UpdatedBy) = 36 OR c.UpdatedBy = '00000000-0000-0000-0000-000000000000')
  AND c.UpdatedBy != 'system';

PRINT 'Customers audit trail updated!'

-- 2. Update ChartOfAccounts table (if needed)
PRINT 'Updating ChartOfAccounts audit trail...'

UPDATE coa
SET coa.CreatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM ChartOfAccounts coa
LEFT JOIN Users u ON TRY_CAST(coa.CreatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE coa.CreatedBy IS NOT NULL 
  AND (LEN(coa.CreatedBy) = 36 OR coa.CreatedBy = '00000000-0000-0000-0000-000000000000')
  AND coa.CreatedBy != 'system';

UPDATE coa
SET coa.UpdatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM ChartOfAccounts coa
LEFT JOIN Users u ON TRY_CAST(coa.UpdatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE coa.UpdatedBy IS NOT NULL 
  AND (LEN(coa.UpdatedBy) = 36 OR coa.UpdatedBy = '00000000-0000-0000-0000-000000000000')
  AND coa.UpdatedBy != 'system';

PRINT 'ChartOfAccounts audit trail updated!'

-- 3. Update Roles table (if needed)
PRINT 'Updating Roles audit trail...'

UPDATE r
SET r.CreatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM Roles r
LEFT JOIN Users u ON TRY_CAST(r.CreatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE r.CreatedBy IS NOT NULL 
  AND (LEN(r.CreatedBy) = 36 OR r.CreatedBy = '00000000-0000-0000-0000-000000000000')
  AND r.CreatedBy != 'system';

UPDATE r
SET r.UpdatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM Roles r
LEFT JOIN Users u ON TRY_CAST(r.UpdatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE r.UpdatedBy IS NOT NULL 
  AND (LEN(r.UpdatedBy) = 36 OR r.UpdatedBy = '00000000-0000-0000-0000-000000000000')
  AND r.UpdatedBy != 'system';

PRINT 'Roles audit trail updated!'

-- 4. Update JournalEntries table (if needed)
PRINT 'Updating JournalEntries audit trail...'

UPDATE je
SET je.CreatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM JournalEntries je
LEFT JOIN Users u ON TRY_CAST(je.CreatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE je.CreatedBy IS NOT NULL 
  AND (LEN(je.CreatedBy) = 36 OR je.CreatedBy = '00000000-0000-0000-0000-000000000000')
  AND je.CreatedBy != 'system';

UPDATE je
SET je.UpdatedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM JournalEntries je
LEFT JOIN Users u ON TRY_CAST(je.UpdatedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE je.UpdatedBy IS NOT NULL 
  AND (LEN(je.UpdatedBy) = 36 OR je.UpdatedBy = '00000000-0000-0000-0000-000000000000')
  AND je.UpdatedBy != 'system';

UPDATE je
SET je.PostedBy = COALESCE(u.FullName, u.UserName, 'System')
FROM JournalEntries je
LEFT JOIN Users u ON TRY_CAST(je.PostedBy AS UNIQUEIDENTIFIER) = u.UserId
WHERE je.PostedBy IS NOT NULL 
  AND (LEN(je.PostedBy) = 36 OR je.PostedBy = '00000000-0000-0000-0000-000000000000')
  AND je.PostedBy != 'system';

PRINT 'JournalEntries audit trail updated!'

-- Verify results
PRINT ''
PRINT '=== VERIFICATION ==='
PRINT 'Customers with GUID in audit fields:'
SELECT COUNT(*) as Count FROM Customers 
WHERE (LEN(CreatedBy) = 36 OR LEN(UpdatedBy) = 36) 
  AND CreatedBy != 'system' AND UpdatedBy != 'system';

PRINT 'Sample Customers audit trail:'
SELECT TOP 5 CustomerCode, CustomerName, CreatedBy, UpdatedBy 
FROM Customers 
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

PRINT ''
PRINT 'Script completed successfully!'
