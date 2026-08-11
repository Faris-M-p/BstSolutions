/*
    Seed data - master seed for a brand-new database.
    Kept simple and idempotent for the machine test.
*/

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Email = N'jane.doe@example.com')
BEGIN
    INSERT INTO dbo.Employees (FirstName, LastName, Email, IsActive, CreatedDate)
    VALUES (N'Jane', N'Doe', N'jane.doe@example.com', 1, SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Email = N'john.smith@example.com')
BEGIN
    INSERT INTO dbo.Employees (FirstName, LastName, Email, IsActive, CreatedDate)
    VALUES (N'John', N'Smith', N'john.smith@example.com', 1, SYSUTCDATETIME());
END
GO
