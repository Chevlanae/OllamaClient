using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ToolParametersViewModel
    {
        public class PropertyEntry(string key, JsFunctionParameters.Property value)
        {
            public string Key { get; set; } = key;
            public JsFunctionParameters.Property Value { get; set; } = value;
        }

        private JsFunctionParameters _Parameters { get; set; }

        public string Type
        {
            get => _Parameters.Type.ToString();
        }

        public string[]? Required
        {
            get => _Parameters.Required;
        }

        public ObservableCollection<PropertyEntry> Properties = [];

        public ToolParametersViewModel(JsFunctionParameters parameters)
        {
            _Parameters = parameters;
        }

        public void Refresh()
        {
            Properties.Clear();

            foreach (KeyValuePair<string, JsFunctionParameters.Property> kvp in _Parameters.Properties)
            {
                Properties.Add(new(kvp.Key, kvp.Value));
            }
        }
    }
}
