# BulkIn Desktop - Deployment Guide

**Version:** 1.0.0  
**Date:** November 2, 2025  
**Target Platform:** Windows (10/11/Server 2022)

---

## Deployment Package Creation

### Build Self-Contained Release

This guide provides instructions for creating a production-ready deployment package for BulkIn Desktop.

---

## Option 1: Self-Contained Windows Deployment (Recommended)

### Step 1: Clean Previous Builds

```powershell
cd d:\Project_TextFile\BulkIn\src\BulkIn.Desktop
dotnet clean --configuration Release
```

### Step 2: Publish Self-Contained Package

```powershell
dotnet publish `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output "..\..\publish\BulkIn-Desktop-v1.0.0-win-x64" `
  /p:PublishSingleFile=false `
  /p:PublishReadyToRun=true `
  /p:PublishTrimmed=false
```

**Parameters Explained:**
- `--configuration Release` - Optimized release build
- `--runtime win-x64` - Windows 64-bit target
- `--self-contained true` - Includes .NET runtime (no separate install needed)
- `--output` - Output directory for published files
- `PublishSingleFile=false` - Multiple files (better compatibility with Avalonia)
- `PublishReadyToRun=true` - Pre-compiled for faster startup
- `PublishTrimmed=false` - Keep all assemblies (avoid runtime issues)

### Step 3: Verify Published Files

The output folder should contain:
```
BulkIn-Desktop-v1.0.0-win-x64/
├── BulkIn.Desktop.exe          (Main executable)
├── BulkIn.Core.dll             (Core library)
├── Avalonia.*.dll              (UI framework)
├── Microsoft.*.dll             (Dependencies)
├── appsettings.json            (Configuration)
├── runtimes/                   (Native libraries)
│   └── win-x64/
└── ... (other dependencies)
```

---

## Option 2: Framework-Dependent Deployment

Requires .NET 8.0 Runtime installed on target machine (smaller package size).

```powershell
dotnet publish `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false `
  --output "..\..\publish\BulkIn-Desktop-v1.0.0-win-x64-framework" `
  /p:PublishReadyToRun=true
```

**Package Size Comparison:**
- Self-Contained: ~80-120 MB (includes .NET runtime)
- Framework-Dependent: ~15-25 MB (requires .NET 8.0 on target)

---

## Option 3: Single-File Executable

Creates a single `.exe` file with all dependencies embedded.

```powershell
dotnet publish `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output "..\..\publish\BulkIn-Desktop-v1.0.0-win-x64-single" `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:PublishReadyToRun=true `
  /p:PublishTrimmed=false
```

**Note:** Single-file extraction happens at runtime (first launch slower).

---

## Create Distribution ZIP Package

### PowerShell Script

```powershell
# Navigate to publish directory
cd d:\Project_TextFile\BulkIn\publish

# Create version folder
$version = "BulkIn-Desktop-v1.0.0-win-x64"

# Copy additional files
Copy-Item "..\..\README.md" "$version\"
Copy-Item "..\..\USER_GUIDE.md" "$version\"
Copy-Item "..\..\KEYBOARD_SHORTCUTS.md" "$version\"
Copy-Item "..\..\ACCESSIBILITY.md" "$version\"
Copy-Item "..\..\scripts\DatabaseSetup.sql" "$version\scripts\" -Force

# Create logs folder
New-Item -ItemType Directory -Path "$version\logs" -Force

# Compress to ZIP
Compress-Archive -Path "$version" -DestinationPath "BulkIn-Desktop-v1.0.0-win-x64.zip" -Force

Write-Host "✓ Deployment package created: BulkIn-Desktop-v1.0.0-win-x64.zip"
```

### Package Contents

```
BulkIn-Desktop-v1.0.0-win-x64.zip
├── BulkIn.Desktop.exe
├── [All DLL dependencies]
├── appsettings.json
├── README.md
├── USER_GUIDE.md
├── KEYBOARD_SHORTCUTS.md
├── ACCESSIBILITY.md
├── scripts/
│   └── DatabaseSetup.sql
└── logs/ (empty folder for runtime logs)
```

---

## Deployment Instructions

### For IT Administrators

**1. Extract Package**
```powershell
Expand-Archive -Path "BulkIn-Desktop-v1.0.0-win-x64.zip" -DestinationPath "C:\Program Files\BulkIn"
```

**2. Set Up Database**
- Open SQL Server Management Studio
- Run `scripts\DatabaseSetup.sql` against `RAW_PROCESS` database
- Verify `TextFileData` table created

**3. Configure Application**
- Edit `appsettings.json` or use UI Settings tab
- Update connection string for your environment
- Test database connection

**4. Set Permissions**
```powershell
# Grant users read/execute on installation folder
icacls "C:\Program Files\BulkIn" /grant "Domain Users:(OI)(CI)RX" /T
```

**5. Create Desktop Shortcut (Optional)**
```powershell
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:Public\Desktop\BulkIn.lnk")
$Shortcut.TargetPath = "C:\Program Files\BulkIn\BulkIn.Desktop.exe"
$Shortcut.WorkingDirectory = "C:\Program Files\BulkIn"
$Shortcut.Save()
```

---

## Advanced: Windows Installer (MSI)

For enterprise deployments with Group Policy, consider creating an MSI installer.

### Using WiX Toolset

**1. Install WiX Toolset**
```powershell
dotnet tool install --global wix
```

**2. Create WiX Configuration**

Create `BulkIn.Desktop.wxs`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="BulkIn Desktop" Language="1033" 
           Version="1.0.0.0" Manufacturer="Your Company" 
           UpgradeCode="PUT-GUID-HERE">
    
    <Package InstallerVersion="200" Compressed="yes" 
             InstallScope="perMachine" />

    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate EmbedCab="yes" />

    <Feature Id="ProductFeature" Title="BulkIn Desktop" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>

    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="BulkIn" />
      </Directory>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="ApplicationProgramsFolder" Name="BulkIn"/>
      </Directory>
    </Directory>

    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <!-- Add files here -->
      <Component Id="BulkInExe">
        <File Source="publish\BulkIn.Desktop.exe" KeyPath="yes" />
      </Component>
    </ComponentGroup>

  </Product>
</Wix>
```

**3. Build MSI**
```powershell
wix build BulkIn.Desktop.wxs -out BulkIn-Desktop-v1.0.0.msi
```

---

## Alternative: ClickOnce Deployment

For automatic updates over network share or web server.

### Enable ClickOnce

Add to `BulkIn.Desktop.csproj`:

```xml
<PropertyGroup>
  <PublishUrl>\\server\share\BulkIn\</PublishUrl>
  <Install>true</Install>
  <InstallFrom>Unc</InstallFrom>
  <UpdateEnabled>true</UpdateEnabled>
  <UpdateMode>Foreground</UpdateMode>
  <UpdateInterval>7</UpdateInterval>
  <UpdateIntervalUnits>Days</UpdateIntervalUnits>
  <ApplicationVersion>1.0.0.0</ApplicationVersion>
</PropertyGroup>
```

### Publish ClickOnce

```powershell
dotnet publish /p:PublishProfile=ClickOnce
```

---

## Verification Steps

### Post-Deployment Testing

**1. Smoke Test**
```powershell
# Launch application
Start-Process "C:\Program Files\BulkIn\BulkIn.Desktop.exe"
```

**2. Verify Components**
- [ ] Application launches without errors
- [ ] Main window displays correctly
- [ ] All three tabs accessible (Processing, Settings, Logs)
- [ ] Help → About shows version 1.0.0

**3. Test Database Connection**
- [ ] Navigate to Settings tab
- [ ] Click Test Connection
- [ ] Verify success message

**4. Test File Processing**
- [ ] Browse to test files folder
- [ ] Start processing
- [ ] Monitor progress
- [ ] Verify data in database
- [ ] Check logs for errors

**5. Verify Logs**
- [ ] Navigate to Logs tab
- [ ] Logs display correctly
- [ ] Export logs function works
- [ ] Log files created in `logs/` folder

---

## Rollback Procedure

If deployment fails or issues occur:

**1. Stop Application**
```powershell
Stop-Process -Name "BulkIn.Desktop" -Force -ErrorAction SilentlyContinue
```

**2. Remove Installation**
```powershell
Remove-Item "C:\Program Files\BulkIn" -Recurse -Force
```

**3. Restore Previous Version**
```powershell
# Extract backup
Expand-Archive -Path "BulkIn-Desktop-v0.9.0-win-x64.zip" -DestinationPath "C:\Program Files\BulkIn"
```

**4. Restore Database (if schema changed)**
```sql
-- Restore from backup
RESTORE DATABASE RAW_PROCESS 
FROM DISK = 'C:\Backups\RAW_PROCESS_Backup.bak' 
WITH REPLACE
```

---

## Distribution Checklist

Before distributing to users:

- [ ] Built with Release configuration
- [ ] Tested on clean Windows 10/11 VM
- [ ] Database setup script included
- [ ] User documentation included (USER_GUIDE.md)
- [ ] Keyboard shortcuts reference included
- [ ] appsettings.json reviewed (no hardcoded credentials)
- [ ] Version number correct in About dialog
- [ ] Code signed (optional, for trusted execution)
- [ ] Antivirus scan completed (no false positives)
- [ ] File permissions set correctly
- [ ] Deployment tested in target environment

---

## File Sizes & Requirements

### Published Package Sizes

| Type | Size | .NET Required |
|------|------|---------------|
| Self-Contained | ~95 MB | No |
| Framework-Dependent | ~18 MB | Yes (8.0) |
| Single-File | ~90 MB | No |

### Installation Requirements

**Disk Space:**
- Application: 100-150 MB
- Logs: 10-100 MB (depending on usage)
- Total: ~250 MB recommended

**Database:**
- Initial Schema: <1 MB
- Data Growth: Variable (depends on file volume)
- Recommend: 10 GB+ for database

---

## Code Signing (Optional)

For trusted execution without SmartScreen warnings:

**1. Obtain Code Signing Certificate**
- Purchase from CA (DigiCert, Sectigo, etc.)
- Or use internal PKI certificate

**2. Sign Executable**
```powershell
signtool sign /f "certificate.pfx" /p "password" `
  /t http://timestamp.digicert.com `
  /fd SHA256 `
  "BulkIn.Desktop.exe"
```

**3. Verify Signature**
```powershell
signtool verify /pa "BulkIn.Desktop.exe"
```

---

## Deployment Scenarios

### Scenario 1: Single Workstation

**Target:** 1 user, local SQL Server Express

**Steps:**
1. Extract to `C:\Program Files\BulkIn`
2. Run database setup against `.\SQLEXPRESS`
3. Configure connection in UI
4. Create desktop shortcut

**Estimated Time:** 15 minutes

---

### Scenario 2: Multiple Workstations (Network Database)

**Target:** 10+ users, shared SQL Server

**Steps:**
1. Deploy application to each workstation (or network share)
2. Run database setup once on shared SQL Server
3. Distribute preconfigured `appsettings.json` with connection string
4. Use Group Policy to deploy shortcut

**Estimated Time:** 30 minutes + 2 minutes per workstation

---

### Scenario 3: Terminal Server / Citrix

**Target:** Multiple users, published application

**Steps:**
1. Install on Terminal Server
2. Configure connection to backend SQL Server
3. Publish via RemoteApp or Citrix
4. Test multi-session behavior

**Notes:**
- Each user session runs independently
- Verify SQL Server connection pooling
- Monitor server resources (RAM/CPU)

**Estimated Time:** 45 minutes

---

## Support & Maintenance

### Update Procedure

**1. Build New Version**
```powershell
dotnet publish --configuration Release --runtime win-x64 --self-contained true
```

**2. Test Update**
- Test on dev machine
- Verify database compatibility
- Test file processing

**3. Deploy Update**
- Notify users of maintenance window
- Stop running instances
- Replace executable and DLLs
- Restart application

**4. Verify**
- Check version in About dialog
- Test core functionality
- Monitor logs for errors

---

## Production Readiness Checklist

- [x] Code reviewed and approved
- [x] Unit tests passed
- [x] Integration tests passed
- [x] Performance tests passed
- [x] Security review completed
- [x] Documentation complete
- [x] User training materials ready
- [x] Database backup plan in place
- [x] Rollback procedure documented
- [x] Support team notified
- [x] Monitoring configured (if applicable)
- [x] Production environment validated

---

## Technical Support

**For deployment issues:**
- Email: support@bulkin.local
- Phone: Extension 1234
- Documentation: See USER_GUIDE.md

**For database issues:**
- Contact SQL Server DBA team
- Review DatabaseSetup.sql
- Check connection string

**For application issues:**
- Review logs in `logs/` folder
- Check TESTING_REPORT.md
- See TROUBLESHOOTING section in USER_GUIDE.md

---

## Appendix: PowerShell Deployment Script

Complete automated deployment script:

```powershell
# BulkIn Desktop Deployment Script
# Version: 1.0.0

param(
    [string]$InstallPath = "C:\Program Files\BulkIn",
    [string]$SourceZip = "BulkIn-Desktop-v1.0.0-win-x64.zip",
    [switch]$CreateShortcut
)

Write-Host "BulkIn Desktop Deployment Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Please run as Administrator!"
    exit 1
}

# Extract package
Write-Host "`nExtracting package..." -ForegroundColor Yellow
Expand-Archive -Path $SourceZip -DestinationPath $InstallPath -Force

# Set permissions
Write-Host "Setting permissions..." -ForegroundColor Yellow
icacls $InstallPath /grant "Users:(OI)(CI)RX" /T | Out-Null

# Create logs folder
Write-Host "Creating logs folder..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "$InstallPath\logs" -Force | Out-Null

# Create desktop shortcut
if ($CreateShortcut) {
    Write-Host "Creating desktop shortcut..." -ForegroundColor Yellow
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$env:Public\Desktop\BulkIn.lnk")
    $Shortcut.TargetPath = "$InstallPath\BulkIn.Desktop.exe"
    $Shortcut.WorkingDirectory = $InstallPath
    $Shortcut.Description = "BulkIn File Bulk Insert Manager"
    $Shortcut.Save()
}

Write-Host "`n✓ Deployment completed successfully!" -ForegroundColor Green
Write-Host "`nInstallation Location: $InstallPath" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Run scripts\DatabaseSetup.sql in SQL Server" -ForegroundColor White
Write-Host "  2. Configure connection string in Settings tab" -ForegroundColor White
Write-Host "  3. Test database connection" -ForegroundColor White
Write-Host "  4. Start processing files" -ForegroundColor White
```

**Usage:**
```powershell
.\Deploy-BulkIn.ps1 -CreateShortcut
```

---

*End of Deployment Guide*
