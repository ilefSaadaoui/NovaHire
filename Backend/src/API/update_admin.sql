-- Update the platform SuperAdmin to the correct email
UPDATE "Users"
SET "Email" = 'adminplatforme@gmail.com',
    "FirstName" = 'Admin',
    "LastName" = 'Plateforme'
WHERE "Id" = '22222222-2222-2222-2222-222222222222';

-- Remove any other SuperAdmin accounts (Role = 0) that are NOT this one
DELETE FROM "Users"
WHERE "Role" = 0
  AND "Id" != '22222222-2222-2222-2222-222222222222';

-- Confirm result
SELECT "Id", "Email", "FirstName", "LastName", "Role"
FROM "Users"
WHERE "Role" = 0;
