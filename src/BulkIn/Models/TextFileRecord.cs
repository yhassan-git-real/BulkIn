namespace BulkIn.Models
{
    /// <summary>
    /// Represents a single text file data record
    /// </summary>
    public class TextFileRecord
    {
        /// <summary>
        /// Unique identifier (auto-generated in database)
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The actual data content from the text file (preserves all whitespace)
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Source filename
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the record was inserted
        /// </summary>
        public DateTime Date { get; set; }
    }
}
