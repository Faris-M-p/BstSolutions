/*
    Database.sql
    ------------------------------------------------------------
    Creates a NEW database using the latest master definitions.
    Does NOT depend on historical patches.

    Run with sqlcmd from the Database folder:

      sqlcmd -S .\SQLEXPRESS -E -i Database.sql

    Or from SSMS with SQLCMD Mode enabled.
*/

:setvar DatabaseName "TaskManagementSystem"

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    DECLARE @createSql nvarchar(200) = N'CREATE DATABASE [' + N'$(DatabaseName)' + N']';
    EXEC (@createSql);
END
GO

USE [$(DatabaseName)];
GO

PRINT 'Applying latest master table definitions...';
GO
:r .\01_Tables\Employees.sql
:r .\01_Tables\WorkTasks.sql

PRINT 'Applying latest master index definitions...';
GO
:r .\02_Indexes\WorkTasks.sql

PRINT 'Applying latest master seed data...';
GO
:r .\03_SeedData\Seed.sql

PRINT 'Database.sql completed successfully.';
GO
