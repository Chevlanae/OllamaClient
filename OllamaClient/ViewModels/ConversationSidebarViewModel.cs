using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using OllamaClient.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ConversationSidebarViewModel
    {
        private Frame _ContentFrame { get; set; }
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }
        private IDialogsService _DialogsService { get; set; }
        private ListView _ConversationsListView { get; set; }
        private ConversationCollection _ConversationCollection { get; set; }

        public ObservableCollection<ConversationViewModel> ConversationViewModelCollection { get; set; } = [];
        public List<string> AvailableModels { get; set; } = [];
        public DateTime? LastUpdated { get; set; }

        public ConversationSidebarViewModel(Frame contentFrame, XamlRoot xamlRoot, DispatcherQueue dispatcherQueue, ListView conversationsListView)
        {
            _ContentFrame = contentFrame;
            _XamlRoot = xamlRoot;
            _DispatcherQueue = dispatcherQueue;
            _DialogsService = App.GetRequiredService<IDialogsService>();
            _ConversationsListView = conversationsListView;

            _ConversationCollection = (ConversationCollection)App.GetRequiredService<IConversationCollection>();
            _ConversationCollection.ConversationsLoaded += ConversationCollection_ConversationsLoaded;
            ConversationViewModelCollection.CollectionChanged += ConversationViewModelCollection_CollectionChanged;
            _ConversationCollection.ModelsLoaded += ConversationCollection_ModelsLoaded;
            _ConversationCollection.UnhandledException += ConversationCollection_UnhandledException;

            if (ConversationViewModelCollection.Count == 0)
            {
                _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.LoadConversations(); });
            }

        }

        private void ConversationCollection_ConversationsLoaded(object? sender, EventArgs e)
        {
            ConversationViewModelCollection.Clear();
            foreach (Conversation conversation in _ConversationCollection.Items)
            {
                ConversationViewModel viewModel = new(conversation, _XamlRoot, _DispatcherQueue);
                viewModel.MessageRecieved += ConversationViewModel_MessageRecieved;
                ConversationViewModelCollection.Add(viewModel);
            }

            if (ConversationViewModelCollection.Count == 0) _ContentFrame.Navigate(typeof(BlankPage));
        }

        private void ConversationViewModel_MessageRecieved(object? sender, EventArgs e)
        {
            _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.Save(); });
        }

        private void ConversationViewModelCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ConversationViewModelCollection.Count == 0)
            {
                _ContentFrame.Navigate(typeof(BlankPage));
            }
        }

        private void ConversationCollection_ModelsLoaded(object? sender, EventArgs e)
        {
            if (_ConversationsListView.SelectedItem is ConversationViewModel conversation)
            {
                ConversationPage.NavArgs args = new(_ConversationCollection.AvailableModels, conversation);

                _ContentFrame.Navigate(typeof(ConversationPage), args);
            }
        }

        private void ConversationCollection_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(_XamlRoot, (Exception)e.ExceptionObject);

            _DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }

        public void ConversationsListView_SelectionChanged()
        {
            if (AvailableModels.Count == 0)
            {
                _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.LoadAvailableModels(); });
            }
            else if (_ConversationsListView.SelectedItem is ConversationViewModel conversation)
            {
                ConversationPage.NavArgs args = new(AvailableModels, conversation);

                _ContentFrame.Navigate(typeof(ConversationPage), args);
            }
        }

        public void DeleteConversation(ConversationViewModel c)
        {
            c.Cancel();
            c.MessageRecieved -= ConversationViewModel_MessageRecieved;
            _ConversationsListView.SelectedIndex = ConversationViewModelCollection.IndexOf(c) - 1;
            _ConversationCollection.Items.Remove(c.Conversation);
            ConversationViewModelCollection.Remove(c);
            _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.Save(); });
        }

        public void NewConversation()
        {
            IConversation conversation = App.GetRequiredService<IConversation>();
            ConversationViewModel viewModel = new((Conversation)conversation, _XamlRoot, _DispatcherQueue);
            viewModel.MessageRecieved += ConversationViewModel_MessageRecieved;
            _ConversationCollection.Items.Add(conversation);
            ConversationViewModelCollection.Add(viewModel);
            _ConversationsListView.SelectedItem = viewModel;
            _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.Save(); });
        }

        public void RefreshConversations()
        {
            _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.LoadAvailableModels(); });
            _DispatcherQueue.TryEnqueue(async () => { await _ConversationCollection.LoadConversations(); });
        }
    }
}
