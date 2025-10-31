using Microsoft.Data.SqlClient;
using BulkIn.Configuration;

namespace BulkIn.Utilities
{
    /// <summary>
    /// Factory class for creating and managing SQL Server connections
    /// </summary>
    public class SqlConnectionFactory
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of SqlConnectionFactory
        /// </summary>
        /// <param name="databaseSettings">Database configuration settings</param>
        public SqlConnectionFactory(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings ?? throw new ArgumentNullException(nameof(databaseSettings));
            _connectionString = databaseSettings.GetConnectionString();
        }

        /// <summary>
        /// Creates and opens a new SQL connection
        /// </summary>
        /// <returns>An open SqlConnection</returns>
        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Creates and opens a new SQL connection with retry logic
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
        /// <param name="retryDelaySeconds">Delay between retries in seconds (default: 2)</param>
        /// <returns>An open SqlConnection</returns>
        public SqlConnection CreateConnectionWithRetry(int maxRetries = 3, int retryDelaySeconds = 2)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    var connection = new SqlConnection(_connectionString);
                    connection.Open();
                    return connection;
                }
                catch (SqlException ex)
                {
                    lastException = ex;
                    
                    if (attempt >= maxRetries)
                    {
                        throw new InvalidOperationException(
                            $"Failed to connect to SQL Server after {maxRetries} attempts. " +
                            $"Server: {_databaseSettings.ServerName}, Database: {_databaseSettings.DatabaseName}", 
                            ex);
                    }

                    // Wait before retrying (exponential backoff)
                    Thread.Sleep(retryDelaySeconds * 1000 * attempt);
                }
            }

            throw new InvalidOperationException("Connection retry logic failed unexpectedly.", lastException);
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection is successful, false otherwise</returns>
        public bool TestConnection(out string errorMessage)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT 1";
                        command.ExecuteScalar();
                    }
                }
                
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Connection test failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Checks if a table exists in the database
        /// </summary>
        /// <param name="tableName">Name of the table to check</param>
        /// <returns>True if table exists, false otherwise</returns>
        public bool TableExists(string tableName)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT COUNT(*) 
                            FROM INFORMATION_SCHEMA.TABLES 
                            WHERE TABLE_NAME = @TableName";
                        command.Parameters.AddWithValue("@TableName", tableName);
                        
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the row count for a specific table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Number of rows in the table</returns>
        public long GetRowCount(string tableName)
        {
            using (var connection = CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
                    var result = command.ExecuteScalar();
                    return Convert.ToInt64(result);
                }
            }
        }

        /// <summary>
        /// Truncates (clears) a table
        /// </summary>
        /// <param name="tableName">Name of the table to truncate</param>
        public void TruncateTable(string tableName)
        {
            using (var connection = CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"TRUNCATE TABLE [{tableName}]";
                    command.CommandTimeout = _databaseSettings.CommandTimeout;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a SQL command and returns the number of affected rows
        /// </summary>
        /// <param name="sql">SQL command to execute</param>
        /// <param name="parameters">Optional parameters for the command</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            using (var connection = CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandTimeout = _databaseSettings.CommandTimeout;

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Gets the connection string (for SqlBulkCopy usage)
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Ensures the temp table exists with correct structure (drops and recreates if needed)
        /// </summary>
        /// <param name="tempTableName">Name of the temp table</param>
        public void EnsureTempTableExists(string tempTableName)
        {
            using (var connection = CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = _databaseSettings.CommandTimeout;

                    // Drop table if it exists
                    command.CommandText = $@"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{tempTableName}]') AND type in (N'U'))
                        BEGIN
                            DROP TABLE [dbo].[{tempTableName}];
                        END";
                    command.ExecuteNonQuery();

                    // Create fresh temp table with correct structure
                    command.CommandText = $@"
                        CREATE TABLE [dbo].[{tempTableName}] (
                            [ID] INT IDENTITY(1,1) NOT NULL,
                            [Data] NVARCHAR(MAX) NOT NULL,
                            [Filename] NVARCHAR(255) NOT NULL,
                            CONSTRAINT [PK_{tempTableName}] PRIMARY KEY CLUSTERED ([ID] ASC)
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Checks if temp table has the correct structure (Data and Filename columns)
        /// </summary>
        /// <param name="tempTableName">Name of the temp table</param>
        /// <returns>True if structure is correct, false otherwise</returns>
        public bool ValidateTempTableStructure(string tempTableName, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT COLUMN_NAME
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = @TableName AND COLUMN_NAME IN ('Data', 'Filename')
                            ORDER BY COLUMN_NAME";
                        command.Parameters.AddWithValue("@TableName", tempTableName);

                        var columns = new List<string>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader.GetString(0));
                            }
                        }

                        if (!columns.Contains("Data"))
                        {
                            errorMessage = $"Temp table '{tempTableName}' is missing 'Data' column";
                            return false;
                        }

                        if (!columns.Contains("Filename"))
                        {
                            errorMessage = $"Temp table '{tempTableName}' is missing 'Filename' column";
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error validating temp table structure: {ex.Message}";
                return false;
            }
        }
    }
}

