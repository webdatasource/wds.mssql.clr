@echo off
REM ===========================================================================
REM Checks if SERVER env var is set; if not, defaults to "localhost"
REM ===========================================================================
if not defined SERVER (
    set "SERVER=localhost"
)

REM ===========================================================================
REM 1. Test Windows Authentication with a simple query
REM ===========================================================================
echo Checking Windows Authentication on server: %SERVER%
sqlcmd -S "%SERVER%" -E -Q "SELECT 1" >NUL 2>&1

if %ERRORLEVEL% EQU 0 (
    echo Windows Authentication successful. Proceeding with integrated security...
    echo.
    sqlcmd -S "%SERVER%" -E -i "WdsClrFunctions.sql" -v BasePath="%CD%"
) else (
    echo Windows Authentication failed. Please enter your SQL Server credentials:
    set /p SQLUSER=Username: 
    set /p SQLPASS=Password: 
    echo.
    sqlcmd -S "%SERVER%" -U "%SQLUSER%" -P "%SQLPASS%" -i "WdsClrFunctions.sql" -v BasePath="%CD%"
)

echo All done. The MS SQL instance on "%SERVER%" is ready to run WDS CLR functions.