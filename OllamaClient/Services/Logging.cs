using Microsoft.Extensions.Logging;
using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace OllamaClient.Services
{
    public enum LogProvider
    {
        Console,
        File,
        Debug
    }

    internal static class Logging
    {
        private static Dictionary<string, ILogger> Loggers { get; } = [];
        private static FileLoggerProvider Factory { get; } = new(Paths.Logs);

        private static ILogger GetLogger(string category)
        {
            if (Loggers.ContainsKey(category))
            {
                return Loggers[category];
            }
            else
            {
                ILogger logger = Factory.CreateLogger(category);
                Loggers.Add(category, logger);
                return logger;
            }
        }

        public static void Log(string message, LogLevel level, Exception? exception = null, [CallerFilePath] string? callerPath = null)
        {
            // Ensure category is not null or empty
            string resolvedCategory = string.IsNullOrEmpty(callerPath) ? "Default" : Path.GetFileNameWithoutExtension(callerPath);

            ILogger logger = GetLogger(resolvedCategory);

            switch (level)
            {
                case LogLevel.Information:
                    logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                    {
                        logger.LogError(exception, message);
                    }
                    else
                    {
                        logger.LogError(message);
                    }
                    break;
                case LogLevel.Critical:
                    if (exception != null)
                    {
                        logger.LogCritical(exception, message);
                    }
                    else
                    {
                        logger.LogCritical(message);
                    }
                    break;
            }
        }
    }
}
