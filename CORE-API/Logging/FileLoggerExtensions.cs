using Microsoft.Extensions.Logging;

namespace CORE_API.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string filePath)
        {
            builder.AddProvider(new FileLoggerProvider(filePath));
            return builder;
        }
    }
}
