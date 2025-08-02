using System.Runtime.Serialization;

namespace OllamaClient.Models.Json
{
    [DataContract]
    public record struct CompletionRequest
    {
        [DataMember]
        public string model { get; set; }
        [DataMember]
        public string prompt { get; set; }
        [DataMember]
        public bool? stream { get; set; }
        [DataMember]
        public string? system { get; set; }
        [DataMember]
        public string? template { get; set; }
        [DataMember]
        public ModelParameters? options { get; set; }
    }
}
