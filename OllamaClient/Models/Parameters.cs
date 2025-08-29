using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OllamaClient.Models.Function;

namespace OllamaClient.Models
{
    public class Parameters
    {
        public class Property
        {
            public PropertyType Type { get; set; }
            public string Description { get; set; }
            public Property(PropertyType type, string description)
            {
                Type = type;
                Description = description;
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

        public Parameters(ParameterType type)
        {
            Type = type;
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
