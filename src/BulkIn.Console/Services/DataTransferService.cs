using Microsoft.Data.SqlClient;
using BulkInApp.Configuration;
using BulkInApp.Utilities;

namespace BulkInApp.Services
{
    /// <summary>
    /// Service for transferring data from temporary table to target table
    /// </summary>
    public class DataTransferService
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly SqlConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of DataTransferService
        /// </summary>
        public DataTransferService(
            DatabaseSettings databaseSettings,
            SqlConnectionFactory connectionFactory)
        {
            _databaseSettings = databaseSettings ?? throw new ArgumentNullException(nameof(databaseSettings));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Transfers data from temp table to target table with filename
        /// </summary>
        /// <param name="filename">Name of the source file (for Filename column)</param>
        /// <param name="transaction">Optional SQL transaction for transactional operations</param>
        /// <returns>Number of rows transferred</returns>
        public long TransferData(string filename, SqlTransaction? transaction = null)
        {
            var tempTableName = _databaseSettings.TempTableName;
            var targetTableName = _databaseSettings.TargetTableName;

            return TransferData(tempTableName, targetTableName, filename, transaction);
        }

        /// <summary>
        /// Transfers data from specified temp table to target table
        /// </summary>
        /// <param name="tempTableName">Source temporary table name</param>
        /// <param name="targetTableName">Destination target table name</param>
        /// <param name="filename">Name of the source file</param>
        /// <param name="transaction">Optional SQL transaction</param>
        /// <returns>Number of rows transferred</returns>
        public long TransferData(
            string tempTableName, 
            string targetTableName, 
            string filename,
            SqlTransaction? transaction = null)
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

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandTimeout = _databaseSettings.CommandTimeout;

                    // Transfer data with filename and timestamp
                    command.CommandText = $@"
                        INSERT INTO [dbo].[{targetTableName}] ([Data], [Filename], [Date])
                        SELECT 
                            [Data],
                            @Filename,
                            GETDATE()
                        FROM [dbo].[{tempTableName}]";

                    command.Parameters.AddWithValue("@Filename", filename);

                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected;
                }
            }
            finally
            {
                if (shouldDisposeConnection && connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Transfers data and validates row count matches
        /// </summary>
        /// <param name="filename">Name of the source file</param>
        /// <param name="expectedRowCount">Expected number of rows to transfer</param>
        /// <param name="transaction">Optional SQL transaction</param>
        /// <returns>Transfer result with validation status</returns>
        public (bool Success, long RowsTransferred, string Message) TransferWithValidation(
            string filename, 
            long expectedRowCount,
            SqlTransaction? transaction = null)
        {
            try
            {
                // Get row count from temp table before transfer
                var tempRowCount = _connectionFactory.GetRowCount(_databaseSettings.TempTableName);

                if (tempRowCount != expectedRowCount)
                {
                    return (false, 0, 
                        $"Row count mismatch in temp table. Expected: {expectedRowCount}, Found: {tempRowCount}");
                }

                // Perform transfer
                var rowsTransferred = TransferData(filename, transaction);

                // Validate transfer
                if (rowsTransferred != expectedRowCount)
                {
                    return (false, rowsTransferred,
                        $"Row count mismatch after transfer. Expected: {expectedRowCount}, Transferred: {rowsTransferred}");
                }

                return (true, rowsTransferred, "Transfer successful and validated");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Transfer failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears (truncates) the temporary table
        /// </summary>
        /// <param name="transaction">Optional SQL transaction</param>
        public void ClearTempTable(SqlTransaction? transaction = null)
        {
            ClearTable(_databaseSettings.TempTableName, transaction);
        }

        /// <summary>
        /// Clears a specified table
        /// </summary>
        /// <param name="tableName">Name of the table to clear</param>
        /// <param name="transaction">Optional SQL transaction</param>
        public void ClearTable(string tableName, SqlTransaction? transaction = null)
        {
            SqlConnection? connection = null;
            bool shouldDisposeConnection = false;

            try
            {
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

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandTimeout = _databaseSettings.CommandTimeout;
                    command.CommandText = $"TRUNCATE TABLE [dbo].[{tableName}]";
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (shouldDisposeConnection && connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the row count from temporary table
        /// </summary>
        /// <returns>Number of rows in temp table</returns>
        public long GetTempTableRowCount()
        {
            return _connectionFactory.GetRowCount(_databaseSettings.TempTableName);
        }

        /// <summary>
        /// Gets the row count from target table
        /// </summary>
        /// <returns>Number of rows in target table</returns>
        public long GetTargetTableRowCount()
        {
            return _connectionFactory.GetRowCount(_databaseSettings.TargetTableName);
        }

        /// <summary>
        /// Gets the row count for a specific filename in the target table
        /// </summary>
        /// <param name="filename">Filename to query</param>
        /// <returns>Number of rows for the specified filename</returns>
        public long GetRowCountByFilename(string filename)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"
                        SELECT COUNT(*) 
                        FROM [dbo].[{_databaseSettings.TargetTableName}] 
                        WHERE [Filename] = @Filename";
                    command.Parameters.AddWithValue("@Filename", filename);

                    var result = command.ExecuteScalar();
                    return Convert.ToInt64(result);
                }
            }
        }

        /// <summary>
        /// Executes the complete transfer workflow with validation and cleanup
        /// </summary>
        /// <param name="filename">Name of the source file</param>
        /// <param name="expectedRowCount">Expected number of rows</param>
        /// <param name="transaction">Optional SQL transaction</param>
        /// <returns>Transfer result</returns>
        public (bool Success, long RowsTransferred, string Message) ExecuteTransferWorkflow(
            string filename,
            long expectedRowCount,
            SqlTransaction? transaction = null)
        {
            try
            {
                // Transfer data with validation
                var result = TransferWithValidation(filename, expectedRowCount, transaction);

                if (!result.Success)
                {
                    return result;
                }

                // Clear temp table after successful transfer
                ClearTempTable(transaction);

                return (true, result.RowsTransferred, "Transfer workflow completed successfully");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Transfer workflow failed: {ex.Message}");
            }
        }
    }
}
