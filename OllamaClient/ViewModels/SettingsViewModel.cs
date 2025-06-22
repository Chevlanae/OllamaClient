using OllamaClient.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        public string SocketAddress
        {
            get => Services.Settings.SocketAddress;
            set
            {
                Services.Settings.SocketAddress = value;
                OnPropertyChanged();
            }
        }
        public bool UseHttps
        {
            get => Services.Settings.UseHttps;
            set
            {
                Services.Settings.UseHttps = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan RequestTimeout
        {
            get => Services.Settings.RequestTimeout;
            set
            {
                Services.Settings.RequestTimeout = value;
                OnPropertyChanged();
            }
        }
        public CompletionRequest SubjectGenerationOptions
        {
            get => Services.Settings.SubjectGenerationOptions;
            set
            {
                Services.Settings.SubjectGenerationOptions = value;
                OnPropertyChanged();
            }
        }
        public bool EnableModelParametersForChat
        {
            get => Services.Settings.EnableModelParametersForChat;
            set
            {
                Services.Settings.EnableModelParametersForChat = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }
    }
}
