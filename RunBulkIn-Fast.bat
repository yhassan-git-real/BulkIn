@echo off
REM ============================================
REM BulkIn - Direct Executable Launcher
REM ============================================

REM Set console to UTF-8 for emoji support
chcp 65001 > nul

REM Set title
title BulkIn - Bulk Text File Data Ingestion System

REM Change to the executable directory
cd /d "%~dp0src\BulkIn.Console\bin\Debug\net8.0"

REM Check if executable exists
if not exist "BulkIn.Console.exe" (
    echo Error: BulkIn.Console.exe not found!
    echo Please build the project first using: dotnet build
    echo.
    pause
    exit /b 1
)

REM Run the executable
BulkIn.Console.exe

REM Keep console open if there was an error
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error occurred! Exit code: %ERRORLEVEL%
    pause
)
