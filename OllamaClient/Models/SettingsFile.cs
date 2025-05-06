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

        public SettingsFile(string filePath)
        {
            Uri fileUri = new(filePath);

            if (!File.Exists(fileUri.LocalPath))
            {
                SocketAddress = Environment.GetEnvironmentVariable("OLLAMA_HOST") ?? "localhost:11434";
                UseHttps = false;
                RequestTimeout = Timeout.InfiniteTimeSpan;
                SubjectGenerationOptions = new()
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
                EnableModelParametersForChat = false;
                _SettingsFile = new(new(filePath));
                Save();
            }
            else
            {
                _SettingsFile = new(fileUri);
                if (_SettingsFile.Get() is SettingsFile file)
                {
                    SocketAddress = file.SocketAddress;
                    UseHttps = file.UseHttps;
                    RequestTimeout = file.RequestTimeout;
                    SubjectGenerationOptions = file.SubjectGenerationOptions;
                    EnableModelParametersForChat = file.EnableModelParametersForChat;
                }
                else throw new FileNotFoundException($"Settings file at '{fileUri.LocalPath}' was not found or was invalid.");
            }
        }

        public void Save()
        {
            _SettingsFile.Set(this);
        }
    }
}
