using Microsoft.Extensions.Logging;
using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace OllamaClient.Services
{
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

        public static void Log(string message, LogLevel level, Exception? exception = null, [CallerMemberName] string? callerName = null)
        {

            ILogger logger = GetLogger(callerName ?? "Default");

            switch (level)
            {
                case LogLevel.Information:
                    logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    logger.LogError(exception, message);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(exception, message);
                    break;
            }
        }
    }
}
