using OllamaClient.Models;
using System;
using System.IO;

namespace OllamaClient.Services
{
    internal static class Settings
    {
        private static SettingsFile _SettingsFile { get; } = new(Paths.State);

        public static string SocketAddress
        {
            get => _SettingsFile.SocketAddress;
            set
            {
                _SettingsFile.SocketAddress = value;
                _SettingsFile.Save();
            }
        }
        public static bool UseHttps
        {
            get => _SettingsFile.UseHttps;
            set
            {
                _SettingsFile.UseHttps = value;
                _SettingsFile.Save();
            }
        }
        public static TimeSpan RequestTimeout
        {
            get => _SettingsFile.RequestTimeout;
            set
            {
                _SettingsFile.RequestTimeout = value;
                _SettingsFile.Save();
            }
        }
        public static CompletionRequest SubjectGenerationOptions
        {
            get => _SettingsFile.SubjectGenerationOptions;
            set
            {
                _SettingsFile.SubjectGenerationOptions = value;
                _SettingsFile.Save();
            }
        }
        public static bool EnableModelParametersForChat
        {
            get => _SettingsFile.EnableModelParametersForChat;
            set
            {
                _SettingsFile.EnableModelParametersForChat = value;
                _SettingsFile.Save();
            }
        }
    }
}
