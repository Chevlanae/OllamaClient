using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public partial class ToolViewModel
    {
        public class ParametersViewModel
        {
            public class PropertyEntry(string key, Parameters.Property value)
            {
                public string Key { get; set; } = key;
                public Parameters.Property Value { get; set; } = value;
            }

            private Parameters _Parameters { get; set; }

            public string Type
            {
                get => _Parameters.Type.ToString();
            }

            public string[]? Required
            {
                get => _Parameters.Required;
            }

            public ObservableCollection<PropertyEntry> Properties = [];

            public ParametersViewModel(Parameters parameters)
            {
                _Parameters = parameters;
            }

            public void Refresh()
            {
                Properties.Clear();

                foreach(KeyValuePair<string, Parameters.Property> kvp in _Parameters.Properties)
                {
                    Properties.Add(new(kvp.Key, kvp.Value));
                }
            }
        }
    }
}
