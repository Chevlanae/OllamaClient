using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace OllamaClient.Models
{
    public class FileLoggerProvider(string dirPath, ILoggerFactory? factory = null) : ILoggerProvider
    {
        private readonly Uri DirectoryUri = new(dirPath);
        private readonly ILoggerFactory Factory = factory ?? LoggerFactory.Create(b => b.AddConsole());

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(Path.Combine(DirectoryUri.LocalPath, $"{categoryName}.log"), Factory.CreateLogger(categoryName));
        }
        public void Dispose()
        {
            Factory.Dispose();
        }
    }
}
