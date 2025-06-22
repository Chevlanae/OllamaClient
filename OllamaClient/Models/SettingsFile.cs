using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;

namespace OllamaClient.Models
{
    [KnownType(typeof(CompletionRequest))]
    [DataContract]
    internal class SettingsFile
    {
        private static class Defaults
        {
            public static string SocketAddress => Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "localhost:11434";
            public static bool UseHttps => false;
            public static TimeSpan RequestTimeout => Timeout.InfiniteTimeSpan;
            public static CompletionRequest SubjectGenerationOptions => new()
            {
                model = "llama3:8b",
                prompt = "Summarize this string, in 4 words or less: $Prompt$. This string is the opening message in a conversation. Do not include quotation marks.",
                stream = true,
                options = new()
                {
                    num_predict = 10,
                    top_k = 10,
                    top_p = (float?)0.5
                }
            };
            public static bool EnableModelParametersForChat => false;
        }

        private DataFile<SettingsFile> _SettingsFile { get; }

        [DataMember]
        public string SocketAddress { get; set; }
        [DataMember]
        public bool UseHttps { get; set; }
        [DataMember]
        public TimeSpan RequestTimeout { get; set; }
        [DataMember]
        public CompletionRequest SubjectGenerationOptions { get; set; }
        [DataMember]
        public bool EnableModelParametersForChat { get; set; }

        public SettingsFile(Uri fileUri)
        {
            _SettingsFile = new(fileUri);
            if (_SettingsFile.Exists() && _SettingsFile.Get() is SettingsFile settings)
            {
                SocketAddress = settings.SocketAddress;
                UseHttps = settings.UseHttps;
                RequestTimeout = settings.RequestTimeout;
                SubjectGenerationOptions = settings.SubjectGenerationOptions;
                EnableModelParametersForChat = settings.EnableModelParametersForChat;
            }
            else
            {
                SocketAddress = Defaults.SocketAddress;
                UseHttps = Defaults.UseHttps;
                RequestTimeout = Defaults.RequestTimeout;
                SubjectGenerationOptions = Defaults.SubjectGenerationOptions;
                EnableModelParametersForChat = Defaults.EnableModelParametersForChat;
                Save();
            }
        }

        public void Save()
        {
            _SettingsFile.Set(this);
        }
    }
}
