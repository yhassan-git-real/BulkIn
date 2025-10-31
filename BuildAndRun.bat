@echo off
REM ============================================
REM BulkIn - Build and Run
REM ============================================

REM Set console to UTF-8 for emoji support
chcp 65001 > nul

REM Set title
title BulkIn - Building...

echo.
echo ════════════════════════════════════════════════════════════
echo  BulkIn - Build and Run
echo ════════════════════════════════════════════════════════════
echo.

REM Change to the project directory
cd /d "%~dp0"

echo Building project...
dotnet build --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Exit code: %ERRORLEVEL%
    pause
    exit /b 1
)

echo.
echo Build successful!
echo.
echo Starting BulkIn application...
echo.

REM Change to the release executable directory
cd /d "%~dp0src\BulkIn\bin\Release\net8.0"

REM Update title
title BulkIn v1.0

REM Run the executable
BulkIn.exe

REM Keep console open if there was an error
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error occurred! Exit code: %ERRORLEVEL%
    pause
)
