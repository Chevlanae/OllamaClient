using Microsoft.Extensions.Logging;
using OllamaClient.Json;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public class ConversationSidebarViewModel
    {
        public ConversationCollection ConversationCollection { get; set; }
        public ObservableCollection<ConversationViewModel> ConversationViewModelCollection { get; set; } = [];
        public List<string> AvailableModels { get; set; } = [];
        public DateTime? LastUpdated { get; set; }

        public ConversationSidebarViewModel()
        {
            ConversationCollection = (ConversationCollection)App.GetRequiredService<IConversationCollection>();
        }
    }
}
