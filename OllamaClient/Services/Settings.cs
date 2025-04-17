using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    [KnownType(typeof(CompletionRequest))]
    [DataContract]
    internal static class Settings
    {
        [DataMember]
        public static Uri ConnectionUri { get; set; }
        [DataMember]
        public static bool UseHttps { get; set; }
        [DataMember]
        public static TimeSpan RequestTimeout { get; set; }
        [DataMember]
        public static CompletionRequest SubjectGenerationOptions { get; set; }
        [DataMember]
        public static bool EnableModelParametersForChat { get; set; }

    }
}
