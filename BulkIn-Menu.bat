@echo off
chcp 65001 > nul
title BulkIn - Launcher Menu

:MENU
cls
echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║                    BulkIn v1.0                             ║
echo ║         Bulk Text File Data Ingestion System               ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo  Select an option:
echo.
echo  [1] 🚀 Run BulkIn (Quick Start)
echo  [2] 🔨 Build and Run (Optimized)
echo  [3] ⚙️  Build Only
echo  [4] 📂 Open Logs Folder
echo  [5] 📝 Open Configuration File
echo  [6] 📖 View Documentation
echo  [7] ❌ Exit
echo.
echo ═══════════════════════════════════════════════════════════
echo.

set /p choice="Enter your choice [1-7]: "

if "%choice%"=="1" goto RUN
if "%choice%"=="2" goto BUILD_RUN
if "%choice%"=="3" goto BUILD
if "%choice%"=="4" goto LOGS
if "%choice%"=="5" goto CONFIG
if "%choice%"=="6" goto DOCS
if "%choice%"=="7" goto EXIT

echo Invalid choice! Please select 1-7.
timeout /t 2 > nul
goto MENU

:RUN
cls
echo.
echo Starting BulkIn...
echo.
cd /d "%~dp0src\BulkIn"
dotnet run
pause
goto MENU

:BUILD_RUN
cls
echo.
echo Building and running BulkIn...
echo.
call "%~dp0BuildAndRun.bat"
goto MENU

:BUILD
cls
echo.
echo Building BulkIn...
echo.
cd /d "%~dp0"
dotnet build --configuration Release
echo.
echo Build complete!
pause
goto MENU

:LOGS
cls
echo.
echo Opening logs folder...
if exist "%~dp0logs" (
    start "" "%~dp0logs"
) else (
    echo Logs folder not found. It will be created when you run the application.
    pause
)
goto MENU

:CONFIG
cls
echo.
echo Opening configuration file...
if exist "%~dp0src\BulkIn\appsettings.json" (
    start "" notepad "%~dp0src\BulkIn\appsettings.json"
) else (
    echo Configuration file not found!
    pause
)
goto MENU

:DOCS
cls
echo.
echo Available Documentation:
echo.
echo [1] README.md - Project Overview
echo [2] QUICKSTART.md - 5-Minute Setup Guide
echo [3] LAUNCHER_GUIDE.md - Batch File Usage
echo [4] MVP_COMPLETE_SUMMARY.md - Complete Feature List
echo [5] Back to Main Menu
echo.

set /p docchoice="Select documentation [1-5]: "

if "%docchoice%"=="1" start "" "%~dp0README.md"
if "%docchoice%"=="2" start "" "%~dp0QUICKSTART.md"
if "%docchoice%"=="3" start "" "%~dp0LAUNCHER_GUIDE.md"
if "%docchoice%"=="4" start "" "%~dp0MVP_COMPLETE_SUMMARY.md"
if "%docchoice%"=="5" goto MENU

goto MENU

:EXIT
cls
echo.
echo Thank you for using BulkIn!
echo.
timeout /t 2 > nul
exit
