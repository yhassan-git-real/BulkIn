namespace BulkInApp.Core.Configuration
{
    /// <summary>
    /// Database connection and configuration settings
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// SQL Server name (e.g., localhost\SQLEXPRESS)
        /// </summary>
        public string ServerName { get; set; } = string.Empty;

        /// <summary>
        /// Target database name
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Temporary staging table name
        /// </summary>
        public string TempTableName { get; set; } = "TempTextFileData";

        /// <summary>
        /// Final target table name
        /// </summary>
        public string TargetTableName { get; set; } = "TextFileData";

        /// <summary>
        /// Connection timeout in seconds (default: 30)
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// Command execution timeout in seconds (default: 600 - 10 minutes)
        /// </summary>
        public int CommandTimeout { get; set; } = 600;

        /// <summary>
        /// Use Windows Authentication (true) or SQL Authentication (false)
        /// </summary>
        public bool UseTrustedConnection { get; set; } = true;

        /// <summary>
        /// SQL Authentication username (if UseTrustedConnection = false)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// SQL Authentication password (if UseTrustedConnection = false)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Builds and returns the SQL Server connection string
        /// </summary>
        public string GetConnectionString()
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = ServerName,
                InitialCatalog = DatabaseName,
                ConnectTimeout = ConnectionTimeout,
                TrustServerCertificate = true // Required for newer SQL Server versions
            };

            if (UseTrustedConnection)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = Username;
                builder.Password = Password;
                builder.IntegratedSecurity = false;
            }

            return builder.ConnectionString;
        }

        /// <summary>
        /// Validates that all required settings are provided
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                errorMessage = "ServerName is required in DatabaseSettings.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                errorMessage = "DatabaseName is required in DatabaseSettings.";
                return false;
            }

            if (!UseTrustedConnection)
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    errorMessage = "Username is required when UseTrustedConnection is false.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    errorMessage = "Password is required when UseTrustedConnection is false.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
