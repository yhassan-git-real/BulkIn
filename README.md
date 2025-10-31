<div align="center">

# � BulkIn

### *High-Performance Bulk Text File Ingestion System*

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
[![C#](https://img.shields.io/badge/C%23-11.0-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](LICENSE)

*Lightning-fast • Memory-efficient • Production-ready*

[Features](#-features) • [Quick Start](#-quick-start) • [Configuration](#-configuration) • [Documentation](#-documentation)

</div>

---

## 🎯 Overview

**BulkIn** is a modern .NET 8 console application engineered for **high-volume text file ingestion** into SQL Server. Process multiple large files (up to 500 MB each) with exceptional speed and reliability while preserving data integrity.

### 🎪 Live Demo Output

```ansi
╔════════════════════════════════════════════════════════════╗
║                    BulkIn v1.0                             ║
║         Bulk Text File Data Ingestion System               ║
╚════════════════════════════════════════════════════════════╝

───────────────────────────────────────────────────────────
📄 [1/18] ACEFOI202508011.txt
   354.18 MB • 2025-10-31 01:36:45
───────────────────────────────────────────────────────────
   🔧 Preparing temp table... ✅
   📊 Processing: 200,000 rows...
   📥 Inserted: 923,843 rows
   🔄 Transferring to target... ✅ (923,843 rows)
   ✅ Completed: 923,843 rows • 23.7s • 38,983 rows/sec
```

### � Performance Metrics

<div align="center">

| Metric | Performance |
|--------|-------------|
| **Throughput** | 35,000 - 65,000 rows/sec |
| **Files Processed** | 18 files (6.37 GB) |
| **Total Rows** | 16.6 million rows |
| **Processing Time** | 7 min 43 sec |
| **Success Rate** | 100% |

</div>

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 🚀 Performance
- **SqlBulkCopy** - Maximum insert speed
- **Streaming architecture** - No memory overflow
- **Batch processing** - 200K rows per batch
- **Transaction safety** - Per-file rollback

</td>
<td width="50%">

### 🎨 User Experience
- **Colorful console** - Modern ANSI colors
- **Real-time progress** - Live row counters
- **Comprehensive logs** - Success/Error tracking
- **Easy launchers** - Batch file shortcuts

</td>
</tr>
<tr>
<td width="50%">

### ⚙️ Flexibility
- **Multi-format support** - .txt, .csv, .log, etc.
- **Multiple patterns** - Process mixed file types
- **Configurable** - External JSON config
- **Whitespace preservation** - Exact data capture

</td>
<td width="50%">

### 🛡️ Reliability
- **Error handling** - Graceful failure recovery
- **Validation** - Pre-flight checks
- **Logging** - Timestamped audit trails
- **Retry logic** - Automatic reconnection

</td>
</tr>
</table>

---

## 🚀 Quick Start

### Prerequisites

```bash
✓ .NET 8 SDK or later
✓ SQL Server 2019+
✓ Windows OS (10/11)
```

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/BulkIn.git
cd BulkIn

# 2. Set up database
sqlcmd -S YOUR_SERVER -i scripts/DatabaseSetup.sql

# 3. Configure settings
# Edit src/BulkIn/appsettings.json with your details

# 4. Run!
dotnet run --project src/BulkIn
```

### 🎯 One-Click Launch

Use the included batch files for instant startup:

```cmd
RunBulkIn.bat          # Quick run (auto-compiles)
BulkIn-Menu.bat        # Interactive menu
RunBulkIn-Fast.bat     # Direct executable (fastest)
```

---

## ⚙️ Configuration

Edit `src/BulkIn/appsettings.json`:

```json
{
  "DatabaseSettings": {
    "ServerName": "YOUR_SERVER\\INSTANCE",
    "DatabaseName": "RAW_PROCESS",
    "UseTrustedConnection": true
  },
  "FileSettings": {
    "SourceFilePath": "D:\\YourPath\\SourceFiles",
    "FilePatterns": [ "*.txt", "*.csv", "*.log" ],
    "ProcessInAlphabeticalOrder": true
  },
  "ProcessingSettings": {
    "BatchSize": 200000,
    "EnableTransactionPerFile": true,
    "ContinueOnError": true
  }
}
```

### 🎨 Multi-Format Support

Process multiple file types in one run:

```json
"FilePatterns": [ "*.txt", "*.csv" ]           // Text and CSV
"FilePatterns": [ "*.txt", "*.log", "*.dat" ]  // Multiple formats
"FilePatterns": [ "data_*.txt" ]               // Pattern matching
```

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    File Processing Flow                     │
└─────────────────────────────────────────────────────────────┘

  📁 Source Files
       ↓
  🔍 Discovery & Validation
       ↓
  📖 Stream Reading (yield return)
       ↓
  📥 SqlBulkCopy → TempTable
       ↓
  🔄 Stored Procedure → TargetTable
       ↓
  ✅ Success Logging
```

### 📂 Project Structure

```
BulkIn/
├── 📁 src/BulkIn/
│   ├── Configuration/         # Config models & validation
│   ├── Services/             # Core business logic
│   │   ├── FileReaderService      → Streaming file reader
│   │   ├── BulkInsertService      → SqlBulkCopy wrapper
│   │   ├── DataTransferService    → Temp→Target transfer
│   │   └── FileProcessorService   → Main orchestrator
│   ├── Models/               # Data models
│   ├── Utilities/            # Helpers & colors
│   └── appsettings.json      # Configuration
├── 📁 scripts/               # SQL scripts
├── 📁 logs/                  # Application logs
└── 📄 *.bat                  # Quick launchers
```

---

## 📚 Documentation

### Quick Reference

- **QUICKSTART.md** - 5-minute setup guide
- **Database Setup** - See `scripts/DatabaseSetup.sql`
- **Batch Launchers** - Run `BulkIn-Menu.bat` for options

### Key Concepts

#### 🔹 Two-Stage Loading

```sql
SourceFiles → TempTextFileData → TextFileData
            (staging)          (permanent)
```

#### 🔹 Data Model

Each row stores:
- `ID` - Auto-increment primary key
- `Data` - Complete line from file (preserves whitespace)
- `Filename` - Source file tracking
- `Date` - Insert timestamp

#### 🔹 Color Coding

- 🟢 **Green** - Success, completed operations
- 🔵 **Cyan** - Information, file names
- 🟡 **Yellow** - Progress counters
- 🟣 **Magenta** - Performance metrics
- 🔴 **Red** - Errors and failures

---

## 🎯 Usage Examples

### Basic Usage

```bash
# Process all .txt files
dotnet run --project src/BulkIn

# Use menu for interactive options
BulkIn-Menu.bat
```

### Advanced Scenarios

```json
// Process multiple formats
"FilePatterns": [ "*.txt", "*.csv", "*.log" ]

// Exclude backup files
"ExcludeFilePatterns": [ "*_backup.*", "*_temp.*" ]

// Large batch size for better performance
"BatchSize": 500000
```

---

## 🔧 Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| **Connection failed** | Check server name in `appsettings.json` |
| **File not found** | Verify `SourceFilePath` directory exists |
| **Permission denied** | Run as Administrator or check folder permissions |
| **Slow performance** | Increase `BatchSize` to 500,000 |

### Logs Location

```
logs/
├── SuccessLog_YYYYMMDD_HHMMSS.txt
└── ErrorLog_YYYYMMDD_HHMMSS.txt
```

---

## 🚀 Performance Tips

1. **Batch Size** - Start with 200K, increase to 500K for better speed
2. **Transaction Mode** - Enable for safety, disable for maximum speed
3. **File Order** - Alphabetical sorting helps track progress
4. **Network** - Use local SQL Server for best performance
5. **Exclusions** - Filter unnecessary files to reduce processing time

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 💡 Tech Stack

- **Runtime**: .NET 8.0
- **Language**: C# 11.0
- **Database**: SQL Server 2019+
- **Libraries**: 
  - Microsoft.Data.SqlClient 5.2.2
  - Serilog 3.1.1
  - Microsoft.Extensions.Configuration 8.0.0

---

## 📞 Support

For issues, questions, or suggestions, please open an issue on GitHub.

---

<div align="center">

**Built with ❤️ using .NET 8 and C#**

⭐ Star this repo if you find it helpful!

</div>

2. **Set up the database**
   ```powershell
   # Run the database setup script in SQL Server Management Studio (SSMS)
   # or using sqlcmd:
   sqlcmd -S localhost\SQLEXPRESS -i scripts\DatabaseSetup.sql
   ```

3. **Configure the application**
   
   Edit `src\BulkIn\appsettings.json`:
   ```json
   {
     "DatabaseSettings": {
       "ServerName": "localhost\\SQLEXPRESS",
       "DatabaseName": "TextFileDatabase"
     },
     "FileSettings": {
       "SourceFilePath": "D:\\SourceFiles"
     }
   }
   ```

4. **Build the application**
   ```powershell
   cd BulkIn
   dotnet build
   ```

5. **Run the application**
   ```powershell
   dotnet run --project src\BulkIn\BulkIn.csproj
   ```

---

## ⚙️ Configuration

### appsettings.json Structure

#### Database Settings
```json
"DatabaseSettings": {
  "ServerName": "localhost\\SQLEXPRESS",
  "DatabaseName": "TextFileDatabase",
  "TempTableName": "TempTextFileData",
  "TargetTableName": "TextFileData",
  "ConnectionTimeout": 30,
  "CommandTimeout": 600,
  "UseTrustedConnection": true
}
```

#### File Settings
```json
"FileSettings": {
  "SourceFilePath": "D:\\SourceFiles",
  "FilePattern": "*.txt",
  "ProcessInAlphabeticalOrder": true,
  "ExcludeFilePatterns": ["*_backup.txt"]
}
```

#### Processing Settings
```json
"ProcessingSettings": {
  "BatchSize": 50000,
  "EnableTransactionPerFile": true,
  "ContinueOnError": true,
  "StreamReaderBufferSize": 65536
}
```

#### Logging Settings
```json
"LoggingSettings": {
  "LogFilePath": "D:\\Project_TextFile\\BulkIn\\logs",
  "SuccessLogPrefix": "SuccessLog",
  "ErrorLogPrefix": "ErrorLog",
  "EnableConsoleLogging": true,
  "LogLevel": "Information"
}
```

---

## 📊 Database Schema

### Target Table: TextFileData
```sql
CREATE TABLE TextFileData (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Data NVARCHAR(MAX) NOT NULL,
    Filename NVARCHAR(255) NOT NULL,
    Date DATETIME2 DEFAULT GETDATE()
);
```

### Staging Table: TempTextFileData
```sql
CREATE TABLE TempTextFileData (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Data NVARCHAR(MAX) NOT NULL
);
```

---

## 📝 Usage Examples

### Basic Usage
```powershell
# Process all .txt files in the configured directory
dotnet run --project src\BulkIn\BulkIn.csproj
```

### Expected Console Output
```
╔════════════════════════════════════════════════════════════╗
║              BulkIn v1.0 - Starting...                     ║
║        Bulk Text File Data Ingestion System                ║
╚════════════════════════════════════════════════════════════╝

[2025-10-31 14:35:22] Found 25 files to process
[2025-10-31 14:35:23] Processing: ACEFOI20250804.txt (1/25)
[2025-10-31 14:35:24]   ├─ Reading file...
[2025-10-31 14:35:42]   ├─ Bulk insert: 1,234,567 rows
[2025-10-31 14:36:15]   ├─ Transfer to target: Complete
[2025-10-31 14:36:16]   └─ ✓ Success (52 seconds)
...
```

---

## 📁 Log Files

Logs are generated in the configured `LogFilePath` directory:

- **Success Log**: `SuccessLog_YYYYMMDD_HHMMSS.txt`
- **Error Log**: `ErrorLog_YYYYMMDD_HHMMSS.txt`

### Log Entry Format
```
[2025-10-31 14:36:16] File: ACEFOI20250804.txt
[2025-10-31 14:36:16] Status: SUCCESS
[2025-10-31 14:36:16] Rows Processed: 1,234,567
[2025-10-31 14:36:16] Duration: 52 seconds
[2025-10-31 14:36:16] ----------------------------------------
```

---

## 🛠️ Troubleshooting

### Common Issues

#### 1. SQL Connection Errors
```
Error: Cannot connect to SQL Server
```
**Solution**: 
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure SQL Server allows TCP/IP connections

#### 2. File Access Denied
```
Error: Access denied to file
```
**Solution**:
- Ensure application has read permissions
- Check if file is locked by another process
- Verify `SourceFilePath` exists

#### 3. Memory Issues
```
Error: OutOfMemoryException
```
**Solution**:
- Reduce `BatchSize` in configuration
- Ensure streaming is enabled (default)
- Check available system memory

#### 4. Transaction Log Full
```
Error: Transaction log is full
```
**Solution**:
- Increase transaction log size
- Use SIMPLE recovery model for bulk operations
- Consider backing up transaction log

---

## 🔧 Development

### Project Structure
- **Configuration**: JSON config loading and validation
- **Services**: Business logic (file reading, bulk insert, transfer)
- **Models**: Data models and result tracking
- **Utilities**: Helper classes (SQL connections, file operations)

### Building for Production
```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

---

## 📈 Performance

**Tested Performance Metrics:**
- **500 MB file**: ~2-5 minutes per file
- **Memory usage**: < 200 MB (streaming architecture)
- **Throughput**: ~400,000+ rows per minute
- **20-30 files**: ~1-2.5 hours total

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Submit a pull request

---

## 📄 License

This project is licensed under the MIT License.

---

## 📞 Support

For issues, questions, or feature requests:
- Create an issue in the repository
- Contact the development team

---

## 🎯 Roadmap

See [BulkIn_Roadmap_TrackingPlan.md](../BulkIn_Roadmap_TrackingPlan.md) for detailed development phases and progress tracking.

---

**Version**: 1.0.0  
**Last Updated**: October 31, 2025  
**Status**: ✅ Phase 1 Complete - Foundation & Infrastructure
