# ⚡ BulkIn - Quick Start Guide

Get up and running with BulkIn in **5 minutes**!

---

## � Prerequisites

```bash
✓ .NET 8 SDK
✓ SQL Server 2019+
✓ Windows 10/11
```

---

## �🚀 Step-by-Step Setup

### 1️⃣ Database Setup

Run the database script:

```bash
sqlcmd -S YOUR_SERVER\INSTANCE -i scripts\DatabaseSetup.sql
```

Or manually execute `scripts/DatabaseSetup.sql` in SQL Server Management Studio.

**This creates:**
- Database: `RAW_PROCESS`
- Table: `TextFileData` (ID, Data, Filename, Date)
- Temp Table: `TempTextFileData`
- Stored Procedure: `usp_TransferDataFromTemp`

---

### 2️⃣ Configure Application

Edit `src/BulkIn/appsettings.json`:

```json
{
  "DatabaseSettings": {
    "ServerName": "YOUR_SERVER\\INSTANCE",     // ← Change this
    "DatabaseName": "RAW_PROCESS",
    "UseTrustedConnection": true
  },
  "FileSettings": {
    "SourceFilePath": "D:\\YourPath\\SourceFiles",  // ← Change this
    "FilePatterns": [ "*.txt", "*.csv" ]
  }
}
```

---

### 3️⃣ Prepare Your Files

Place your text files in the source directory:

```
D:\YourPath\SourceFiles\
├── file1.txt
├── file2.csv
└── data.log
```

---

### 4️⃣ Run the Application

**Option A: Quick Run**
```bash
cd src\BulkIn
dotnet run
```

**Option B: Use Batch Files**
```bash
RunBulkIn.bat         # Auto-compile and run
BulkIn-Menu.bat       # Interactive menu
RunBulkIn-Fast.bat    # Direct executable
```

---

## 📊 What Happens Next?

```
1. Configuration loads ✅
2. Database connection tests ✅
3. Files discovered (e.g., 18 files found)
4. Press ENTER to start processing
5. Progress shown in real-time
6. Final summary displayed
```

---

## 🎯 Expected Output

```
───────────────────────────────────────────────────────────
📄 [1/18] yourfile.txt
   354.18 MB • 2025-10-31 01:36:45
───────────────────────────────────────────────────────────
   🔧 Preparing temp table... ✅
   📊 Processing: 200,000 rows...
   📥 Inserted: 923,843 rows
   🔄 Transferring to target... ✅ (923,843 rows)
   ✅ Completed: 923,843 rows • 23.7s • 38,983 rows/sec
```

---

## 📁 Check Results

Query your data:

```sql
USE RAW_PROCESS;

-- View all data
SELECT * FROM TextFileData;

-- Count rows per file
SELECT Filename, COUNT(*) AS RowCount
FROM TextFileData
GROUP BY Filename
ORDER BY Filename;

-- View recent inserts
SELECT TOP 100 * 
FROM TextFileData
ORDER BY Date DESC;
```

---

## 🔧 Common Issues

| Issue | Fix |
|-------|-----|
| **Connection failed** | Update `ServerName` in appsettings.json |
| **Files not found** | Check `SourceFilePath` exists |
| **Permission denied** | Run as Administrator |

---

## 📝 Logs

Check logs for detailed information:

```
logs/
├── SuccessLog_YYYYMMDD_HHMMSS.txt
└── ErrorLog_YYYYMMDD_HHMMSS.txt
```

---

## 🎉 Success!

You're now processing bulk text files at **35,000-65,000 rows/second**!

For advanced configuration, see the main [README.md](README.md).

---

**Questions?** Open an issue on GitHub!

**Version:** 1.0  
**Date:** October 31, 2025  
**Status:** Production-Ready MVP

---

## ⚡ 5-Minute Setup

### **Prerequisites**
✅ Windows 10/11  
✅ .NET 8 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))  
✅ SQL Server 2019+ (Express/Standard/Enterprise)  
✅ SQL Server Management Studio (SSMS) - Optional but recommended  

---

## 📋 Setup Steps

### **Step 1: Database Setup** (2 minutes)

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Open the file: `BulkIn\scripts\DatabaseSetup.sql`
4. Execute the script (F5)

**Expected Result:**
```
Database TextFileDatabase created successfully.
Table TextFileData created successfully.
Table TempTextFileData created successfully.
```

---

### **Step 2: Configure Application** (1 minute)

1. Navigate to: `BulkIn\src\BulkIn\`
2. Open `appsettings.json` in any text editor
3. Update the following settings:

```json
{
  "DatabaseSettings": {
    "ServerName": "localhost\\SQLEXPRESS",     ← Change to your SQL Server
    "DatabaseName": "TextFileDatabase"
  },
  "FileSettings": {
    "SourceFilePath": "D:\\SourceFiles",      ← Change to your file folder
    "FilePattern": "*.txt"
  },
  "LoggingSettings": {
    "LogFilePath": "D:\\Project_TextFile\\BulkIn\\logs"  ← Change to your log folder
  }
}
```

**Important:**
- Use double backslashes `\\` in paths (JSON format)
- Ensure the source file path exists
- Log path will be created automatically

---

### **Step 3: Prepare Your Files** (1 minute)

1. Create your source folder (e.g., `D:\SourceFiles`)
2. Place your text files there (e.g., `ACEFOI20250804.txt`)
3. Ensure files are not locked by another application

**Supported Formats:**
- ✅ Plain text files (.txt)
- ✅ CSV files (.csv) - treated as text
- ✅ UTF-8 encoding
- ✅ Any file size (tested up to 500 MB)
- ✅ Any content (whitespace preserved)

---

### **Step 4: Build Application** (30 seconds)

Open PowerShell in the `BulkIn` folder and run:

```powershell
dotnet build src\BulkIn\BulkIn.csproj
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### **Step 5: Run Application** (30 seconds)

```powershell
dotnet run --project src\BulkIn\BulkIn.csproj
```

**You'll see:**
```
╔════════════════════════════════════════════════════════════╗
║                    BulkIn v1.0                             ║
║         Bulk Text File Data Ingestion System               ║
╚════════════════════════════════════════════════════════════╝

⚙️  Loading configuration...
✓ Configuration loaded successfully
...
```

---

## 🎯 Usage Example

### **Scenario:** Process 3 text files into SQL Server

**Files:**
- `DataFile1.txt` (10 MB, 100,000 lines)
- `DataFile2.txt` (25 MB, 250,000 lines)
- `DataFile3.txt` (50 MB, 500,000 lines)

**Steps:**
1. Place files in `D:\SourceFiles`
2. Run BulkIn application
3. Press ENTER when prompted
4. Wait for processing to complete

**Expected Output:**
```
📁 Discovering files...
✓ Found 3 file(s) to process

Files to be processed:
   1. DataFile1.txt
   2. DataFile2.txt
   3. DataFile3.txt

Press ENTER to start processing (or Ctrl+C to cancel):

╔════════════════════════════════════════════════════════════╗
║              STARTING BATCH PROCESSING                     ║
╚════════════════════════════════════════════════════════════╝

[1/3] Processing: DataFile1.txt
   ├─ Reading and inserting: DataFile1.txt
   ├─ Bulk insert complete: 100,000 rows
   ├─ Transfer complete: 100,000 rows
✓ DataFile1.txt - 100,000 rows in 5.2s

[2/3] Processing: DataFile2.txt
   ├─ Reading and inserting: DataFile2.txt
   ├─ Bulk insert complete: 250,000 rows
   ├─ Transfer complete: 250,000 rows
✓ DataFile2.txt - 250,000 rows in 12.8s

[3/3] Processing: DataFile3.txt
   ├─ Reading and inserting: DataFile3.txt
   ├─ Bulk insert complete: 500,000 rows
   ├─ Transfer complete: 500,000 rows
✓ DataFile3.txt - 500,000 rows in 25.5s

╔═══════════════════════════════════════════════════════════╗
║            BULK PROCESSING COMPLETE - SUMMARY             ║
╚═══════════════════════════════════════════════════════════╝

📊 OVERALL STATISTICS:
   Total Files Discovered: 3
   ✓ Successfully Processed: 3
   ✗ Failed: 0
   Success Rate: 100.00%

📈 DATA METRICS:
   Total Rows Processed: 850,000
   Average Speed: 19,535 rows/second

⏱️  TIMING:
   Total Duration: 00:00:43
```

---

## 📊 Verify Results in SQL Server

### **Option 1: SSMS Query**
```sql
USE TextFileDatabase;
GO

-- Check total rows
SELECT COUNT(*) AS TotalRows FROM TextFileData;

-- Check rows by file
SELECT 
    Filename,
    COUNT(*) AS RowCount,
    MIN(Date) AS FirstInserted,
    MAX(Date) AS LastInserted
FROM TextFileData
GROUP BY Filename
ORDER BY Filename;

-- View sample data
SELECT TOP 10 
    ID,
    LEFT(Data, 100) AS DataPreview,
    Filename,
    Date
FROM TextFileData
ORDER BY ID;
```

### **Expected Results:**
```
TotalRows: 850,000

Filename         RowCount   FirstInserted        LastInserted
---------------------------------------------------------------------------
DataFile1.txt    100,000    2025-10-31 14:30:00  2025-10-31 14:30:00
DataFile2.txt    250,000    2025-10-31 14:30:05  2025-10-31 14:30:05
DataFile3.txt    500,000    2025-10-31 14:30:18  2025-10-31 14:30:18
```

---

## 📝 Check Log Files

Logs are automatically created in your configured log path:

**Success Log:** `logs\SuccessLog_20251031_143000.txt`
```
[2025-10-31 14:30:00] Session Started
[2025-10-31 14:30:05] File: DataFile1.txt - SUCCESS - 100,000 rows
[2025-10-31 14:30:18] File: DataFile2.txt - SUCCESS - 250,000 rows
[2025-10-31 14:30:43] File: DataFile3.txt - SUCCESS - 500,000 rows
[2025-10-31 14:30:43] Batch Complete - 3/3 files successful
```

**Error Log:** `logs\ErrorLog_20251031_143000.txt`
- Empty if no errors occurred
- Contains detailed error messages and stack traces if errors occur

---

## ⚙️ Configuration Options

### **Key Settings You Can Adjust:**

| Setting | Purpose | Default | Range |
|---------|---------|---------|-------|
| `BatchSize` | Rows per batch | 50,000 | 10,000 - 100,000 |
| `CommandTimeout` | SQL timeout (seconds) | 600 | 60 - 3600 |
| `ContinueOnError` | Continue if file fails | true | true/false |
| `StreamReaderBufferSize` | Read buffer (bytes) | 65,536 | 1,024 - 1,048,576 |
| `FilePattern` | File matching pattern | *.txt | *.txt, *.csv, *.* |
| `ProcessInAlphabeticalOrder` | Sort files A-Z | true | true/false |

### **Example: Process Larger Batches**
```json
"ProcessingSettings": {
  "BatchSize": 100000,        ← Double the batch size
  "CommandTimeout": 900       ← Increase timeout to 15 minutes
}
```

---

## 🔧 Troubleshooting

### **Problem: "Configuration file not found"**
**Solution:**
- Ensure `appsettings.json` is in `src\BulkIn\` folder
- Check the file is not renamed or moved

### **Problem: "Database connection failed"**
**Solution:**
- Verify SQL Server is running
- Check `ServerName` in appsettings.json
- Test connection in SSMS first
- For SQL Authentication, add Username/Password:
  ```json
  "UseTrustedConnection": false,
  "Username": "sa",
  "Password": "YourPassword"
  ```

### **Problem: "Table does not exist"**
**Solution:**
- Run `DatabaseSetup.sql` script
- Verify database name matches configuration
- Check table names: `TextFileData` and `TempTextFileData`

### **Problem: "Access denied to file"**
**Solution:**
- Close the file in Excel, Notepad, etc.
- Check file is not read-only
- Verify user has read permissions

### **Problem: "Out of memory"**
**Solution:**
- Reduce `BatchSize` to 25,000 or lower
- Close other applications
- Process files individually (not 30 at once)

### **Problem: "Transaction log full"**
**Solution:**
```sql
-- Switch to SIMPLE recovery model
USE master;
ALTER DATABASE TextFileDatabase SET RECOVERY SIMPLE;

-- Shrink transaction log
USE TextFileDatabase;
DBCC SHRINKFILE (TextFileDatabase_log, 1);
```

---

## 🎓 Tips & Best Practices

### **For Best Performance:**
✅ Use `SIMPLE` recovery model during bulk loads  
✅ Process during off-peak hours  
✅ Disable indexes on target table during bulk load  
✅ Rebuild indexes after processing  
✅ Use local SQL Server (not remote)  
✅ Close unnecessary applications  

### **For Data Integrity:**
✅ Always run a test with 1-2 files first  
✅ Verify row counts after processing  
✅ Keep backup of original files  
✅ Check log files for any warnings  
✅ Test whitespace preservation with sample data  

### **For Large Batches (20+ files):**
✅ Set `ContinueOnError: true`  
✅ Monitor log files during processing  
✅ Ensure adequate disk space (3x file size)  
✅ Consider processing in smaller groups  
✅ Schedule during maintenance windows  

---

## 📞 Need Help?

### **Check These First:**
1. ✅ Log files in `logs\` folder
2. ✅ Console output messages
3. ✅ SQL Server error logs
4. ✅ File permissions

### **Common Questions:**

**Q: Can I process CSV files?**  
A: Yes! Use `"FilePattern": "*.csv"` in configuration.

**Q: Will it trim my data?**  
A: NO! All whitespace is preserved exactly as in the file.

**Q: Can I process files in parallel?**  
A: Not in v1.0. Files are processed sequentially for data integrity.

**Q: What if processing fails mid-batch?**  
A: Each file is in its own transaction. Failed files rollback automatically. Successful files remain committed.

**Q: Can I resume a failed batch?**  
A: Move successfully processed files to a different folder, then re-run.

---

## 🎯 What's Next?

After successful processing:

1. ✅ **Verify Data:** Query SQL Server to confirm row counts
2. ✅ **Check Logs:** Review success/error logs
3. ✅ **Backup Data:** Take a database backup
4. ✅ **Archive Files:** Move processed files to archive folder
5. ✅ **Schedule:** Set up automated runs if needed

---

## 📚 Additional Resources

- **README.md** - Complete project documentation
- **DatabaseSetup.sql** - Database schema details
- **MVP_COMPLETE_SUMMARY.md** - Full feature list
- **BulkIn_Roadmap_TrackingPlan.md** - Development details

---

## ✨ Success Checklist

Before starting production use:

- [ ] SQL Server database created
- [ ] Tables exist (TextFileData, TempTextFileData)
- [ ] appsettings.json configured correctly
- [ ] Source file path exists
- [ ] Test run completed successfully
- [ ] Row counts verified in SQL Server
- [ ] Log files reviewed
- [ ] Backup strategy in place

---

**You're ready to go! Happy bulk loading! 🚀**

---

**Document Version:** 1.0  
**Last Updated:** October 31, 2025  
**Status:** Production-Ready
