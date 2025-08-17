using OllamaClient.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    public class ModelParameterViewModel(ModelParameterKey key, string value) : IModelParameter, INotifyPropertyChanged
    {
        private ModelParameterKey _Key { get; set; } = key;
        private string _Value { get; set; } = value;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));

        public ModelParameterKey Key
        {
            get => _Key;
            set
            {
                _Key = value;
                OnPropertyChanged();
            }
        }
        public string Value
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged();
            }
        }
    }
}
