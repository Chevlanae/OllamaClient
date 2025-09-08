using Microsoft.JavaScript.NodeApi;
using OllamaClient.JsEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class Tool
    {
        public enum ToolType
        {
            Function
        }

        public ToolType Type { get; set; }
        public JsFunction Function { get; set; }
        
        public Tool(string name, string description, INodeJs node, JSReference reference, string filename)
        {
            Type = ToolType.Function;

            Function = new(name, description, filename, reference, node);
        }

        public Services.Json.Tool ToJson()
        {
            return new()
            {
                type = Type.ToString().ToLower(),
                function = Function?.ToJson()
            };
        }
    }
}
