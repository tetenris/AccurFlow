-- Clear and Reseed Customers
-- This will delete all existing customers and reseed with proper audit trail

PRINT 'Clearing existing customers...'
DELETE FROM Customers;
PRINT 'Customers cleared!'

PRINT ''
PRINT 'Now run the application or execute the seeder to insert fresh data with proper audit trail.'
PRINT 'Or run the INSERT statements from run-customer-seed.sql'
