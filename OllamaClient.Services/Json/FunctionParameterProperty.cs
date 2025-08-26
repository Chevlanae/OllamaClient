using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Services.Json
{
    public record struct FunctionParameterProperty
    {
        public string type { get; set; }
        public string description { get; set; }
    }
}
