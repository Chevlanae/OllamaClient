using OllamaClient.Json;
using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.DataContracts
{
    [DataContract]
    public class ChatMessageContract
    {
        [DataMember]
        public Role Role { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public DateTime? Timestamp { get; set; }
    }
}
