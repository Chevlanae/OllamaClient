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

        private INodeJs _NodeJS { get; set; }

        public ToolType Type { get; set; }
        public JsFunction? Function { get; set; }
        
        public Tool(string name, string description, INodeJs node, JSReference? reference)
        {
            _NodeJS = node;

            Type = ToolType.Function;
            
            if(reference is not null)
            {
                Function = new(name, description, reference, node);
            }
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
