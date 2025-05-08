using OllamaClient.Models;
using System;
using System.Linq;

namespace OllamaClient.ViewModels
{
    internal class SettingsSidebar
    {
        public string[] OptionsArray = typeof(Services.Settings).GetProperties().Select(i => i.Name).ToArray();

        public static string SocketAddress => Services.Settings.SocketAddress;
        public static bool UseHttps => Services.Settings.UseHttps;
        public static TimeSpan RequestTimeout => Services.Settings.RequestTimeout;
        public static CompletionRequest SubjectGenerationOptions => Services.Settings.SubjectGenerationOptions;
        public static bool EnableModelParametersForChat => Services.Settings.EnableModelParametersForChat;
    }
}
