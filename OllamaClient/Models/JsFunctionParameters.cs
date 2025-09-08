using System.Collections.Generic;
using System.Linq;

namespace OllamaClient.Models
{
    public class JsFunctionParameters
    {
        public class Property
        {
            public enum PropertyType
            {
                Object,
                String,
                Integer,
                Boolean
            }

            public PropertyType Type { get; set; }
            public string Description { get; set; }
            public object Value { get; set; }

            public Property(PropertyType type, string description, object value)
            {
                Type = type;
                Description = description;
                Value = value;
            }

            public Services.Json.FunctionParameterProperty ToJson()
            {
                return new()
                {
                    type = Type.ToString().ToLower(),
                    description = Description
                };
            }
        }

        public ParameterType Type { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public string[]? Required { get; set; }

        public JsFunctionParameters()
        {
            Type = ParameterType.Object;
            Properties = new Dictionary<string, Property>();
        }

        public Services.Json.FunctionParameters ToJson()
        {
            return new()
            {
                type = Type.ToString().ToLower(),
                properties = Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToJson()),
                required = Required
            };
        }
    }
}
