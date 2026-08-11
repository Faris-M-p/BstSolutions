@echo off
setlocal EnableExtensions

REM Always run from this script's folder (works even if the project is moved).
cd /d "%~dp0"

REM Easy-to-change connection settings (matches appsettings.json).
set SERVER=(localdb)\MSSQLLocalDB
set DATABASE=TaskManagementSystem

echo.
echo ==========================================
echo TaskManagementSystem - Database Patch
echo Server  : %SERVER%
echo Database: %DATABASE%
echo ==========================================
echo.
echo This script patches an EXISTING database only.
echo It never recreates the database and never runs Database.sql.
echo.

where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo sqlcmd is not installed or not available in PATH.
    echo Verify installation with: sqlcmd -?
    echo.
    pause
    exit /b 1
)

echo Running Database-Patch.sql against [%DATABASE%] ...
sqlcmd -S "%SERVER%" -E -d "%DATABASE%" -b -i "Database-Patch.sql"
if errorlevel 1 (
    echo.
    echo ==========================================
    echo DATABASE PATCH FAILED
    echo ==========================================
    echo.
    pause
    exit /b 1
)

echo.
echo ==========================================
echo DATABASE PATCH COMPLETED SUCCESSFULLY
echo ==========================================
echo.
pause
exit /b 0
