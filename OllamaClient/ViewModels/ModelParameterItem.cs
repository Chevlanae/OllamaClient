using OllamaClient.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    public class ModelParameterItem : IModelParameter, INotifyPropertyChanged
    {
        private ModelParameterKey K { get; set; }
        private string V { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModelParameterKey[] KeyOptions => Enum.GetValues<ModelParameterKey>();

        public ModelParameterKey Key
        {
            get => K;
            set
            {
                K = value;
                OnPropertyChanged();
            }
        }
        public string Value
        {
            get => V;
            set
            {
                V = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
