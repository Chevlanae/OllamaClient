using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.IO;

namespace OllamaClient.Models
{
    public class FileLoggerProvider(string dirPath) : ILoggerProvider
    {
        private readonly Uri DirectoryUri = new(dirPath);
        private readonly DebugLoggerProvider Factory = new();
        private readonly object AllLogsFileLock = new();

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(DirectoryUri.LocalPath, categoryName, AllLogsFileLock, Factory.CreateLogger(categoryName));
        }
        public void Dispose()
        {
            Factory.Dispose();
        }
    }
}
