using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
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

        public string? Name
        {
            get => _Tool.Function?.Name;
        }

        public string? Description
        {
            get => _Tool.Function?.Description;
        }

        public ToolParametersViewModel? Parameters { get; set; }

        public ToolViewModel(Tool tool)
        {
            _Tool = tool;
            
            if(_Tool.Function is not null)
            {
                Parameters = new(_Tool.Function.Parameters);
            }
        }
    }
}
