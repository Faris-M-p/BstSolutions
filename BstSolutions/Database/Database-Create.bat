@echo off
setlocal EnableExtensions

REM Always run from this script's folder (works even if the project is moved).
cd /d "%~dp0"

REM Easy-to-change connection settings (matches appsettings.json).
set SERVER=(localdb)\MSSQLLocalDB
set DATABASE=TaskManagementSystem

echo.
echo ==========================================
echo TaskManagementSystem - Database Create
echo Server  : %SERVER%
echo Database: %DATABASE%
echo ==========================================
echo.

where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo sqlcmd is not installed or not available in PATH.
    echo Verify installation with: sqlcmd -?
    echo.
    pause
    exit /b 1
)

echo Running Database.sql ...
sqlcmd -S "%SERVER%" -E -b -i "Database.sql"
if errorlevel 1 (
    echo.
    echo ==========================================
    echo DATABASE CREATION FAILED
    echo ==========================================
    echo.
    pause
    exit /b 1
)

echo.
echo ==========================================
echo DATABASE CREATED SUCCESSFULLY
echo ==========================================
echo.
pause
exit /b 0
