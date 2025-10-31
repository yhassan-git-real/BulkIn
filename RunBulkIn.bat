@echo off
REM ============================================
REM BulkIn - Application Launcher
REM ============================================

REM Set console to UTF-8 for emoji support
chcp 65001 > nul

REM Change to the project directory
cd /d "%~dp0src\BulkIn"

REM Run the application
dotnet run

REM Pause only if there was an error
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error occurred! Exit code: %ERRORLEVEL%
    pause
)
