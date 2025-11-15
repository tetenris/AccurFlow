-- Update existing roles with proper RoleType based on RoleName
-- Run this script in SQL Server Management Studio or via dotnet ef

UPDATE Roles
SET RoleType = 1
WHERE RoleName LIKE '%Administrator%' AND RoleType = 0;

UPDATE Roles
SET RoleType = 2
WHERE RoleName LIKE '%Accountant%' AND RoleType = 0;

UPDATE Roles
SET RoleType = 3
WHERE RoleName LIKE '%Manager%' AND RoleType = 0;

UPDATE Roles
SET RoleType = 4
WHERE RoleName LIKE '%User%' AND RoleType = 0;

-- Verify the update
SELECT RoleId, RoleType, RoleName, Description, IsActive
FROM Roles
WHERE IsDeleted = 0
ORDER BY RoleType;
