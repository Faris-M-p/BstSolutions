/*
    Database.sql
    ------------------------------------------------------------
    Master entry point for creating a NEW database.
    Executes the latest master SQL files only.
    Does NOT include Patch.sql files.
    Does NOT depend on Database-Patch.sql.

    Preferred: double-click Database-Create.bat

    SeedData.sql inserts the demo admin user with a PasswordHasher hash
    (never plain-text password).
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
:r .\01_Tables\ApplicationUsers.sql
:r .\01_Tables\Employees.sql
:r .\01_Tables\WorkTasks.sql

PRINT 'Applying latest master index definitions...';
GO
:r .\02_Indexes\Indexes.sql

PRINT 'Applying latest master seed data...';
GO
:r .\03_SeedData\SeedData.sql

PRINT 'Database.sql completed successfully.';
GO
