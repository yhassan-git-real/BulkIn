-- =============================================
-- BulkIn - Database Setup Script
-- Description: Creates required tables in existing database
-- Version: 1.1
-- Date: October 31, 2025
-- Target Database: RAW_PROCESS (existing)
-- =============================================

-- NOTE: This script assumes RAW_PROCESS database already exists
-- It will only create the tables if they don't exist

USE RAW_PROCESS;
GO

PRINT '═══════════════════════════════════════════════════════════';
PRINT 'BulkIn - Database Setup for RAW_PROCESS';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '';
GO

-- =============================================
-- Create Target Table: TextFileData
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TextFileData]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TextFileData] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [Data] NVARCHAR(MAX) NOT NULL,
        [Filename] NVARCHAR(255) NOT NULL,
        [Date] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_TextFileData] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    
    PRINT 'Table TextFileData created successfully.';
    
    -- Create non-clustered index on Filename for faster lookups
    CREATE NONCLUSTERED INDEX [IX_TextFileData_Filename] 
    ON [dbo].[TextFileData] ([Filename] ASC);
    
    -- Create non-clustered index on Date for date-based queries
    CREATE NONCLUSTERED INDEX [IX_TextFileData_Date] 
    ON [dbo].[TextFileData] ([Date] ASC);
    
    PRINT 'Indexes created on TextFileData.';
END
ELSE
BEGIN
    PRINT 'Table TextFileData already exists.';
END
GO

-- =============================================
-- NOTE: TempTextFileData Table
-- =============================================
-- The temp staging table (TempTextFileData) is automatically
-- managed by the BulkIn application. It will be created/recreated
-- at runtime before each batch processing session.
-- No manual creation required!
--
-- Structure: ID (INT IDENTITY), Data (NVARCHAR(MAX)), Filename (NVARCHAR(255))
-- =============================================

PRINT '';
PRINT 'NOTE: Temp table (TempTextFileData) will be created automatically by the application.';
PRINT '';

-- =============================================
-- Verify Table Creation & Display Current Row Counts
-- =============================================
PRINT '';
PRINT 'Verifying target table...';
PRINT '';

SELECT 
    'TextFileData' AS TableName,
    (SELECT COUNT(*) FROM TextFileData) AS CurrentRowCount,
    'Target Table - Stores final data with Filename and Date' AS Description;
GO

PRINT '';
PRINT '✓ Target table verified successfully in RAW_PROCESS database';
PRINT '';
GO

-- =============================================
-- Optional: Create Stored Procedure for Data Transfer
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_TransferDataFromTemp]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_TransferDataFromTemp];
END
GO

CREATE PROCEDURE [dbo].[usp_TransferDataFromTemp]
    @Filename NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowCount INT;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Transfer data from temp to target table
        -- Filename comes from temp table (already populated during bulk load)
        INSERT INTO [dbo].[TextFileData] ([Data], [Filename], [Date])
        SELECT 
            [Data],
            [Filename],
            GETDATE()
        FROM [dbo].[TempTextFileData];
        
        SET @RowCount = @@ROWCOUNT;
        
        -- Clear temp table
        TRUNCATE TABLE [dbo].[TempTextFileData];
        
        COMMIT TRANSACTION;
        
        -- Return success status
        SELECT 
            @RowCount AS RowsTransferred,
            'SUCCESS' AS Status,
            @Filename AS Filename;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Return error information
        SELECT 
            0 AS RowsTransferred,
            'ERROR' AS Status,
            ERROR_MESSAGE() AS ErrorMessage;
            
        THROW;
    END CATCH
END
GO

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '✓ BulkIn Database Setup Complete!';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'Database: RAW_PROCESS';
PRINT '';
PRINT 'Tables Created:';
PRINT '  ✓ TextFileData (Target Table)';
PRINT '';
PRINT 'Managed by Application:';
PRINT '  ⚙ TempTextFileData (Auto-created at runtime)';
PRINT '';
PRINT 'Stored Procedures:';
PRINT '  ✓ usp_TransferDataFromTemp';
PRINT '';
PRINT 'The BulkIn application is now ready to process files!';
PRINT 'The temp table will be automatically managed during processing.';
PRINT '═══════════════════════════════════════════════════════════';
GO
