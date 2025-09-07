using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Services.Json;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace OllamaClient.ViewModels
{
    public partial class ConversationViewModel : INotifyPropertyChanged
    {
        public class Settings
        {
            public CompletionRequest SubjectRequest { get; set; }
        }

        private ObservableCollection<ChatMessageViewModel> _ChatMessageViewModelCollection { get; set; } = [];
        private Conversation _Conversation { get; set; }
        private XamlRoot _XamlRoot;
        private DispatcherQueue _DispatcherQueue;
        private IDialogsService _DialogsService;
        private IProgress<CompletionResponse> _SubjectProgress { get; set; }
        private IProgress<ChatResponse>? _ChatProgress { get; set; }

        private bool _IsSendingMessage { get; set; } = false;
        private bool _IsInputEnabled { get; set; } = true;

        public List<string> AvailableModels { get; set; }

        public SymbolIcon SendChatButtonIcon { get; set; } = new(Symbol.Send);

        public ConversationViewModel(Conversation conversation, XamlRoot root, DispatcherQueue dispatcherQueue, List<string> availableModels)
        {
            _Conversation = conversation;
            _XamlRoot = root;
            _DispatcherQueue = dispatcherQueue;
            _DialogsService = App.GetRequiredService<IDialogsService>();
            AvailableModels = availableModels;


            foreach (ChatMessage chatMessage in conversation.ChatMessageCollection)
            {
                _ChatMessageViewModelCollection.Add(new(chatMessage));
            }

            _SubjectProgress = new Progress<CompletionResponse>(r => Subject += r.response ?? "");

            _Conversation.StartOfChatRequest += _Conversation_StartOfChatRequest;
            _Conversation.EndOfChatRequest += _Conversation_EndOfChatRequest;
            _Conversation.UnhandledException += _Conversation_UnhandledException;

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? MessageRecieved;

        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
        protected void OnMessageRecieved(EventArgs e) => MessageRecieved?.Invoke(this, e);

        public ObservableCollection<ChatMessageViewModel> ChatMessages
        {
            get => _ChatMessageViewModelCollection;
            set
            {
                _ChatMessageViewModelCollection = value;
                OnPropertyChanged();
            }
        }

        public string Subject
        {
            get => _Conversation.Subject ?? "New Conversation";
            set
            {
                _Conversation.Subject = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedModel
        {
            get => _Conversation.SelectedModel;
            set
            {
                _Conversation.SelectedModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsSendingMessage
        {
            get => _IsSendingMessage;
            set
            {
                _IsSendingMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsInputEnabled
        {
            get => _IsInputEnabled;
            set
            {
                _IsInputEnabled = value;
                OnPropertyChanged();
            }
        }

        public Conversation Conversation
        {
            get => _Conversation;
        }

        private void _Conversation_StartOfChatRequest(object? sender, EventArgs e)
        {
            IsSendingMessage = true;
            IsInputEnabled = false;
        }

        private void _Conversation_EndOfChatRequest(object? sender, Conversation.EventArgs.EndOfChatRequest e)
        {
            IsSendingMessage = false;
            IsInputEnabled = true;
            SendChatButtonIcon = new SymbolIcon(Symbol.Send);
        }

        private void _Conversation_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(_XamlRoot, (Exception)e.ExceptionObject);

            _DispatcherQueue.TryEnqueue(async () =>
            {
                await _DialogsService.QueueDialog(dialog);

            });
        }

        public void Cancel()
        {
            _Conversation.Cancel();
            IsSendingMessage = false;
        }

        public void GenerateSubject(string prompt)
        {
            Subject = "";
            _DispatcherQueue.TryEnqueue(async () => await _Conversation.GenerateSubject(prompt, AvailableModels, _SubjectProgress));
        }

        public void SetSystemMessage(string prompt) => _Conversation.SetSystemMessage(prompt);

        public void SendUserChatMessage(string prompt)
        {
            foreach(ChatMessage message in _Conversation.BuildUserChatMessageAndResponse(prompt))
            {
                ChatMessageViewModel viewModel = new(message);
                _ChatMessageViewModelCollection.Add(viewModel);
            }

            _DispatcherQueue.TryEnqueue(async () => await SendMessages(_ChatMessageViewModelCollection.Last()));
        }

        private async Task SendMessages(ChatMessageViewModel responseViewModel)
        {
            StringBuilder responseContent = new();
            _ChatProgress = new Progress<ChatResponse>(r => 
            {
                responseContent.Append(r.message?.content ?? "");
                responseViewModel.Content = responseContent.ToString();
            });


            responseViewModel.ProgressRingEnabled = true;
            await _Conversation.SendChatRequest(_ChatProgress);
            responseContent.Clear();
            responseViewModel.Timestamp = DateTime.Now;
            responseViewModel.ProgressRingEnabled = false;
            OnMessageRecieved(EventArgs.Empty);
        }
    }
}
