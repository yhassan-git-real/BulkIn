# BulkIn Desktop - User Guide

**Version:** 1.0.0  
**Release Date:** November 2, 2025  
**Platform:** Windows (10/11/Server 2022)  
**Framework:** .NET 8.0

---

## Table of Contents

1. [Introduction](#introduction)
2. [System Requirements](#system-requirements)
3. [Installation](#installation)
4. [Quick Start Guide](#quick-start-guide)
5. [Application Overview](#application-overview)
6. [Settings Configuration](#settings-configuration)
7. [Processing Files](#processing-files)
8. [Viewing Logs](#viewing-logs)
9. [Keyboard Shortcuts](#keyboard-shortcuts)
10. [Troubleshooting](#troubleshooting)
11. [FAQ](#faq)
12. [Support](#support)

---

## Introduction

**BulkIn Desktop** is a high-performance Windows application designed to efficiently bulk insert text file data into SQL Server databases. Built with modern Avalonia UI framework, it provides an intuitive interface for processing large volumes of text files with real-time monitoring and comprehensive logging.

### Key Features

✅ **Batch Processing** - Process multiple text files simultaneously  
✅ **Real-Time Monitoring** - Track progress with live statistics and progress bars  
✅ **Pause & Resume** - Full control over processing operations  
✅ **Comprehensive Logging** - Detailed logs with filtering and export capabilities  
✅ **Keyboard Shortcuts** - Efficient navigation with keyboard-first design  
✅ **Accessibility** - WCAG 2.1 AA compliant with screen reader support  
✅ **High Performance** - Processes 12,500+ lines per second

---

## System Requirements

### Minimum Requirements

- **Operating System:** Windows 10 (version 1809 or later)
- **Processor:** 2 GHz dual-core processor
- **Memory:** 4 GB RAM
- **Storage:** 100 MB available space
- **Database:** SQL Server 2016 or later
- **Framework:** .NET 8.0 Runtime (included in installer)

### Recommended Requirements

- **Operating System:** Windows 11 or Windows Server 2022
- **Processor:** 3 GHz quad-core processor
- **Memory:** 8 GB RAM or higher
- **Storage:** 500 MB available space (for logs and temporary files)
- **Database:** SQL Server 2019 or later
- **Display:** 1920×1080 resolution or higher

---

## Installation

### Step 1: Download the Application

1. Download the BulkIn Desktop installer package
2. Extract the ZIP file to a temporary location
3. Locate `BulkIn.Desktop.exe` in the extracted folder

### Step 2: Verify .NET 8.0 Runtime

The application requires .NET 8.0 Runtime. To check if installed:

```powershell
dotnet --version
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Step 3: Database Setup

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Locate the database setup script: `scripts/DatabaseSetup.sql`
4. Open the script in SSMS
5. Execute the script to create required tables and stored procedures

The script will create:
- `TextFileData` table (target table for processed data)
- `TempTextFileData` table (staging table, auto-managed)
- `usp_TransferDataFromTemp` stored procedure

### Step 4: Configure Application Settings

1. Launch `BulkIn.Desktop.exe`
2. Navigate to the **Settings** tab
3. Configure database connection (see [Settings Configuration](#settings-configuration))
4. Click **Test Connection** to verify
5. Click **Save Settings**

---

## Quick Start Guide

### Process Your First File (5 Minutes)

**Step 1: Configure Database Connection**

1. Launch BulkIn Desktop
2. Click **Settings** tab (or press `Ctrl+2`)
3. Enter your database connection details:
   - **Server:** `localhost` (or your SQL Server address)
   - **Database:** `RAW_PROCESS`
   - **Use Windows Authentication:** ✅ (recommended)
4. Click **Test Connection** ✅
5. Click **Save Settings** (or press `Ctrl+S`)

**Step 2: Select Files**

1. Click **Processing** tab (or press `Ctrl+1`)
2. Click **Browse Files** button
3. Navigate to your text files folder (e.g., `C:\Data\TextFiles`)
4. Select the folder containing `.txt` files
5. Files will appear in the queue

**Step 3: Start Processing**

1. Review the file list
2. Click **Start Processing** (or press `F5`)
3. Monitor progress in real-time:
   - Progress bar shows overall completion
   - Statistics display: Total lines, success rate, speed
   - Current file being processed
4. Wait for completion notification

**Step 4: Verify Results**

1. Click **Logs** tab (or press `Ctrl+3`) to review processing logs
2. Open SQL Server Management Studio
3. Query the target table:

```sql
SELECT TOP 100 * FROM RAW_PROCESS.dbo.TextFileData
ORDER BY Date DESC
```

✅ **Done!** Your files have been successfully processed.

---

## Application Overview

### Main Window Layout

```
┌─────────────────────────────────────────────────────────┐
│ Menu Bar: [Help]                                        │
├─────────────────────────────────────────────────────────┤
│ 📦 BulkIn - File Bulk Insert Manager                   │
├─────────────────────────────────────────────────────────┤
│ [📊 Processing] [⚙️ Settings] [📝 Logs]                │
│                                                         │
│                   TAB CONTENT AREA                      │
│                                                         │
├─────────────────────────────────────────────────────────┤
│ ● Ready  | 💡 Shortcuts | BulkIn v1.0                 │
└─────────────────────────────────────────────────────────┘
```

### Three Main Tabs

1. **📊 Processing Tab**
   - File selection and queue management
   - Start/Pause/Stop controls
   - Real-time progress monitoring
   - Live statistics display

2. **⚙️ Settings Tab**
   - Database connection configuration
   - File processing settings
   - Batch size and performance tuning
   - Logging preferences

3. **📝 Logs Tab**
   - Real-time log viewer
   - Filter by severity (Info/Warning/Error)
   - Search functionality
   - Export logs to file

---

## Settings Configuration

### Database Settings

Navigate to **Settings** tab → **Database Configuration**

**Connection String**
- **Server/Instance:** Your SQL Server address
  - Examples: `localhost`, `.\SQLEXPRESS`, `192.168.1.100`
- **Database Name:** Target database (default: `RAW_PROCESS`)
- **Authentication:**
  - ✅ **Windows Authentication** (Recommended for domain users)
  - ☐ **SQL Server Authentication** (Requires username/password)

**Advanced Options**
- **Connection Timeout:** 30 seconds (default)
- **Trust Server Certificate:** ✅ (for self-signed certificates)
- **Encrypt Connection:** ✅ (recommended for production)

**Testing Connection**
1. Click **Test Connection** button
2. Wait for validation (2-5 seconds)
3. ✅ Green checkmark = Success
4. ❌ Red X = Failed (check error message)

### File Processing Settings

**Source Folder**
- Click **Browse** to select folder containing text files
- Application processes all `.txt` files in the folder
- Subfolders are not included (flat directory only)

**Processing Options**
- **Batch Size:** Number of rows per batch insert (default: 10,000)
  - Smaller batches: More frequent commits, slower overall
  - Larger batches: Fewer commits, faster but more memory
  - Recommended: 5,000 - 50,000 depending on line length

- **Delete After Processing:** 
  - ✅ Move processed files to archive folder
  - ☐ Keep original files

- **Archive Folder:** Location for processed files (if enabled)

### Logging Settings

- **Log Level:** 
  - `Information` (default) - All messages
  - `Warning` - Warnings and errors only
  - `Error` - Errors only

- **Log to File:** ✅ Enable file logging
- **Log Folder:** `logs/` (default)
- **Max Log File Size:** 10 MB (auto-rotate)

### Saving Settings

1. Make your changes
2. Click **Save Settings** (or press `Ctrl+S`)
3. Settings saved to `appsettings.json`
4. Confirmation message appears

**Reset to Defaults**
- Click **Reset** button to restore default settings
- Confirmation dialog will appear

---

## Processing Files

### Preparing Files

**File Requirements**
- ✅ Format: Plain text files (`.txt`)
- ✅ Encoding: UTF-8, UTF-16, or ASCII
- ✅ Structure: One record per line
- ⚠️ Size: Up to 500 MB per file (larger files split automatically)
- ❌ Binary files not supported

**File Naming**
- No specific naming convention required
- Filename will be stored in database for tracking
- Avoid special characters: `< > : " / \ | ? *`

### Starting Processing

**Method 1: Button Click**
1. Select files via **Browse Files** button
2. Review file queue
3. Click **Start Processing** button

**Method 2: Keyboard Shortcut**
1. Select files
2. Press `F5` to start

### Monitoring Progress

**Progress Bar**
- Blue bar indicates overall completion percentage
- Updates in real-time as files process

**Statistics Panel**
- **Total Lines:** Number of lines processed so far
- **Success Rate:** Percentage of successfully inserted records
- **Processing Speed:** Lines per second
- **Current File:** Name of file being processed
- **Estimated Time:** Time remaining (calculated dynamically)

**File Queue**
- ✅ Green checkmark = Completed
- 🔄 Blue spinner = Currently processing
- ⏸️ Gray = Queued/Paused
- ❌ Red X = Failed

### Pausing & Resuming

**To Pause:**
- Click **Pause** button (or press `Ctrl+P`)
- Current batch completes before pausing
- Progress saved automatically

**To Resume:**
- Click **Resume** button (or press `F5`)
- Processing continues from where it left off

**To Stop:**
- Click **Stop** button
- Confirmation dialog appears
- Already processed data remains in database
- Unprocessed files remain in queue

### Handling Errors

**If a File Fails:**
1. Error logged to Logs tab
2. File marked with ❌ red X in queue
3. Processing continues with next file
4. Review error message for details

**Common Errors:**
- Database connection lost → Check network/SQL Server
- File locked by another process → Close other applications
- Invalid file format → Verify file is plain text
- Permission denied → Run as Administrator

---

## Viewing Logs

### Log Tab Overview

Navigate to **Logs** tab (or press `Ctrl+3`)

**Log Entry Format:**
```
[2025-11-02 14:35:22] [INFO] Processing started for file: data.txt
[2025-11-02 14:35:25] [INFO] Batch inserted: 10,000 rows
[2025-11-02 14:35:30] [SUCCESS] File processed: data.txt (50,000 lines)
```

### Filtering Logs

**By Severity:**
- Click **Info** button to toggle Information messages
- Click **Warning** button to toggle Warning messages
- Click **Error** button to toggle Error messages
- Multiple filters can be active simultaneously

**By Text Search:**
1. Enter search term in search box
2. Logs filter in real-time
3. Case-insensitive search
4. Searches entire log message

### Exporting Logs

1. Click **Export** button
2. Choose save location
3. Logs saved as `.txt` file with timestamp
4. Example: `SuccessLog_20251102_143522.txt`

### Clearing Logs

1. Click **Clear** button
2. Confirmation dialog appears
3. All logs removed from display
4. Previously exported logs retained

---

## Keyboard Shortcuts

### Global Shortcuts

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Ctrl+1` | Switch to Processing | Navigate to the Processing tab |
| `Ctrl+2` | Switch to Settings | Navigate to the Settings tab |
| `Ctrl+3` | Switch to Logs | Navigate to the Logs tab |
| `F1` | Show About | Display application information |

### Processing View

| Shortcut | Action | Description |
|----------|--------|-------------|
| `F5` | Start/Resume Processing | Begin or resume file processing |
| `Ctrl+P` | Pause Processing | Pause the current operation |
| `Esc` | Stop Processing | Stop processing (with confirmation) |

### Settings View

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Ctrl+S` | Save Settings | Save all configuration changes |
| `Ctrl+R` | Reset Settings | Restore default settings |

### General Navigation

| Shortcut | Action |
|----------|--------|
| `Tab` | Move to next control |
| `Shift+Tab` | Move to previous control |
| `Enter` | Activate focused button |
| `Escape` | Close dialogs |
| `Alt+F4` | Close application |

💡 **Tip:** Hover over any button to see its keyboard shortcut in the tooltip!

---

## Troubleshooting

### Application Won't Start

**Problem:** Double-clicking `.exe` does nothing

**Solutions:**
1. Verify .NET 8.0 Runtime is installed:
   ```powershell
   dotnet --version
   ```
2. Right-click → Run as Administrator
3. Check Windows Event Viewer for error details
4. Disable antivirus temporarily to rule out interference

---

### Database Connection Fails

**Problem:** "Failed to connect to database" error

**Solutions:**

1. **Verify SQL Server is running:**
   ```powershell
   Get-Service -Name MSSQLSERVER
   ```

2. **Check connection string:**
   - Server name correct? (Use `.\SQLEXPRESS` for local Express)
   - Database exists? (Create if needed)
   - Firewall blocking port 1433?

3. **Test connection manually:**
   - Open SSMS with same credentials
   - If SSMS connects but app doesn't, check Trusted_Connection setting

4. **Enable TCP/IP protocol:**
   - Open SQL Server Configuration Manager
   - SQL Server Network Configuration → Protocols
   - Enable TCP/IP
   - Restart SQL Server service

---

### Files Not Processing

**Problem:** Click Start but nothing happens

**Solutions:**

1. **Verify files selected:**
   - File queue should show `.txt` files
   - Empty folder? No files found message appears

2. **Check file permissions:**
   - Right-click folder → Properties → Security
   - Ensure Read permission enabled

3. **File in use by another program:**
   - Close Excel, Notepad, or other applications
   - Check Task Manager for file locks

4. **Review logs:**
   - Switch to Logs tab
   - Look for error messages with details

---

### Slow Processing Speed

**Problem:** Processing slower than expected (<1,000 lines/sec)

**Solutions:**

1. **Increase batch size:**
   - Settings → Batch Size → Try 25,000 or 50,000
   - Larger batches = fewer round-trips to database

2. **Check database performance:**
   - SQL Server CPU/Memory usage high?
   - Run `sp_who2` to check for blocking
   - Rebuild indexes on TextFileData table

3. **Disable real-time antivirus scanning:**
   - Add BulkIn folder to exclusions
   - Temporary test: Disable AV and retry

4. **Network latency:**
   - If SQL Server is remote, check network speed
   - Consider moving database to local server

5. **Optimize database:**
   ```sql
   -- Rebuild indexes
   ALTER INDEX ALL ON TextFileData REBUILD
   
   -- Update statistics
   UPDATE STATISTICS TextFileData
   ```

---

### Out of Memory Error

**Problem:** Application crashes with memory error

**Solutions:**

1. **Reduce batch size:**
   - Settings → Batch Size → Try 5,000
   - Trades speed for memory stability

2. **Process fewer files at once:**
   - Move some files to different folder
   - Process in batches of 10-20 files

3. **Close other applications:**
   - Free up RAM for BulkIn
   - Check Task Manager for memory hogs

4. **Check file sizes:**
   - Very large files (>100 MB) may need splitting
   - Use file splitter utility if needed

---

### Application Freezes

**Problem:** UI becomes unresponsive

**Solutions:**

1. **Wait for current batch:**
   - Application may be committing large transaction
   - Check SQL Server activity monitor

2. **Force stop:**
   - Press `Ctrl+C` in terminal (if running from console)
   - Or use Task Manager → End Task

3. **Check logs for errors:**
   - Restart application
   - Navigate to Logs tab
   - Review last entries before freeze

4. **Disable logging:**
   - Settings → Logging → Set to Error only
   - Reduces I/O overhead

---

### Data Not Appearing in Database

**Problem:** Processing completes but no data in TextFileData table

**Solutions:**

1. **Verify correct database:**
   ```sql
   SELECT DB_NAME()  -- Should show RAW_PROCESS
   ```

2. **Check for transaction rollback:**
   - Review error logs
   - Look for constraint violations

3. **Query temp table:**
   ```sql
   SELECT COUNT(*) FROM TempTextFileData
   ```
   - If data here but not in TextFileData, stored procedure issue

4. **Manual data transfer:**
   ```sql
   EXEC usp_TransferDataFromTemp
   ```

5. **Check permissions:**
   ```sql
   -- Verify INSERT permission
   SELECT HAS_PERMS_BY_NAME('TextFileData', 'OBJECT', 'INSERT')
   ```

---

## FAQ

### Q: Can I process CSV files?

**A:** Currently, BulkIn Desktop only supports plain text (`.txt`) files. CSV support is planned for a future version. As a workaround, you can:
1. Rename `.csv` files to `.txt` (if compatible format)
2. Use a CSV-to-TXT converter
3. Import CSV via SQL Server Import/Export Wizard

---

### Q: How do I process files from multiple folders?

**A:** The current version processes one folder at a time. To process multiple folders:

**Option 1:** Sequential Processing
1. Process Folder A
2. Browse to Folder B
3. Process Folder B

**Option 2:** Consolidate Files
1. Copy all files to single folder
2. Process combined folder

**Option 3:** Multiple Instances
1. Launch multiple copies of BulkIn
2. Configure each with different folder
3. Run simultaneously (ensure sufficient resources)

---

### Q: What happens if the application crashes mid-processing?

**A:** BulkIn uses transaction-based processing:
- ✅ **Completed batches** are committed to database (data safe)
- ⏸️ **Current batch** being processed may be rolled back
- ❌ **Unprocessed files** remain in queue

**Recovery Steps:**
1. Restart BulkIn Desktop
2. Browse to same folder
3. Already-processed files can be moved/deleted
4. Re-run processing on remaining files

**Best Practice:** Enable "Archive After Processing" to automatically move completed files.

---

### Q: Can I schedule automatic processing?

**A:** The Desktop version doesn't have built-in scheduling. Options:

**Option 1: Windows Task Scheduler**
1. Create a batch script to launch BulkIn and trigger processing
2. Schedule via Task Scheduler (not supported in v1.0)

**Option 2: Console Version**
- BulkIn has a console version (`BulkIn.Console.exe`)
- Supports command-line arguments
- Can be scheduled via Task Scheduler
- See console documentation for details

---

### Q: How do I backup my settings?

**A:** Settings are stored in `appsettings.json`:

**Backup:**
```powershell
Copy-Item "appsettings.json" "appsettings.backup.json"
```

**Restore:**
```powershell
Copy-Item "appsettings.backup.json" "appsettings.json"
```

**Location:** Same folder as `BulkIn.Desktop.exe`

---

### Q: Can multiple users process files simultaneously?

**A:** Yes, with considerations:

✅ **Database Level:**
- SQL Server handles concurrent connections
- Each user's data inserted independently
- No conflicts or deadlocks

⚠️ **File System Level:**
- Each user needs separate source folder
- Cannot process same file simultaneously (file locking)

**Recommended Setup:**
- User A: `\\Server\Files\FolderA`
- User B: `\\Server\Files\FolderB`
- Both write to same database (no issues)

---

### Q: How do I update to a new version?

**A:** Update process:

1. **Export your settings:**
   - Copy `appsettings.json` to safe location

2. **Close BulkIn Desktop:**
   - Ensure all processing stopped

3. **Install new version:**
   - Download new installer
   - Extract to same folder (overwrite old files)
   - Or install to new folder

4. **Restore settings:**
   - Copy `appsettings.json` back
   - Or reconfigure via Settings tab

5. **Verify:**
   - Check version: Help → About
   - Test connection to database

**Note:** Database schema updates (if any) will be documented in release notes.

---

### Q: Is there a dark theme?

**A:** The current version uses a light theme optimized for clarity and accessibility. Dark theme support may be added in a future release based on user feedback.

---

### Q: How do I report bugs or request features?

**A:** Contact information:
- **Email:** support@bulkin.local
- **Issue Tracker:** Internal ticketing system
- **Phone:** Extension 1234

**When reporting bugs, include:**
1. BulkIn version (Help → About)
2. Windows version
3. Error message (if any)
4. Steps to reproduce
5. Screenshot (if applicable)
6. Log file (export from Logs tab)

---

## Support

### Getting Help

**📧 Email Support:** support@bulkin.local  
**📞 Phone:** Internal Extension 1234  
**🕐 Hours:** Monday-Friday, 9 AM - 5 PM

**📚 Documentation:**
- User Guide (this document)
- Keyboard Shortcuts Reference: `KEYBOARD_SHORTCUTS.md`
- Accessibility Guide: `ACCESSIBILITY.md`
- Testing Report: `TESTING_REPORT.md`

**🔧 Technical Support:**
- Database setup issues
- Performance optimization
- Integration assistance
- Custom feature requests

### Self-Help Resources

**Check the Logs:**
- Most issues have detailed error messages in Logs tab
- Export logs for troubleshooting

**Review Documentation:**
- Keyboard Shortcuts: `KEYBOARD_SHORTCUTS.md`
- Accessibility: `ACCESSIBILITY.md`

**Test Components:**
- Settings → Test Connection (verifies database)
- Settings → Browse Files (verifies file access)
- Help → About (shows version information)

### Reporting Issues

**Include This Information:**
1. **Environment:**
   - Windows version: `winver`
   - .NET version: `dotnet --version`
   - BulkIn version: Help → About

2. **Problem Description:**
   - What were you trying to do?
   - What actually happened?
   - Error messages (exact text)

3. **Attachments:**
   - Exported log file
   - Screenshot of error
   - Sample file (if relevant)

### Feature Requests

We welcome feedback! If you have ideas for new features:
- Submit via email with detailed description
- Include use case and business benefit
- Indicate priority (nice-to-have vs. critical)

---

## Appendix A: Configuration File Reference

### appsettings.json Structure

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=RAW_PROCESS;Trusted_Connection=True;TrustServerCertificate=True;",
    "ConnectionTimeout": 30,
    "BatchSize": 10000
  },
  "FileProcessing": {
    "SourceFolder": "",
    "ArchiveFolder": "Archive",
    "DeleteAfterProcessing": false,
    "SupportedExtensions": [".txt"]
  },
  "Logging": {
    "LogLevel": "Information",
    "LogToFile": true,
    "LogFolder": "logs",
    "MaxLogFileSizeMB": 10
  }
}
```

### Configuration Options

**Database.ConnectionString**
- Type: String
- Required: Yes
- Example: `Server=.\SQLEXPRESS;Database=RAW_PROCESS;Integrated Security=True;`

**Database.ConnectionTimeout**
- Type: Integer
- Default: 30
- Range: 15-120 seconds

**Database.BatchSize**
- Type: Integer
- Default: 10000
- Range: 1000-100000 rows

**FileProcessing.SourceFolder**
- Type: String
- Default: Empty (must be configured)
- Example: `C:\Data\TextFiles`

**Logging.LogLevel**
- Type: String
- Options: "Information", "Warning", "Error"
- Default: "Information"

---

## Appendix B: Database Schema

### TextFileData Table

```sql
CREATE TABLE [dbo].[TextFileData] (
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Data] NVARCHAR(MAX) NOT NULL,
    [Filename] NVARCHAR(255) NOT NULL,
    [Date] DATETIME2(7) NOT NULL DEFAULT GETDATE()
)

-- Indexes
CREATE NONCLUSTERED INDEX [IX_TextFileData_Filename] 
ON [dbo].[TextFileData] ([Filename] ASC)

CREATE NONCLUSTERED INDEX [IX_TextFileData_Date] 
ON [dbo].[TextFileData] ([Date] ASC)
```

### TempTextFileData Table (Auto-Created)

```sql
CREATE TABLE [dbo].[TempTextFileData] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [Data] NVARCHAR(MAX) NOT NULL,
    [Filename] NVARCHAR(255) NOT NULL
)
```

### Stored Procedure

```sql
CREATE PROCEDURE [dbo].[usp_TransferDataFromTemp]
    @Filename NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO [dbo].[TextFileData] ([Data], [Filename], [Date])
        SELECT [Data], [Filename], GETDATE()
        FROM [dbo].[TempTextFileData];
        
        TRUNCATE TABLE [dbo].[TempTextFileData];
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
```

---

## Appendix C: Performance Tuning

### Optimal Settings by Scenario

**Small Files (<10 MB each, <50 files)**
- Batch Size: 5,000
- Expected Speed: 8,000-10,000 lines/sec

**Medium Files (10-100 MB each, 50-200 files)**
- Batch Size: 10,000 (default)
- Expected Speed: 10,000-15,000 lines/sec

**Large Files (>100 MB each, 200+ files)**
- Batch Size: 25,000-50,000
- Expected Speed: 12,000-20,000 lines/sec
- Consider splitting very large files

### SQL Server Optimization

```sql
-- Increase tempdb size
ALTER DATABASE tempdb 
MODIFY FILE (NAME = tempdev, SIZE = 1024MB, FILEGROWTH = 256MB)

-- Set recovery model to SIMPLE for bulk operations
ALTER DATABASE RAW_PROCESS SET RECOVERY SIMPLE

-- Disable indexes during bulk insert
ALTER INDEX ALL ON TextFileData DISABLE

-- After processing, rebuild
ALTER INDEX ALL ON TextFileData REBUILD
```

### Windows Optimization

1. **Increase Page File:**
   - System Properties → Advanced → Performance Settings
   - Virtual Memory → Increase size

2. **Disable Search Indexing:**
   - Services → Windows Search → Stop & Disable

3. **Set High Performance Power Plan:**
   - Control Panel → Power Options → High Performance

---

## Glossary

**Batch Size** - Number of rows inserted in a single transaction  
**Bulk Insert** - High-speed database insert operation  
**FluentTheme** - Modern UI design system used by Avalonia  
**Source Folder** - Directory containing text files to process  
**Staging Table** - Temporary table (TempTextFileData) for bulk load  
**Target Table** - Final destination table (TextFileData)  
**Transaction** - Unit of database work that succeeds or fails atomically  

---

## Document Information

**Version:** 1.0.0  
**Last Updated:** November 2, 2025  
**Author:** BulkIn Development Team  
**Feedback:** support@bulkin.local

---

*End of User Guide*
