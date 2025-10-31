# 🚀 BulkIn Launcher Scripts

## Available Batch Files

### 1. **BulkIn-Menu.bat** 🎯 (EASIEST - Recommended)
**Usage:** Double-click to open interactive menu
- Interactive menu with all options
- No need to remember commands
- Easy access to logs, config, and documentation
- Perfect for all users

```cmd
D:\Project_TextFile\BulkIn\BulkIn-Menu.bat
```

**Desktop Shortcut:**
Run `CreateDesktopShortcut.bat` to add BulkIn icon to your desktop!

---

### 2. **RunBulkIn.bat** ⚡ (Quick for Development)
**Usage:** Double-click to run
- Compiles and runs the application using `dotnet run`
- Automatically sets UTF-8 encoding for emoji support
- Best for development (always uses latest code changes)
- Slightly slower startup (compiles on each run)

```cmd
D:\Project_TextFile\BulkIn\RunBulkIn.bat
```

---

### 2. **RunBulkIn.bat** ⚡ (Quick for Development)
**Usage:** Double-click to run
- Runs the pre-compiled executable directly (BulkIn.exe)
- Much faster startup time (no compilation)
- Requires building first with `dotnet build`
- Best for production/frequent use

**Prerequisites:**
```cmd
cd D:\Project_TextFile\BulkIn
dotnet build
```

Then run:
```cmd
D:\Project_TextFile\BulkIn\RunBulkIn-Fast.bat
```

---

### 3. **RunBulkIn-Fast.bat** 🏃 (Fastest for Production)
**Usage:** Double-click to run
- Runs the pre-compiled executable directly (BulkIn.exe)
- Much faster startup time (no compilation)
- Requires building first with `dotnet build`
- Best for production/frequent use

**Prerequisites:**
```cmd
cd D:\Project_TextFile\BulkIn
dotnet build
```

Then run:
```cmd
D:\Project_TextFile\BulkIn\RunBulkIn-Fast.bat
```

---

### 4. **BuildAndRun.bat** 🔨 (All-in-One)
**Usage:** Double-click to run
- Builds the project in **Release** mode (optimized)
- Automatically runs the application after successful build
- Best for creating an optimized version for production

```cmd
D:\Project_TextFile\BulkIn\BuildAndRun.bat
```

---

### 5. **CreateDesktopShortcut.bat** 🖥️ (One-Time Setup)
**Usage:** Double-click to create desktop shortcut
- Creates a shortcut on your desktop
- Links to the interactive menu
- One-click access to BulkIn
- Run once for easy access

```cmd
D:\Project_TextFile\BulkIn\CreateDesktopShortcut.bat
```

---

## 📋 Quick Start

**First Time Setup (Recommended):**
1. Double-click `CreateDesktopShortcut.bat` to create desktop icon
2. Double-click the "BulkIn" icon on your desktop
3. Select option [2] "🔨 Build and Run (Optimized)"
4. Wait for build to complete
5. Application will start automatically

**Alternative - Simple Start:**
1. Double-click `BulkIn-Menu.bat`
2. Select option [1] "🚀 Run BulkIn (Quick Start)"

---

## 🎨 Emoji Support

All batch files automatically enable UTF-8 encoding (`chcp 65001`) so emojis display correctly in the console:
- ✅ Success indicators
- ❌ Error messages
- 📊 Statistics
- 🚀 Progress updates
- And more!

---

## 🛠️ Manual Commands

If you prefer command line:

**Development (with auto-compile):**
```cmd
cd D:\Project_TextFile\BulkIn\src\BulkIn
dotnet run
```

**Production (pre-compiled):**
```cmd
cd D:\Project_TextFile\BulkIn\src\BulkIn\bin\Release\net8.0
BulkIn.exe
```

**Build only:**
```cmd
cd D:\Project_TextFile\BulkIn
dotnet build --configuration Release
```

---

## 📁 File Locations

After building, executables are located at:
- **Debug:** `src\BulkIn\bin\Debug\net8.0\BulkIn.exe`
- **Release:** `src\BulkIn\bin\Release\net8.0\BulkIn.exe`

---

## ⚙️ Configuration

Before running, ensure `appsettings.json` is configured:
- **Database:** Server name, database name, authentication
- **Files:** Source path, file patterns, exclusions
- **Processing:** Batch size, transaction settings
- **Logging:** Log file location

File location: `src\BulkIn\appsettings.json`

---

## 📝 Logs

Application logs are saved to:
```
D:\Project_TextFile\BulkIn\logs\
├── SuccessLog_YYYYMMDD_HHMMSS.txt
└── ErrorLog_YYYYMMDD_HHMMSS.txt
```

---

## 🆘 Troubleshooting

**"BulkIn.exe not found"**
- Run `BuildAndRun.bat` first, or manually build:
  ```cmd
  cd D:\Project_TextFile\BulkIn
  dotnet build
  ```

**"Configuration file not found"**
- Ensure `appsettings.json` exists in `src\BulkIn\` folder

**Emojis not displaying**
- The batch files automatically enable UTF-8
- If still having issues, run: `chcp 65001` manually

**Database connection errors**
- Verify SQL Server is running
- Check connection settings in `appsettings.json`
- Ensure you have access to the database

---

## 📞 Support

For issues or questions, check the main project documentation:
- `README.md` - Project overview
- `QUICKSTART.md` - 5-minute setup guide
- `MVP_COMPLETE_SUMMARY.md` - Feature documentation
