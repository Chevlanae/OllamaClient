using OllamaClient.Services.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    public class ToolFunctionParametersViewModel
    {
        public class ParameterProperty
        {
            public string Type { get; set; }
            public string Description { get; set; }

            public ParameterProperty() { }

            public ParameterProperty(FunctionParameterProperty property)
            {
                Type = property.type;
                Description = property.description;
            }
        }

        public ObservableCollection<KeyValuePair<string, ParameterProperty>> Properties { get; set; } = new();

        public string? Type
        {
            get => _Parameters.type;
        }

        public string[]? Required
        {
            get => _Parameters.required;
        }

        private FunctionParameters _Parameters { get; set; }

        public ToolFunctionParametersViewModel(FunctionParameters? parameters = null)
        {
            _Parameters = parameters ?? new FunctionParameters();

            Refresh();
        }

        public void Refresh()
        {
            Properties.Clear();
            foreach (var property in _Parameters.properties)
            {
                KeyValuePair<string, ParameterProperty> pair = new(property.Key, new(property.Value));

                Properties.Add(pair);
            }
        }

        public void AddProperty(string key, ParameterProperty property)
        {
            Properties.Add(new(key, property));
        }
    }
}
