﻿using OllamaClient.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    public class ModelParameter : IModelParameter, INotifyPropertyChanged
    {
        private ModelParameterKey _Key { get; set; }
        private string _Value { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModelParameterKey[] KeyOptions => Enum.GetValues<ModelParameterKey>();

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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
