namespace OllamaClient.Models
{
    public class Function
    {
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
        public string Description { get; set; }
        public Parameters Parameters { get; set; }

        public Function(string name, string description, ParameterType? parameterType = null)
        {
            Name = name;
            Description = description;
            Parameters = new Parameters(parameterType ?? ParameterType.Object);
        }

        public Services.Json.Function ToJson()
        {
            return new()
            {
                name = Name,
                description = Description,
                parameters = Parameters.ToJson()
            };
        }
    }
}
