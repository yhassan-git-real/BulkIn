using System.Data;

namespace BulkInApp.Core.Services
{
    /// <summary>
    /// Custom IDataReader implementation for streaming text file lines to SqlBulkCopy
    /// This avoids materializing all data in memory as a DataTable
    /// </summary>
    public class TextFileDataReader : IDataReader
    {
        private readonly IEnumerator<string> _lineEnumerator;
        private readonly string _filename;
        private bool _disposed = false;
        private bool _isClosed = false;

        /// <summary>
        /// Initializes a new instance of TextFileDataReader
        /// </summary>
        /// <param name="lines">Enumerable of file lines</param>
        /// <param name="filename">Source filename to include in each row</param>
        public TextFileDataReader(IEnumerable<string> lines, string filename)
        {
            _lineEnumerator = lines.GetEnumerator();
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        /// <summary>
        /// Current line data
        /// </summary>
        private string? _currentLine;

        /// <summary>
        /// Number of fields (columns) - 2 for our use case (Data and Filename columns)
        /// </summary>
        public int FieldCount => 2;

        /// <summary>
        /// Depth of the data reader (always 0)
        /// </summary>
        public int Depth => 0;

        /// <summary>
        /// Whether the data reader is closed
        /// </summary>
        public bool IsClosed => _isClosed;

        /// <summary>
        /// Records affected (not applicable for reading)
        /// </summary>
        public int RecordsAffected => -1;

        /// <summary>
        /// Indexer by ordinal
        /// </summary>
        public object this[int i] => GetValue(i);

        /// <summary>
        /// Indexer by name
        /// </summary>
        public object this[string name] => GetValue(GetOrdinal(name));

        /// <summary>
        /// Advances to the next record
        /// </summary>
        public bool Read()
        {
            if (_isClosed)
                return false;

            bool hasNext = _lineEnumerator.MoveNext();
            if (hasNext)
            {
                _currentLine = _lineEnumerator.Current;
            }
            return hasNext;
        }

        /// <summary>
        /// Gets the value at the specified ordinal
        /// </summary>
        public object GetValue(int i)
        {
            return i switch
            {
                0 => _currentLine ?? string.Empty,  // Data column - CRITICAL: No trimming
                1 => _filename,                      // Filename column
                _ => throw new IndexOutOfRangeException($"Invalid column index: {i}")
            };
        }

        /// <summary>
        /// Gets the name of the column
        /// </summary>
        public string GetName(int i)
        {
            return i switch
            {
                0 => "Data",
                1 => "Filename",
                _ => throw new IndexOutOfRangeException($"Invalid column index: {i}")
            };
        }

        /// <summary>
        /// Gets the ordinal of a column by name
        /// </summary>
        public int GetOrdinal(string name)
        {
            if (name.Equals("Data", StringComparison.OrdinalIgnoreCase))
                return 0;
            if (name.Equals("Filename", StringComparison.OrdinalIgnoreCase))
                return 1;
            
            throw new IndexOutOfRangeException($"Column not found: {name}");
        }

        /// <summary>
        /// Gets the data type of the column
        /// </summary>
        public Type GetFieldType(int i)
        {
            return i switch
            {
                0 => typeof(string),  // Data
                1 => typeof(string),  // Filename
                _ => throw new IndexOutOfRangeException($"Invalid column index: {i}")
            };
        }

        /// <summary>
        /// Gets the string value at the specified ordinal
        /// </summary>
        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        /// <summary>
        /// Checks if the value at the specified ordinal is null
        /// </summary>
        public bool IsDBNull(int i)
        {
            return _currentLine == null;
        }

        /// <summary>
        /// Closes the data reader
        /// </summary>
        public void Close()
        {
            _isClosed = true;
        }

        /// <summary>
        /// Disposes the data reader
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _lineEnumerator?.Dispose();
                _disposed = true;
                _isClosed = true;
            }
        }

        // Remaining IDataReader members (not used but required by interface)
        public bool NextResult() => false;
        public DataTable GetSchemaTable() => throw new NotImplementedException();
        public bool GetBoolean(int i) => throw new NotImplementedException();
        public byte GetByte(int i) => throw new NotImplementedException();
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public char GetChar(int i) => throw new NotImplementedException();
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public IDataReader GetData(int i) => throw new NotImplementedException();
        public string GetDataTypeName(int i) => throw new NotImplementedException();
        public DateTime GetDateTime(int i) => throw new NotImplementedException();
        public decimal GetDecimal(int i) => throw new NotImplementedException();
        public double GetDouble(int i) => throw new NotImplementedException();
        public float GetFloat(int i) => throw new NotImplementedException();
        public Guid GetGuid(int i) => throw new NotImplementedException();
        public short GetInt16(int i) => throw new NotImplementedException();
        public int GetInt32(int i) => throw new NotImplementedException();
        public long GetInt64(int i) => throw new NotImplementedException();
        public int GetValues(object[] values) => throw new NotImplementedException();
    }
}
