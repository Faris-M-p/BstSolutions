/*
    Database-Patch.sql
    ------------------------------------------------------------
    Master entry point for updating an EXISTING database.
    Executes only active Patch.sql files.
    Does NOT recreate the database.
    Does NOT execute master SQL files.

    Preferred: double-click Database-Patch.bat

    Patch lifecycle:
    1. Update master SQL file(s)
    2. Add upgrade SQL / uncomment :r in the folder Patch.sql
    3. Run Database-Patch.bat
    4. After deploy, remove/comment completed patch entries
*/

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
