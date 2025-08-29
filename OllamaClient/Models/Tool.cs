namespace OllamaClient.Models
{
    public class Tool
    {
        public enum ToolType
        {
            Function
        }

        public ToolType Type { get; set; }
        public Function Function { get; set; }

        public Tool(string name, string description)
        {
            Type = ToolType.Function;
            Function = new(name, description);
        }

        public Services.Json.Tool ToJson()
        {
            return new()
            {
                type = Type.ToString().ToLower(),
                function = Function.ToJson()
            };
        }
    }
}
