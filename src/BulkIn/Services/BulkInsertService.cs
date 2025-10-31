using Microsoft.Data.SqlClient;
using BulkIn.Configuration;
using BulkIn.Utilities;

namespace BulkIn.Services
{
    /// <summary>
    /// Service for bulk inserting data into SQL Server using SqlBulkCopy
    /// </summary>
    public class BulkInsertService
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly ProcessingSettings _processingSettings;
        private readonly SqlConnectionFactory _connectionFactory;

        /// <summary>
        /// Event raised when rows are copied (for progress reporting)
        /// </summary>
        public event EventHandler<long>? RowsCopied;

        /// <summary>
        /// Initializes a new instance of BulkInsertService
        /// </summary>
        public BulkInsertService(
            DatabaseSettings databaseSettings, 
            ProcessingSettings processingSettings,
            SqlConnectionFactory connectionFactory)
        {
            _databaseSettings = databaseSettings ?? throw new ArgumentNullException(nameof(databaseSettings));
            _processingSettings = processingSettings ?? throw new ArgumentNullException(nameof(processingSettings));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Bulk inserts file lines into the temporary table using streaming
        /// </summary>
        /// <param name="lines">Enumerable of file lines to insert</param>
        /// <param name="tableName">Target table name (typically temp table)</param>
        /// <param name="filename">Source filename to include in each row</param>
        /// <param name="transaction">Optional SQL transaction for transactional operations</param>
        /// <returns>Number of rows inserted</returns>
        public long BulkInsert(
            IEnumerable<string> lines, 
            string tableName,
            string filename,
            SqlTransaction? transaction = null)
        {
            long totalRowsInserted = 0;

            using (var dataReader = new TextFileDataReader(lines, filename))
            {
                SqlConnection? connection = null;
                bool shouldDisposeConnection = false;

                try
                {
                    // Use transaction's connection if provided, otherwise create new
                    if (transaction != null)
                    {
                        connection = transaction.Connection;
                    }
                    else
                    {
                        connection = _connectionFactory.CreateConnectionWithRetry();
                        shouldDisposeConnection = true;
                    }

                    if (connection == null)
                    {
                        throw new InvalidOperationException("Failed to establish database connection.");
                    }

                    // Configure SqlBulkCopy
                    using (var bulkCopy = new SqlBulkCopy(
                        connection, 
                        SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepIdentity,
                        transaction))
                    {
                        bulkCopy.DestinationTableName = $"[dbo].[{tableName}]";
                        bulkCopy.BatchSize = _processingSettings.BatchSize;
                        bulkCopy.BulkCopyTimeout = _databaseSettings.CommandTimeout;
                        bulkCopy.EnableStreaming = true;
                        bulkCopy.NotifyAfter = _processingSettings.BatchSize; // Notify after each batch

                        // Map source columns to destination columns
                        bulkCopy.ColumnMappings.Add("Data", "Data");
                        bulkCopy.ColumnMappings.Add("Filename", "Filename");

                        // Subscribe to progress event
                        bulkCopy.SqlRowsCopied += (sender, e) =>
                        {
                            totalRowsInserted = e.RowsCopied;
                            RowsCopied?.Invoke(this, e.RowsCopied);
                        };

                        // Execute bulk insert
                        bulkCopy.WriteToServer(dataReader);

                        // Get the ACTUAL final row count from the database
                        // (The progress event might not fire for the last partial batch)
                        using (var countCommand = connection.CreateCommand())
                        {
                            countCommand.Transaction = transaction;
                            countCommand.CommandText = $"SELECT COUNT(*) FROM [dbo].[{tableName}]";
                            var actualCount = Convert.ToInt64(countCommand.ExecuteScalar());
                            totalRowsInserted = actualCount;
                        }
                    }

                    return totalRowsInserted;
                }
                finally
                {
                    if (shouldDisposeConnection && connection != null)
                    {
                        connection.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Bulk inserts file lines with transaction management
        /// </summary>
        /// <param name="lines">Enumerable of file lines to insert</param>
        /// <param name="tableName">Target table name</param>
        /// <param name="filename">Source filename to include in each row</param>
        /// <returns>Number of rows inserted</returns>
        public long BulkInsertWithTransaction(IEnumerable<string> lines, string tableName, string filename)
        {
            using (var connection = _connectionFactory.CreateConnectionWithRetry())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var rowsInserted = BulkInsert(lines, tableName, filename, transaction);
                        transaction.Commit();
                        return rowsInserted;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Bulk inserts with progress callback for real-time updates
        /// </summary>
        /// <param name="lines">Enumerable of file lines to insert</param>
        /// <param name="tableName">Target table name</param>
        /// <param name="filename">Source filename to include in each row</param>
        /// <param name="progressCallback">Callback for progress updates (rows copied)</param>
        /// <param name="transaction">Optional SQL transaction</param>
        /// <returns>Number of rows inserted</returns>
        public long BulkInsertWithProgress(
            IEnumerable<string> lines, 
            string tableName,
            string filename,
            Action<long> progressCallback,
            SqlTransaction? transaction = null)
        {
            // Subscribe to the event temporarily
            EventHandler<long> handler = (sender, rowsCopied) => progressCallback(rowsCopied);
            RowsCopied += handler;

            try
            {
                return BulkInsert(lines, tableName, filename, transaction);
            }
            finally
            {
                RowsCopied -= handler;
            }
        }

        /// <summary>
        /// Validates that the target table exists and has the correct schema
        /// </summary>
        /// <param name="tableName">Table name to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateTargetTable(string tableName, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Check if table exists
                if (!_connectionFactory.TableExists(tableName))
                {
                    errorMessage = $"Table '{tableName}' does not exist in the database.";
                    return false;
                }

                // Verify table has 'Data' column with correct type
                using (var connection = _connectionFactory.CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = @TableName AND COLUMN_NAME IN ('Data', 'Filename')
                            ORDER BY COLUMN_NAME";
                        command.Parameters.AddWithValue("@TableName", tableName);

                        int columnCount = 0;
                        bool hasDataColumn = false;
                        bool hasFilenameColumn = false;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columnCount++;
                                var columnName = reader.GetString(0);
                                var dataType = reader.GetString(1);

                                if (columnName.Equals("Data", StringComparison.OrdinalIgnoreCase))
                                {
                                    hasDataColumn = true;
                                    if (!dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
                                    {
                                        errorMessage = $"Table '{tableName}' 'Data' column must be NVARCHAR type, found: {dataType}";
                                        return false;
                                    }
                                }
                                else if (columnName.Equals("Filename", StringComparison.OrdinalIgnoreCase))
                                {
                                    hasFilenameColumn = true;
                                    if (!dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
                                    {
                                        errorMessage = $"Table '{tableName}' 'Filename' column must be NVARCHAR type, found: {dataType}";
                                        return false;
                                    }
                                }
                            }

                            if (!hasDataColumn)
                            {
                                errorMessage = $"Table '{tableName}' does not have a 'Data' column.";
                                return false;
                            }

                            if (!hasFilenameColumn)
                            {
                                errorMessage = $"Table '{tableName}' does not have a 'Filename' column.";
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error validating table '{tableName}': {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Estimates the time required to process a given number of rows
        /// </summary>
        /// <param name="rowCount">Number of rows to process</param>
        /// <returns>Estimated duration</returns>
        public TimeSpan EstimateProcessingTime(long rowCount)
        {
            // Assume average processing speed of 400,000 rows per minute
            const double averageRowsPerSecond = 6666.67; // ~400k per minute
            var estimatedSeconds = rowCount / averageRowsPerSecond;
            return TimeSpan.FromSeconds(estimatedSeconds);
        }
    }
}
