using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Desktop_Creatures.Utilities
{
    /// <summary>
    /// Provides static methods for logging errors and debugging information to files within the application's current directory.
    /// </summary>
    /// <remarks>
    /// Logs are written to the 'Logs' directory in the application's current directory. Error logs are stored in 'errors.log',
    /// and debug logs in 'debug.log'. Each log entry is timestamped. Ensure thread safety if used in a multi-threaded context.
    /// </remarks>
    public static class Logger
    {
        // Fields for log file paths
        private static readonly string _logDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
        private static readonly string _errorLogFile = Path.Combine(_logDirectory, "errors.log");
        private static readonly string _debugLogFile = Path.Combine(_logDirectory, "debug.log");
        private static readonly string _warningLogFile = Path.Combine(_logDirectory, "warning.log");
        // Lock for thread safety
        private static readonly object _logLock = new();
        // Field to set max Log File size
        private static readonly long _maxLogFileSize = 10 * 1024 * 1024; // 10 MB, adjust as needed

        // Default to Error if not specified
        public static LogLevel CurrentLogLevel { get; private set; } = LogLevel.Debug;

        public enum LogLevel
        {
            None = 0,
            Error = 1,
            Debug = 2,
            //future flags
            Info = 3,
            Warning = 4
        }

        // Static constructor
        static Logger()
        {
            EnsureLogDirectoryExists();
            InitializeDebugLog();
        }

        /// <summary>
        /// Logs an exception as an error.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="customMessage">An optional custom message to include with the error log.</param>
        public static void LogError(Exception ex, string customMessage = "")
        {
            // Check logging level requirement
            if (CurrentLogLevel < LogLevel.Error) return;

            try
            {
                lock (_logLock)
                {
                    RotateLogFileIfNeeded(_errorLogFile);
                    string errorMessage = $"[{DateTime.Now}] Error: {(string.IsNullOrEmpty(customMessage) ? "" : customMessage + ": ")}{ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}{Environment.NewLine}";
                    File.AppendAllText(_errorLogFile, errorMessage + Environment.NewLine);
                }
            }
            catch (Exception logEx)
            {
                // If logging fails, consider writing to the console or another fall-back mechanism
                Console.WriteLine($"Failed to log error: {logEx.Message}");
                //Debug.WriteLine($"Failed to log error: {logEx.Message}");
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogDebug(string message)
        {
            // Check logging level requirement
            //if (CurrentLogLevel < LogLevel.Debug) return;

            try
            {
                lock (_logLock)
                {
                    RotateLogFileIfNeeded(_debugLogFile);
                    File.AppendAllText(_debugLogFile, $"{DateTime.Now}: {message}{Environment.NewLine}");
                }
            }
            catch (Exception logEx)
            {
                // If logging fails, consider writing to the console or another fall-back mechanism
                Console.WriteLine($"Failed to log debug message: {logEx.Message}");
                Debug.WriteLine($"Failed to log debug message: {logEx.Message}");
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogWarning(string message)
        {
            // Check logging level requirement
            //if (CurrentLogLevel < LogLevel.Warning) return;

            try
            {
                lock (_logLock)
                {
                    RotateLogFileIfNeeded(_warningLogFile);
                    File.AppendAllText(_warningLogFile, $"{DateTime.Now}: {message}{Environment.NewLine}");
                }
            }
            catch (Exception logEx)
            {
                // If logging fails, consider writing to the console or another fall-back mechanism
                Console.WriteLine($"Failed to log debug message: {logEx.Message}");
                //Debug.WriteLine($"Failed to log debug message: {logEx.Message}");
            }
        }

        /// <summary>
        /// Checks and performs log file rotation if the current log file size exceeds the predefined maximum size.
        /// When rotation is necessary, the current log file is renamed (archived) with a timestamp, and subsequent log entries
        /// will continue in a new file.
        /// </summary>
        /// <param name="logFilePath">The path to the current log file to check for rotation.</param>
        private static void RotateLogFileIfNeeded(string logFilePath)
        {
            FileInfo logFileInfo = new(logFilePath);
            if (logFileInfo.Exists && logFileInfo.Length > _maxLogFileSize)
            {
                string archiveFilePath = $"{logFilePath}_{DateTime.Now:yyyyMMddHHmmss}.archive";
                File.Move(logFilePath, archiveFilePath);
            }
        }

        /// <summary>
        /// Ensures the existence of the directory where log files will be stored.
        /// This method is called at the initialization phase of the application to prepare the logging infrastructure.
        /// </summary>
        private static void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Initializes the debug log file with a starting message. This method is part of the logger's initialization process,
        /// marking the beginning of a new logging session.
        /// </summary>
        private static void InitializeDebugLog()
        {
            string startMessage = $"{Environment.NewLine}Debug Log started on {DateTime.Now}{Environment.NewLine}";
            File.AppendAllText(_debugLogFile, startMessage);
        }

        public static void LoadLoggingLevelFromConfig(string configPath)
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);
                var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                if (config != null && config.TryGetValue("LoggingLevel", out string logLevelStr))
                {
                    CurrentLogLevel = Enum.TryParse(logLevelStr, out LogLevel logLevel) ? logLevel : LogLevel.Error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load logging configuration: {ex.Message}");
                // Default to LogLevel.Error if loading fails
                CurrentLogLevel = LogLevel.Error;
            }
        }

    }
}
