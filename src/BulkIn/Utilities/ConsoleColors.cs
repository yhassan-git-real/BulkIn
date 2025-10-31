namespace BulkIn.Utilities
{
    /// <summary>
    /// Console color utility for modern, colorful output
    /// </summary>
    public static class ConsoleColors
    {
        // ANSI Color Codes
        public const string Reset = "\x1b[0m";
        
        // Text Colors
        public const string Cyan = "\x1b[36m";
        public const string BrightCyan = "\x1b[96m";
        public const string Green = "\x1b[32m";
        public const string BrightGreen = "\x1b[92m";
        public const string Yellow = "\x1b[33m";
        public const string BrightYellow = "\x1b[93m";
        public const string Red = "\x1b[31m";
        public const string BrightRed = "\x1b[91m";
        public const string Blue = "\x1b[34m";
        public const string BrightBlue = "\x1b[94m";
        public const string Magenta = "\x1b[35m";
        public const string BrightMagenta = "\x1b[95m";
        public const string White = "\x1b[37m";
        public const string BrightWhite = "\x1b[97m";
        public const string Gray = "\x1b[90m";
        
        // Text Styles
        public const string Bold = "\x1b[1m";
        public const string Dim = "\x1b[2m";
        public const string Underline = "\x1b[4m";
        
        /// <summary>
        /// Formats text with color
        /// </summary>
        public static string Color(string text, string color)
        {
            return $"{color}{text}{Reset}";
        }
        
        /// <summary>
        /// Formats text with bold style
        /// </summary>
        public static string BoldText(string text)
        {
            return $"{Bold}{text}{Reset}";
        }
        
        /// <summary>
        /// Formats text with color and bold
        /// </summary>
        public static string ColorBold(string text, string color)
        {
            return $"{Bold}{color}{text}{Reset}";
        }
        
        /// <summary>
        /// Success message (green)
        /// </summary>
        public static string Success(string text)
        {
            return $"{BrightGreen}{text}{Reset}";
        }
        
        /// <summary>
        /// Error message (red)
        /// </summary>
        public static string Error(string text)
        {
            return $"{BrightRed}{text}{Reset}";
        }
        
        /// <summary>
        /// Info message (cyan)
        /// </summary>
        public static string Info(string text)
        {
            return $"{BrightCyan}{text}{Reset}";
        }
        
        /// <summary>
        /// Warning message (yellow)
        /// </summary>
        public static string Warning(string text)
        {
            return $"{BrightYellow}{text}{Reset}";
        }
        
        /// <summary>
        /// Highlight message (magenta)
        /// </summary>
        public static string Highlight(string text)
        {
            return $"{BrightMagenta}{text}{Reset}";
        }
        
        /// <summary>
        /// Secondary/dim text (gray)
        /// </summary>
        public static string Secondary(string text)
        {
            return $"{Gray}{text}{Reset}";
        }
    }
}
