@echo off
REM ============================================
REM Create Desktop Shortcut for BulkIn
REM ============================================

echo.
echo Creating desktop shortcut for BulkIn...
echo.

REM Get the current directory
set SCRIPT_DIR=%~dp0

REM Create VBScript to generate shortcut
echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%USERPROFILE%\Desktop\BulkIn.lnk" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.TargetPath = "%SCRIPT_DIR%BulkIn-Menu.bat" >> CreateShortcut.vbs
echo oLink.WorkingDirectory = "%SCRIPT_DIR%" >> CreateShortcut.vbs
echo oLink.Description = "BulkIn - Bulk Text File Data Ingestion System" >> CreateShortcut.vbs
echo oLink.IconLocation = "shell32.dll,21" >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs

REM Execute the VBScript
cscript //nologo CreateShortcut.vbs

REM Clean up
del CreateShortcut.vbs

echo.
echo ✓ Desktop shortcut created successfully!
echo   Location: %USERPROFILE%\Desktop\BulkIn.lnk
echo.
echo You can now double-click the "BulkIn" icon on your desktop.
echo.

pause
