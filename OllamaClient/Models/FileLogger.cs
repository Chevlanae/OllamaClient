using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace OllamaClient.Models
{
    internal class FileLogger : ILogger
    {
        private readonly ILogger Logger;
        private readonly string FilePath;
        private readonly string AllLogsFilePath;
        private readonly string CategoryName;
        private readonly object FileLock = new();
        private readonly object AllLogsFileLock = new();

        public FileLogger(string dirPath, string categoryName, object allLogsLock, ILogger logger)
        {
            Logger = logger;
            FilePath = Path.Combine(dirPath, $"{categoryName}.log");
            AllLogsFilePath = Path.Combine(dirPath, "All.log");
            CategoryName = categoryName;
            AllLogsFileLock = allLogsLock;
        }

        private string FormatMessage(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                return $"{message} - Exception: {exception.Message}\n{exception.Source}\n{exception.StackTrace}";
            }
            else return message;
        }

        private void LogLazy(LogLevel logLevel, string message, Exception? exception = null)
        {
            Log(logLevel, new EventId(), message, exception, (s, e) => s);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logger.Log(logLevel, eventId, state, exception, formatter);

            string message = FormatMessage(state?.ToString() ?? "", exception);

            string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{CategoryName}] [{logLevel}]: {message}\n";

            lock (FileLock)
            {
                File.AppendAllText(FilePath, logEntry);
            }

            lock (AllLogsFileLock)
            {
                File.AppendAllText(AllLogsFilePath, logEntry);
            }
        }

        public void LogInformation(string message)
        {
            LogLazy(LogLevel.Information,  message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            LogLazy(LogLevel.Error, message, exception);
        }

        public void LogWarning(string message)
        {
            LogLazy(LogLevel.Warning, message);
        }

        public void LogCritical(string message, Exception? exception = null)
        {
            LogLazy(LogLevel.Critical, message, exception);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return Logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel) => Logger.IsEnabled(logLevel);
    }
}
