using OllamaClient.Models;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ToolViewModel
    {
        private Tool _Tool { get; set; }

        public string Name
        {
            get => _Tool.Name;
            set => _Tool.Name = value;
        }

        public string Type
        {
            get => _Tool.Type;
            set => _Tool.Type = value;
        }

        public ToolFunctionParametersViewModel? FunctionParametersViewModel { get; set; }

        public ToolViewModel(Tool tool)
        {
            _Tool = tool;

            if (_Tool.Function.HasValue)
            {
                FunctionParametersViewModel = new(_Tool.Function.Value.parameters);
            }
        }
    }
}
