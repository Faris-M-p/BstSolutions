-- TaskManagementSystem patch script
-- Uncomment entries inside each Patch.sql before running.
-- Run with: sqlcmd -S .\SQLEXPRESS -E -d TaskManagementSystem -i Database-Patch.sql
-- Or use SSMS with SQLCMD Mode enabled.

PRINT '=== Patch 01_Tables ===';
GO
:r .\01_Tables\Patch.sql

PRINT '=== Patch 02_Indexes ===';
GO
:r .\02_Indexes\Patch.sql

PRINT '=== Patch 03_SeedData ===';
GO
:r .\03_SeedData\Patch.sql

PRINT '=== Database patch complete ===';
GO
