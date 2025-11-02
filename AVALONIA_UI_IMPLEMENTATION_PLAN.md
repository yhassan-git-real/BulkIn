# BulkIn Avalonia UI - Implementation Plan & Design Specification

> **Status**: 📋 PLANNING PHASE - Awaiting Approval Before Development  
> **Date**: November 2, 2025  
> **Version**: 1.0

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Architecture & Project Structure](#architecture--project-structure)
3. [Shared Core Library Design](#shared-core-library-design)
4. [Avalonia UI Application Design](#avalonia-ui-application-design)
5. [UI/UX Design Specification](#uiux-design-specification)
6. [Visual Mockups & Wireframes](#visual-mockups--wireframes)
7. [Implementation Roadmap](#implementation-roadmap)
8. [Risk Assessment & Mitigation](#risk-assessment--mitigation)

---

## Executive Summary

### Objective
Create a modern, cross-platform Avalonia UI desktop application that provides a graphical interface for the existing BulkIn text file ingestion system, while maintaining **zero impact** on the current console application.

### Key Principles
- ✅ **Zero Console Impact**: Console app remains untouched and fully functional
- ✅ **Code Reuse**: Extract core logic into shared library
- ✅ **Modern UI**: Clean, professional, aesthetically pleasing interface
- ✅ **Cross-Platform**: Windows, macOS, Linux support via Avalonia
- ✅ **User-Centric**: Intuitive controls, real-time feedback, responsive design

### Technology Stack
- **UI Framework**: Avalonia UI 11.x (latest stable)
- **Architecture Pattern**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration (appsettings.json)
- **Logging**: Serilog with dual output (UI + file)
- **Target Framework**: .NET 8.0
- **Icons**: FluentAvalonia icons + custom SVG assets

---

## Architecture & Project Structure

### Proposed Solution Structure

```
BulkIn/
├── src/
│   ├── BulkIn.Console/              [EXISTING - NO CHANGES]
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── ... (all existing files)
│   │
│   ├── BulkIn.Core/                 [NEW - SHARED LIBRARY]
│   │   ├── Configuration/
│   │   │   ├── AppSettings.cs
│   │   │   ├── DatabaseSettings.cs
│   │   │   ├── FileSettings.cs
│   │   │   ├── ProcessingSettings.cs
│   │   │   └── LoggingSettings.cs
│   │   ├── Models/
│   │   │   ├── TextFileRecord.cs
│   │   │   ├── ProcessingResult.cs
│   │   │   └── FileProcessingStats.cs
│   │   ├── Services/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IFileProcessor.cs
│   │   │   │   ├── IFileReaderService.cs
│   │   │   │   ├── IBulkInsertService.cs
│   │   │   │   ├── IDataTransferService.cs
│   │   │   │   └── ILoggingService.cs
│   │   │   ├── FileProcessorService.cs
│   │   │   ├── FileReaderService.cs
│   │   │   ├── BulkInsertService.cs
│   │   │   ├── DataTransferService.cs
│   │   │   └── TextFileDataReader.cs
│   │   ├── Utilities/
│   │   │   ├── FileHelper.cs
│   │   │   └── SqlConnectionFactory.cs
│   │   └── BulkIn.Core.csproj
│   │
│   ├── BulkIn.Desktop/               [NEW - AVALONIA UI APP]
│   │   ├── App.axaml
│   │   ├── App.axaml.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Views/
│   │   │   ├── MainWindow.axaml
│   │   │   ├── MainWindow.axaml.cs
│   │   │   ├── SettingsView.axaml
│   │   │   ├── SettingsView.axaml.cs
│   │   │   ├── ProcessingView.axaml
│   │   │   ├── ProcessingView.axaml.cs
│   │   │   └── LogViewerControl.axaml
│   │   ├── ViewModels/
│   │   │   ├── ViewModelBase.cs
│   │   │   ├── MainWindowViewModel.cs
│   │   │   ├── SettingsViewModel.cs
│   │   │   ├── ProcessingViewModel.cs
│   │   │   └── LogViewerViewModel.cs
│   │   ├── Services/
│   │   │   ├── UILoggingService.cs
│   │   │   ├── ProgressReportingService.cs
│   │   │   └── DialogService.cs
│   │   ├── Converters/
│   │   │   ├── BoolToColorConverter.cs
│   │   │   └── StatusToIconConverter.cs
│   │   ├── Assets/
│   │   │   ├── Icons/
│   │   │   └── Fonts/
│   │   └── BulkIn.Desktop.csproj
│   │
│   └── BulkIn.Tests/                 [NEW - OPTIONAL]
│       └── ... (unit tests)
│
├── scripts/
│   └── DatabaseSetup.sql
├── logs/
└── BulkIn.sln                        [UPDATED - ADD NEW PROJECTS]
```

### Dependency Flow

```
┌─────────────────────────────────────────┐
│     BulkIn.Console (Unchanged)          │
│  - Uses inline service instances        │
│  - Direct Program.cs orchestration      │
└─────────────────────────────────────────┘
                    ↓
        [Independent Operation]

┌─────────────────────────────────────────┐
│          BulkIn.Core (Shared)           │
│  - Configuration Models                 │
│  - Service Interfaces & Implementations │
│  - Business Logic & Workflows           │
│  - Data Models & Utilities              │
└─────────────────────────────────────────┘
            ↑                 ↑
            │                 │
    ┌───────┘                 └───────┐
    │                                 │
┌───────────────┐           ┌─────────────────┐
│  Console App  │           │  Desktop App    │
│  (Existing)   │           │  (New Avalonia) │
└───────────────┘           └─────────────────┘
```

---

## Shared Core Library Design

### Strategy: Extract Without Disruption

**Phase 1: File Copying (Not Moving)**
1. Copy existing code from `BulkIn.Console/` to `BulkIn.Core/`
2. Console app continues using its original files
3. Core library becomes independent copy
4. Zero risk to console functionality

**Phase 2: Interface Abstraction**
Create interfaces for all services to enable:
- Dependency injection in Desktop app
- Unit testing
- Future extensibility
- Mock implementations for testing

### Core Library Components

#### 1. Configuration Module
**Files**: All `*Settings.cs` + `ConfigurationLoader.cs`
**Purpose**: Shared configuration binding and validation
**Dependencies**: `Microsoft.Extensions.Configuration.*`

#### 2. Models Module
**Files**: `TextFileRecord.cs`, `ProcessingResult.cs`, `FileProcessingStats.cs`
**Purpose**: Data transfer objects and result tracking
**Dependencies**: None (pure POCOs)

#### 3. Services Module
**Files**: All service implementations + interfaces
**Purpose**: Core business logic and workflow orchestration

**Key Interfaces to Create**:

```csharp
// IFileProcessor.cs (already exists as interface)
public interface IFileProcessor
{
    Task<ProcessingResult> ProcessFileAsync(string filePath);
    Task<FileProcessingStats> ProcessFilesAsync(List<string> filePaths);
    bool ValidatePrerequisites(out List<string> errorMessages);
}

// ILoggingService.cs (new)
public interface ILoggingService : IDisposable
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
    void LogFileSuccess(ProcessingResult result);
    void LogFileFailure(ProcessingResult result);
    void LogBatchComplete(FileProcessingStats stats);
    
    // For UI integration
    event EventHandler<LogEntryEventArgs>? LogEntryAdded;
}

// IProgressReporter.cs (new - for UI updates)
public interface IProgressReporter
{
    event EventHandler<ProgressEventArgs>? ProgressChanged;
    event EventHandler<StatusEventArgs>? StatusChanged;
    void ReportProgress(int fileIndex, int totalFiles, long rowsProcessed);
    void ReportStatus(ProcessingStatus status, string message);
}
```

#### 4. Utilities Module
**Files**: `FileHelper.cs`, `SqlConnectionFactory.cs`
**Purpose**: Helper functions and factory patterns
**Dependencies**: `Microsoft.Data.SqlClient`

### Console App Adaptation (Optional - Phase 3)

**IF** approved after Desktop app is stable, we can optionally:
1. Add reference to `BulkIn.Core` in console project
2. Replace inline code with library references
3. Maintain identical behavior

**Benefits**: Single source of truth, easier maintenance
**Risk**: Low (extensive testing required)
**Timeline**: Post-Desktop release

---

## Avalonia UI Application Design

### MVVM Architecture Pattern

```
View (XAML) ← Binding → ViewModel (Logic) → Service (Business Logic)
     ↓                        ↓                      ↓
 UI Events              Commands & Props        Core Library
```

### Key ViewModels

#### 1. MainWindowViewModel
**Responsibilities**:
- Application shell orchestration
- Navigation between views
- Global status management
- Window lifecycle

**Properties**:
```csharp
public ICommand NavigateToProcessingCommand { get; }
public ICommand NavigateToSettingsCommand { get; }
public string ApplicationTitle { get; set; }
public string CurrentView { get; set; }
public bool IsProcessing { get; set; }
```

#### 2. ProcessingViewModel
**Responsibilities**:
- File processing orchestration
- Real-time progress updates
- Control panel management (Start/Stop/Pause)
- Status indicators

**Properties**:
```csharp
// Configuration
public string SourcePath { get; set; }
public string DatabaseServer { get; set; }

// State
public ObservableCollection<FileStatusItem> FileList { get; }
public ProcessingStatus CurrentStatus { get; set; }
public int FilesCompleted { get; set; }
public int TotalFiles { get; set; }
public long TotalRowsProcessed { get; set; }
public double ProgressPercentage { get; set; }

// Current File Progress
public string CurrentFileName { get; set; }
public long CurrentFileRows { get; set; }
public double CurrentFileSpeed { get; set; }

// Commands
public ICommand StartProcessingCommand { get; }
public ICommand StopProcessingCommand { get; }
public ICommand PauseProcessingCommand { get; }
public ICommand BrowseFolderCommand { get; }
public ICommand RefreshFilesCommand { get; }
```

**Integration with Core**:
```csharp
private readonly IFileProcessor _fileProcessor;
private readonly ILoggingService _loggingService;
private readonly IProgressReporter _progressReporter;
private CancellationTokenSource _cancellationTokenSource;

public async Task StartProcessingAsync()
{
    _cancellationTokenSource = new CancellationTokenSource();
    CurrentStatus = ProcessingStatus.Running;
    
    var files = DiscoverFiles();
    var stats = await _fileProcessor.ProcessFilesAsync(files);
    
    CurrentStatus = ProcessingStatus.Completed;
}
```

#### 3. SettingsViewModel
**Responsibilities**:
- Configuration management (read/write appsettings.json)
- Validation before save
- Database connection testing
- File path validation

**Properties**:
```csharp
// Database Settings
public string ServerName { get; set; }
public string DatabaseName { get; set; }
public bool UseTrustedConnection { get; set; }
public string Username { get; set; }
public string Password { get; set; }

// File Settings
public string SourceFilePath { get; set; }
public ObservableCollection<string> FilePatterns { get; }
public bool ProcessInAlphabeticalOrder { get; set; }

// Processing Settings
public int BatchSize { get; set; }
public bool EnableTransactionPerFile { get; set; }
public bool ContinueOnError { get; set; }

// Commands
public ICommand TestConnectionCommand { get; }
public ICommand BrowseFolderCommand { get; }
public ICommand SaveSettingsCommand { get; }
public ICommand ResetDefaultsCommand { get; }
```

#### 4. LogViewerViewModel
**Responsibilities**:
- Real-time log streaming
- Log filtering and search
- Auto-scroll management
- Export logs

**Properties**:
```csharp
public ObservableCollection<LogEntry> LogEntries { get; }
public string SearchFilter { get; set; }
public LogLevel MinimumLogLevel { get; set; }
public bool AutoScroll { get; set; }
public int MaxLogEntries { get; set; } = 1000;

// Commands
public ICommand ClearLogsCommand { get; }
public ICommand ExportLogsCommand { get; }
public ICommand CopySelectedCommand { get; }
```

### Service Layer (Desktop-Specific)

#### UILoggingService
Wraps core `LoggingService` and adds UI-specific features:
```csharp
public class UILoggingService : ILoggingService
{
    private readonly LoggingService _coreLogging;
    
    public event EventHandler<LogEntryEventArgs>? LogEntryAdded;
    
    public void LogInfo(string message)
    {
        _coreLogging.LogInfo(message);
        LogEntryAdded?.Invoke(this, new LogEntryEventArgs
        {
            Level = LogLevel.Information,
            Message = message,
            Timestamp = DateTime.Now
        });
    }
    
    // ... implement other methods
}
```

#### ProgressReportingService
Bridges service layer to UI:
```csharp
public class ProgressReportingService : IProgressReporter
{
    public event EventHandler<ProgressEventArgs>? ProgressChanged;
    public event EventHandler<StatusEventArgs>? StatusChanged;
    
    // Inject into FileProcessorService constructor
    // Report progress during file processing
}
```

#### DialogService
Platform-independent dialog abstraction:
```csharp
public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string message);
    Task ShowErrorAsync(string message);
    Task<string?> ShowFolderPickerAsync();
}
```

---

## UI/UX Design Specification

### Design System

#### Color Palette
```
Primary Colors:
- Primary Blue:      #0078D4 (Accent, buttons, highlights)
- Success Green:     #10B981 (Completed status)
- Warning Orange:    #F59E0B (In-progress, warnings)
- Error Red:         #EF4444 (Errors, failures)
- Info Cyan:         #06B6D4 (Information messages)

Neutral Colors:
- Background:        #FFFFFF (Light) / #1E1E1E (Dark)
- Surface:           #F9FAFB (Light) / #2D2D2D (Dark)
- Border:            #E5E7EB (Light) / #3F3F3F (Dark)
- Text Primary:      #111827 (Light) / #F9FAFB (Dark)
- Text Secondary:    #6B7280 (Light) / #9CA3AF (Dark)
```

#### Typography
```
Font Family: "Segoe UI", "San Francisco", "Roboto", system-ui
Headings:    16-24pt, SemiBold
Body:        14pt, Regular
Labels:      12pt, Medium
Captions:    11pt, Regular
```

#### Spacing System
```
Base Unit: 8px
xs: 4px    (tight spacing)
sm: 8px    (compact controls)
md: 16px   (default spacing)
lg: 24px   (section separation)
xl: 32px   (major divisions)
```

#### Component Styling

**Buttons**:
```
Height: 36px
Padding: 8px 16px
Border Radius: 6px
Font: 14pt Medium

Primary Button:
- Background: #0078D4
- Text: White
- Hover: #106EBE
- Icon: Left-aligned, 16x16px

Secondary Button:
- Background: Transparent
- Border: 1px solid #E5E7EB
- Text: #6B7280
- Hover: Background #F9FAFB

Disabled Button:
- Opacity: 0.5
- Cursor: not-allowed
```

**Input Fields**:
```
Height: 36px
Padding: 8px 12px
Border: 1px solid #E5E7EB
Border Radius: 6px
Focus: Border #0078D4, Shadow 0 0 0 3px rgba(0,120,212,0.1)
```

**Status Indicators**:
```
Size: 12x12px circle or icon
Colors: Match status (green/yellow/red/blue)
Animation: Pulse effect for "In Progress"
Position: Left of status text
```

### Layout Structure

#### Main Window (1200x800px default)

```
┌─────────────────────────────────────────────────────────────┐
│  [🔷 BulkIn]                                    [_ □ ✕]     │ ← Title Bar (40px)
├─────────────────────────────────────────────────────────────┤
│  ┌────────┬────────┐                                        │
│  │ Process│Settings│  Navigation Tabs (48px)                │
│  └────────┴────────┘                                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                    [Content Area]                           │ ← 712px
│                  (Dynamic View)                             │
│                                                             │
│                                                             │
│                                                             │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  ⓘ Ready  •  16 files discovered  •  Database: Connected   │ ← Status Bar (32px)
└─────────────────────────────────────────────────────────────┘
```

#### Processing View Layout

```
┌─────────────────────────────────────────────────────────────┐
│  ┌─── Control Panel ───────────────────────────────────┐   │
│  │  📁 Source: D:\SourceFiles           [Browse...]    │   │
│  │  🗄️  Database: MATRIX\MATRIX                         │   │
│  │                                                      │   │
│  │  [▶️ Start Processing]  [⏸️ Pause]  [⏹️ Stop]        │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─── Progress Overview ───────────────────────────────┐   │
│  │  Status: ⚙️ Processing...                            │   │
│  │  Files:  █████████████░░░░░░░  8 / 16  (50%)       │   │
│  │  Rows:   1,234,567  •  45,320 rows/sec              │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─── File List ───────────────────────────────────────┐   │
│  │  ✅ file001.txt       923,843 rows   [38,983/sec]   │   │
│  │  ✅ file002.txt       856,234 rows   [42,156/sec]   │   │
│  │  ⚙️ file003.txt       234,567 rows   [45,320/sec]   │   │ ← Scrollable
│  │  ⏳ file004.txt       Pending...                     │   │
│  │  ⏳ file005.txt       Pending...                     │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─── Live Logs ───────────────────────────────────────┐   │
│  │  [14:32:15] ℹ️ Started processing file003.txt        │   │
│  │  [14:32:16] ℹ️ Bulk insert: 200,000 rows...         │   │ ← Auto-scroll
│  │  [14:32:18] ✅ Transfer complete: 234,567 rows       │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

#### Settings View Layout

```
┌─────────────────────────────────────────────────────────────┐
│  ┌─── Database Connection ─────────────────────────────┐   │
│  │  Server Name:     [MATRIX\MATRIX____________]       │   │
│  │  Database Name:   [RAW_PROCESS______________]       │   │
│  │  Authentication:  ○ Windows  ● SQL Server           │   │
│  │  Username:        [sa_______________________]       │   │
│  │  Password:        [••••••••••••••••••••••••]       │   │
│  │                                                      │   │
│  │  [🔌 Test Connection]                               │   │
│  │  Status: ✅ Connected successfully                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─── File Processing ─────────────────────────────────┐   │
│  │  Source Folder:   [D:\SourceFiles_______] [Browse]  │   │
│  │  File Patterns:   [*.txt, *.csv_________]           │   │
│  │  Sort Order:      ☑ Alphabetical                    │   │
│  │                                                      │   │
│  │  Batch Size:      [200,000_] rows                   │   │
│  │  Options:         ☑ Transaction per file            │   │
│  │                   ☑ Continue on error                │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─── Logging ──────────────────────────────────────────┐   │
│  │  Log Directory:   [D:\BulkIn\logs___] [Browse]      │   │
│  │  Log Level:       [Information ▼]                    │   │
│  │  Console Logging: ☑ Enabled                          │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  [💾 Save Settings]  [🔄 Reset to Defaults]                │
└─────────────────────────────────────────────────────────────┘
```

### Icon System

**Fluent UI Icons** (16x16px, scalable SVG):
- ▶️ Play (Start Processing)
- ⏸️ Pause (Pause Processing)
- ⏹️ Stop (Stop Processing)
- 📁 Folder (Browse folders)
- 🔄 Refresh (Refresh file list)
- 💾 Save (Save settings)
- 🔌 Database (Connection test)
- ✅ Check (Success)
- ❌ Error (Failure)
- ⚙️ Gear (In Progress)
- ⏳ Clock (Pending)
- ℹ️ Info (Information)
- ⚠️ Warning (Warning)
- 🔍 Search (Log search)

### Responsive Behavior

**Minimum Window Size**: 1024x768px
**Maximum Window Size**: Unlimited (scales content)
**Resizing**:
- File list height adjusts dynamically
- Log viewer takes remaining vertical space
- Horizontal scrolling disabled (wrapping enabled)

### Animations & Transitions

```
Button Hover: 150ms ease-in-out
View Transitions: 200ms fade
Progress Updates: Immediate (no delay)
Status Changes: 100ms color fade
Pulse Animation (In Progress): 2s infinite
```

---

## Visual Mockups & Wireframes

### Main Window - Processing View (Light Theme)

```
╔═══════════════════════════════════════════════════════════════╗
║  🔷 BulkIn - Bulk Text File Ingestion System      [_ □ ✕]    ║
╠═══════════════════════════════════════════════════════════════╣
║  ┏━━━━━━━━━┓┌─────────┐                                      ║
║  ┃ Process ┃│Settings │                                      ║
║  ┗━━━━━━━━━┛└─────────┘                                      ║
╠═══════════════════════════════════════════════════════════════╣
║  ┌──────────────────────────────────────────────────────────┐║
║  │  📁 Source: D:\Project_TextFile\SourceFiles  [Browse...] │║
║  │  🗄️  Database: MATRIX\MATRIX → RAW_PROCESS               │║
║  │                                                           │║
║  │  ┌─────────────────────────────────────────────────────┐ │║
║  │  │ ▶️ Start Processing │ ⏸️ Pause │ ⏹️ Stop │ 🔄 Refresh│ │║
║  │  └─────────────────────────────────────────────────────┘ │║
║  └──────────────────────────────────────────────────────────┘║
║                                                               ║
║  ┌─ Progress ─────────────────────────────────────────────┐ ║
║  │  Status: ⚙️  Processing file003.txt                     │ ║
║  │  ╔════════════════════════════════════════╗ 50% (8/16) │ ║
║  │  ║████████████████████░░░░░░░░░░░░░░░░░░░║             │ ║
║  │  ╚════════════════════════════════════════╝             │ ║
║  │  Total Rows: 1,234,567  •  Speed: 45,320 rows/sec      │ ║
║  └───────────────────────────────────────────────────────┘ ║
║                                                               ║
║  ┌─ Files (16 total) ───────────────────────────────────┐  ║
║  │  ✅ ACEFOI202508011.txt   923,843 rows  38.9k/s  23s  │  ║
║  │  ✅ ACEFOI20250804.txt    856,234 rows  42.1k/s  20s  │  ║
║  │  ⚙️  ACEFOI202508042.txt  234,567 rows  45.3k/s  ···  │  ║
║  │  ⏳ ACEFOI20250807.txt    Pending...                  │  ║
║  │  ⏳ LCEFOI20250802.txt    Pending...                  │  ║
║  └───────────────────────────────────────────────────────┘  ║
║                                                               ║
║  ┌─ Live Logs ──────────────────────────── [🔍] [📋] [✕]─┐  ║
║  │  [14:32:15] ℹ️ Processing file003.txt...              │  ║
║  │  [14:32:16] ℹ️ Preparing temp table... ✅              │  ║
║  │  [14:32:17] ℹ️ Bulk insert: 200,000 rows...           │  ║
║  │  [14:32:19] ✅ Inserted: 234,567 rows                  │  ║
║  │  [14:32:20] ℹ️ Transferring to target... ✅            │  ║
║  └───────────────────────────────────────────────────────┘  ║
╠═══════════════════════════════════════════════════════════════╣
║  ⓘ Ready  •  16 files  •  DB: Connected  •  Last: 14:32:20  ║
╚═══════════════════════════════════════════════════════════════╝
```

### Settings View (Light Theme)

```
╔═══════════════════════════════════════════════════════════════╗
║  🔷 BulkIn - Bulk Text File Ingestion System      [_ □ ✕]    ║
╠═══════════════════════════════════════════════════════════════╣
║  ┌─────────┐┏━━━━━━━━━┓                                      ║
║  │ Process ││Settings │                                      ║
║  └─────────┘┗━━━━━━━━━┛                                      ║
╠═══════════════════════════════════════════════════════════════╣
║  ┌─ Database Connection ──────────────────────────────────┐  ║
║  │  Server Name:     [MATRIX\MATRIX_________________]     │  ║
║  │  Database Name:   [RAW_PROCESS____________________]    │  ║
║  │  Connection TO:   [30___] sec  Command TO: [600__] sec│  ║
║  │                                                         │  ║
║  │  Authentication:  ☑ Windows (Trusted)                  │  ║
║  │                   ☐ SQL Server                         │  ║
║  │                                                         │  ║
║  │  ┌──────────────────────────────────────────────────┐ │  ║
║  │  │ 🔌 Test Connection                               │ │  ║
║  │  └──────────────────────────────────────────────────┘ │  ║
║  │  Status: ✅ Connected to MATRIX\MATRIX successfully   │  ║
║  └─────────────────────────────────────────────────────────┘  ║
║                                                               ║
║  ┌─ File Processing ──────────────────────────────────────┐  ║
║  │  Source Folder:                                        │  ║
║  │  [D:\Project_TextFile\SourceFiles________] [Browse...] │  ║
║  │                                                         │  ║
║  │  File Patterns:   [*.txt, *.csv__________________]     │  ║
║  │  Exclude:         [*_backup.*, *_temp.*__________]     │  ║
║  │                                                         │  ║
║  │  Sort Order:      ☑ Process in alphabetical order     │  ║
║  └─────────────────────────────────────────────────────────┘  ║
║                                                               ║
║  ┌─ Processing Options ───────────────────────────────────┐  ║
║  │  Batch Size:      [200,000_______] rows per batch      │  ║
║  │  Buffer Size:     [65,536________] bytes               │  ║
║  │                                                         │  ║
║  │  Options:                                               │  ║
║  │  ☑ Enable transaction per file (rollback on failure)  │  ║
║  │  ☑ Continue processing after file errors              │  ║
║  └─────────────────────────────────────────────────────────┘  ║
║                                                               ║
║  ┌─ Logging ───────────────────────────────────────────────┐ ║
║  │  Log Directory:  [D:\BulkIn\logs_______] [Browse...]   │ ║
║  │  Log Level:      [Information        ▼]                │ ║
║  │  Console Output: ☑ Enabled                             │ ║
║  └─────────────────────────────────────────────────────────┘ ║
║                                                               ║
║  ┌──────────────────────────────────────────────────────┐   ║
║  │    💾 Save Settings    │    🔄 Reset to Defaults    │   ║
║  └──────────────────────────────────────────────────────┘   ║
╠═══════════════════════════════════════════════════════════════╣
║  ⓘ Settings not saved  •  Click Save to apply changes        ║
╚═══════════════════════════════════════════════════════════════╝
```

### Dark Theme Variant

**Key Changes**:
- Background: #1E1E1E
- Surface: #2D2D2D
- Text: #F9FAFB
- Borders: #3F3F3F
- Accent colors remain vibrant
- Status icons maintain full color

---

## Implementation Roadmap

### Phase 1: Core Library Extraction (Week 1)
**Deliverables**:
- ✅ Create `BulkIn.Core` project
- ✅ Copy all models, services, utilities
- ✅ Create service interfaces
- ✅ Add NuGet packages
- ✅ Build and test library independently

**Tasks**:
1. Create new Class Library project (net8.0)
2. Copy files maintaining folder structure
3. Add interface definitions
4. Resolve dependencies
5. Compile without errors
6. Basic unit tests (optional)

**Success Criteria**:
- Core library builds successfully
- Zero console app modifications
- All interfaces defined
- Configuration binding works

---

### Phase 2: Avalonia Foundation (Week 1-2)
**Deliverables**:
- ✅ Create Avalonia Desktop project
- ✅ Set up MVVM infrastructure
- ✅ Configure dependency injection
- ✅ Implement basic navigation
- ✅ Create main window shell

**Tasks**:
1. Create Avalonia app from template
2. Add NuGet packages:
   - Avalonia 11.x
   - Avalonia.Desktop
   - Microsoft.Extensions.DependencyInjection
   - CommunityToolkit.Mvvm
3. Set up ViewModelBase with ReactiveUI
4. Configure DI container in Program.cs
5. Create MainWindow with tab navigation
6. Implement basic theme (light/dark toggle)

**Success Criteria**:
- App launches without errors
- Navigation between tabs works
- DI container resolves services
- Settings load from appsettings.json

---

### Phase 3: Settings View Implementation (Week 2)
**Deliverables**:
- ✅ Complete SettingsView UI
- ✅ Configuration editing functionality
- ✅ Validation and saving
- ✅ Connection testing

**Tasks**:
1. Design XAML layout per mockup
2. Implement SettingsViewModel
3. Two-way data binding to AppSettings
4. Test connection button logic
5. Save to appsettings.json
6. Validation messages
7. Browse folder dialogs

**Success Criteria**:
- All settings editable in UI
- Changes persist to file
- Connection test works
- Input validation active
- Professional appearance

---

### Phase 4: Processing View - Core UI (Week 3)
**Deliverables**:
- ✅ ProcessingView layout
- ✅ File discovery and listing
- ✅ Control panel (buttons)
- ✅ Basic status display

**Tasks**:
1. Design XAML layout
2. Implement ProcessingViewModel
3. File discovery integration
4. Command handlers (Start/Stop/Pause)
5. File list with status icons
6. Progress bar components
7. Real-time stats display

**Success Criteria**:
- Files discovered and listed
- Buttons enabled/disabled correctly
- Status updates in real-time
- Clean, professional UI
- Responsive layout

---

### Phase 5: Processing Logic Integration (Week 3-4)
**Deliverables**:
- ✅ Full integration with Core library
- ✅ Real-time progress updates
- ✅ File-by-file status tracking
- ✅ Error handling

**Tasks**:
1. Inject IFileProcessor into ViewModel
2. Implement progress reporting
3. Update UI on progress events
4. Handle cancellation (Stop/Pause)
5. Update file list status
6. Display row counts and speed
7. Handle errors gracefully

**Success Criteria**:
- Files process end-to-end
- Progress updates smoothly
- Pause/Resume works
- Stop cancels cleanly
- Errors don't crash app
- Performance matches console

---

### Phase 6: Log Viewer Implementation (Week 4)
**Deliverables**:
- ✅ Live log viewer control
- ✅ Filtering and search
- ✅ Auto-scroll management
- ✅ Export functionality

**Tasks**:
1. Create LogViewerControl
2. Implement UILoggingService
3. Observable log collection
4. Auto-scroll with manual override
5. Search/filter logic
6. Copy/export commands
7. Color-coded log levels

**Success Criteria**:
- Logs stream in real-time
- Search works accurately
- Auto-scroll toggleable
- Performance with 1000+ entries
- Export to file works

---

### Phase 7: Polish & Testing (Week 5)
**Deliverables**:
- ✅ Final UI polish
- ✅ Comprehensive testing
- ✅ Documentation
- ✅ Installer/Package

**Tasks**:
1. Apply final styling
2. Add tooltips
3. Keyboard shortcuts
4. Accessibility features
5. End-to-end testing
6. Performance optimization
7. User documentation
8. Create installer (optional)

**Success Criteria**:
- No known bugs
- Meets all design specs
- Performance acceptable
- User documentation complete
- Ready for production use

---

### Phase 8: Console Migration (Optional, Post-Release)
**Timeline**: 2-4 weeks after Desktop stable
**Deliverables**:
- Console app references Core library
- Identical behavior maintained
- Single codebase for logic

**Only if approved after successful Desktop deployment**

---

## Risk Assessment & Mitigation

### Risk Matrix

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Console app inadvertently modified | Low | Critical | Git branch protection, file copying vs. moving |
| Performance degradation in UI | Medium | High | Async operations, background threading, progress throttling |
| Configuration incompatibility | Low | Medium | Shared configuration models, validation |
| Memory leaks in UI | Medium | Medium | Proper disposal, weak event handlers, testing |
| Cross-platform issues | Medium | Low | Target Windows first, test on macOS/Linux later |
| User adoption resistance | Low | Low | Keep console app available, gradual rollout |

### Mitigation Strategies

#### 1. Console Protection
- **Strategy**: Work in separate Git branch until approval
- **Verification**: Console app never referenced in commits
- **Testing**: Run console app after each phase
- **Rollback**: Keep console app in master branch pristine

#### 2. Performance Monitoring
- **Strategy**: Profile with large files (500MB+)
- **Benchmarks**: Must match console app speed (±10%)
- **Optimization**: 
  - Background threads for I/O
  - UI updates throttled to 100ms
  - Virtual scrolling for large lists
  - Async/await throughout

#### 3. Memory Management
- **Strategy**: Implement IDisposable correctly
- **Testing**: Run overnight with memory profiler
- **Best Practices**:
  - Weak event handlers
  - Dispose of services
  - Clear large collections
  - No circular references

#### 4. Configuration Compatibility
- **Strategy**: Use same AppSettings classes
- **Validation**: Test with production config files
- **Migration**: Desktop and Console share appsettings.json
- **Versioning**: Handle missing/new settings gracefully

#### 5. Testing Strategy
- **Unit Tests**: Core library service methods
- **Integration Tests**: Full workflow with test database
- **UI Tests**: Avalonia UI testing framework
- **Manual Tests**: Real files, real database, real scenarios
- **Regression Tests**: Console app unchanged after each phase

---

## Appendix

### Dependencies Summary

#### BulkIn.Core
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.2" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

#### BulkIn.Desktop
```xml
<PackageReference Include="Avalonia" Version="11.1.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.1.0" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.1.0" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.1.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<ProjectReference Include="..\BulkIn.Core\BulkIn.Core.csproj" />
```

### Estimated Timeline

| Phase | Duration | Cumulative |
|-------|----------|------------|
| Phase 1: Core Library | 5 days | 1 week |
| Phase 2: Avalonia Foundation | 5 days | 2 weeks |
| Phase 3: Settings View | 5 days | 3 weeks |
| Phase 4: Processing View UI | 5 days | 4 weeks |
| Phase 5: Processing Logic | 5 days | 5 weeks |
| Phase 6: Log Viewer | 5 days | 6 weeks |
| Phase 7: Polish & Testing | 5 days | 7 weeks |
| **Total** | **35 days** | **~8 weeks** |

*Assumes single developer, full-time commitment*

---

## Next Steps - Awaiting Approval

### Before Proceeding, Please Review:

1. ✅ **Architecture Approach**: Does the Core Library extraction strategy meet requirements?
2. ✅ **UI Design**: Do the mockups align with expectations for aesthetics and usability?
3. ✅ **Feature Scope**: Are all required features covered (Settings, Processing, Logs)?
4. ✅ **Risk Mitigation**: Are protections for console app sufficient?
5. ✅ **Timeline**: Is 8-week timeline acceptable?

### Decision Points:

- **Approve**: Proceed to Phase 1 (Core Library extraction)
- **Revise**: Provide feedback on specific areas
- **Alternative**: Suggest different approach or technology

### Contact for Questions:
Please provide feedback on:
- UI design preferences
- Feature priorities
- Timeline constraints
- Technical concerns

---

**Document Status**: 📋 DRAFT - Awaiting Stakeholder Approval  
**Version**: 1.0  
**Date**: November 2, 2025  
**Author**: AI Development Team  

---

## Visual Design Reference

### Color Usage Examples

**Status Colors in UI**:
```
✅ Success (Green #10B981):  Completed files, successful operations
⚙️  In Progress (Orange #F59E0B): Currently processing
⏳ Pending (Gray #9CA3AF): Queued files
❌ Error (Red #EF4444): Failed operations
ℹ️  Info (Cyan #06B6D4): Informational messages
```

**Button States**:
```
Primary Action (Start):
- Default: #0078D4 background, white text
- Hover: #106EBE background
- Pressed: #005A9E background
- Disabled: 50% opacity

Danger Action (Stop):
- Default: #EF4444 background, white text
- Hover: #DC2626 background
- Pressed: #B91C1C background
```

### Component Catalog

**Progress Bar**:
```
Height: 24px
Background: #E5E7EB (Light) / #3F3F3F (Dark)
Fill: Linear gradient #0078D4 → #06B6D4
Border Radius: 12px
Animation: Smooth fill transition 300ms
Label: Percentage + counts above bar
```

**Status Badge**:
```
Height: 24px
Padding: 4px 8px
Border Radius: 12px
Font: 12pt Medium

Success: #D1FAE5 bg, #059669 text
Warning: #FEF3C7 bg, #D97706 text
Error: #FEE2E2 bg, #DC2626 text
Info: #DBEAFE bg, #2563EB text
```

**File List Item**:
```
Height: 48px
Padding: 12px 16px
Border Bottom: 1px solid #E5E7EB
Hover: Background #F9FAFB

Layout:
[Icon] [Filename]                    [Status] [Stats]
 16px   Flex-grow                    Badge    Right-align
```

This comprehensive plan provides the complete blueprint for developing the Avalonia desktop application while maintaining zero impact on the existing console application. Please review and provide feedback before development commences.
