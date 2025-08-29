using OllamaClient.Models;
using System;

namespace OllamaClient.ViewModels
{
    public partial class ToolViewModel
    {

        private Tool _Tool { get; set; }

        public string? Type
        {
            get => Enum.GetName(_Tool.Type);
        }

        public string Name
        {
            get => _Tool.Function.Name;
        }

        public string Description
        {
            get => _Tool.Function.Description;
        }

        public ParametersViewModel Parameters { get; set; }

        public ToolViewModel(Tool tool)
        {
            _Tool = tool;
            Parameters = new(_Tool.Function.Parameters);
        }

        public ToolViewModel(string name, string description)
        {
            _Tool = new(name, description);
            Parameters = new(_Tool.Function.Parameters);
        }
    }
}
