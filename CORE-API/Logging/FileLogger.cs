using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace CORE_API.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private static readonly object _lock = new object();

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            // Customize log levels if needed
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string logMessage = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}";

            if (exception != null)
            {
                logMessage += $" | Exception: {exception.Message} | StackTrace: {exception.StackTrace}";
            }

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Optional: Handle logging failure
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
    }
}
