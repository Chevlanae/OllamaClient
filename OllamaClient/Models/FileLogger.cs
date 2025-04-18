using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace OllamaClient.Models
{
    internal class FileLogger : ILogger
    {
        private readonly ILogger Logger;

        private readonly string FilePath;
        private readonly object FileLock = new();

        public FileLogger(string filePath, ILogger logger)
        {
            FilePath = filePath;
            Logger = logger;
        }

        private void Log(LogLevel logLevel, string message, Exception? exception = null)
        {
            Log(logLevel, new EventId(), message, exception, (s, e) =>
            {
                if (e != null)
                {
                    return $"{s} - Exception: {e.Message}\n{e.Source}\n{e.StackTrace}";
                }
                else return s;
            });
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logger.Log(logLevel, eventId, state, exception, formatter);

            string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}]: {formatter(state, exception)}\n";

            lock (FileLock)
            {
                File.AppendAllText(FilePath, logEntry);
            }
        }

        public void LogInformation(string message)
        {
            Log(LogLevel.Information,  message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            Log(LogLevel.Error, message, exception);
        }

        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void LogCritical(string message, Exception? exception = null)
        {
            Log(LogLevel.Critical, message, exception);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return Logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel) => Logger.IsEnabled(logLevel);
    }
}
