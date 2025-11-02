using BulkInApp.Core.Models;

namespace BulkInApp.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for file processing operations
    /// </summary>
    public interface IFileProcessor
    {
        /// <summary>
        /// Processes a single file: Read → Bulk Insert → Transfer → Cleanup
        /// </summary>
        /// <param name="filePath">Full path to the file to process</param>
        /// <returns>Processing result with statistics</returns>
        Task<ProcessingResult> ProcessFileAsync(string filePath);

        /// <summary>
        /// Processes multiple files sequentially
        /// </summary>
        /// <param name="filePaths">List of file paths to process</param>
        /// <returns>Overall processing statistics</returns>
        Task<FileProcessingStats> ProcessFilesAsync(List<string> filePaths);

        /// <summary>
        /// Validates prerequisites before processing (database connection, tables, etc.)
        /// </summary>
        /// <returns>True if all prerequisites are met</returns>
        bool ValidatePrerequisites(out List<string> errorMessages);
    }
}
