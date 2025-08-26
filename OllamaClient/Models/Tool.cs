using OllamaClient.Services.Json;

namespace OllamaClient.Models
{
    public class Tool
    {
        public enum ToolType
        {
            Function
        }

        public enum ParameterType
        {
            Object
        }

        public enum PropertyType
        {
            Object,
            String,
            Integer,
            Boolean
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public Function? Function { get; set; }

        public Tool(string name, string type)
        {
            Name = name;
            Type = type;

            if(Type == "Function")
            {
                Function = new();
            }
        }
    }
}
