using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversationSidebarPage : Page
    {
        public class NavArgs(Frame contentFrame)
        {
            public Frame ContentFrame { get; set; } = contentFrame;
        }

        private Frame? ContentFrame { get; set; }
        private IDialogsService _DialogsService;
        private ConversationSidebarViewModel _ConversationsSidebarViewModel { get; set; }

        public ConversationSidebarPage()
        {
            _DialogsService = App.GetRequiredService<IDialogsService>();
            _ConversationsSidebarViewModel = new();

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                ContentFrame = args.ContentFrame;

                _ConversationsSidebarViewModel.ConversationViewModelCollection.CollectionChanged += Conversations_CollectionChanged;
                _ConversationsSidebarViewModel.ConversationCollection.ConversationsLoaded += Conversations_ConversationsLoaded;
                _ConversationsSidebarViewModel.ConversationCollection.ModelsLoaded += Conversations_ModelsLoaded;
                _ConversationsSidebarViewModel.ConversationCollection.UnhandledException += Conversations_UnhandledException;

                if (_ConversationsSidebarViewModel.ConversationViewModelCollection.Count == 0)
                {
                    DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.LoadConversations(); });
                }

                ConversationsListView.ItemsSource = _ConversationsSidebarViewModel.ConversationViewModelCollection;
                ConversationsListView.SelectedIndex = -1;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (ConversationViewModel viewModel in _ConversationsSidebarViewModel.ConversationViewModelCollection)
            {
                viewModel.MessageRecieved -= Conversation_MessageRecieved;
            }

            _ConversationsSidebarViewModel.ConversationViewModelCollection.CollectionChanged -= Conversations_CollectionChanged;
            _ConversationsSidebarViewModel.ConversationCollection.ConversationsLoaded -= Conversations_ConversationsLoaded;
            _ConversationsSidebarViewModel.ConversationCollection.ModelsLoaded -= Conversations_ModelsLoaded;
            _ConversationsSidebarViewModel.ConversationCollection.UnhandledException -= Conversations_UnhandledException;

            base.OnNavigatedFrom(e);
        }

        private void Conversation_MessageRecieved(object? sender, EventArgs e)
        {
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.Save(); });
        }

        private void Conversations_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_ConversationsSidebarViewModel.ConversationViewModelCollection.Count == 0)
            {
                ContentFrame?.Navigate(typeof(BlankPage));
            }
        }

        private void Conversations_ConversationsLoaded(object? sender, EventArgs e)
        {
            if (_ConversationsSidebarViewModel.ConversationViewModelCollection.Count == 0) ContentFrame?.Navigate(typeof(BlankPage));
            else
            {
                foreach (ConversationViewModel viewModel in _ConversationsSidebarViewModel.ConversationViewModelCollection)
                {
                    viewModel.MessageRecieved += Conversation_MessageRecieved;
                }
            }
        }

        private void Conversations_ModelsLoaded(object? sender, EventArgs e)
        {
            if (ConversationsListView.SelectedItem is ConversationViewModel conversation)
            {
                ConversationPage.NavArgs args = new(_ConversationsSidebarViewModel.ConversationCollection.AvailableModels, conversation);

                ContentFrame?.Navigate(typeof(ConversationPage), args);
            }
        }

        private void Conversations_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ConversationsSidebarViewModel.AvailableModels.Count == 0)
            {
                DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.LoadAvailableModels(); });
            }
            else if (ConversationsListView.SelectedItem is ConversationViewModel conversation)
            {
                ConversationPage.NavArgs args = new(_ConversationsSidebarViewModel.AvailableModels, conversation);

                ContentFrame?.Navigate(typeof(ConversationPage), args);
            }
        }

        private void DeleteConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.DataContext is ConversationViewModel c)
            {
                c.Cancel();
                c.MessageRecieved -= Conversation_MessageRecieved;
                _ConversationsSidebarViewModel.ConversationViewModelCollection.Remove(c);
                DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.Save(); });
            }
        }

        private void AddConversationButton_Click(object sender, RoutedEventArgs e)
        {
            IConversation conversation = App.GetRequiredService<IConversation>();
            ConversationViewModel viewModel = new((Conversation)conversation, XamlRoot, DispatcherQueue, _DialogsService);
            viewModel.MessageRecieved += Conversation_MessageRecieved;
            _ConversationsSidebarViewModel.ConversationViewModelCollection.Add(viewModel);
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.Save(); });
        }

        private void RefreshConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.LoadAvailableModels(); });
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.ConversationCollection.LoadConversations(); });
        }
    }
}
