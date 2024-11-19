using Microsoft.Extensions.Logging;

namespace CORE_API.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logFilePath;

        public FileLoggerProvider(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_logFilePath);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
