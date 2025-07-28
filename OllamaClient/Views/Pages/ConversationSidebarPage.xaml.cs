using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
        private DialogsService _DialogsService;
        private ConversationSidebarViewModel _ConversationsSidebarViewModel { get; set; }

        public ConversationSidebarPage()
        {
            if (App.GetService<DialogsService>() is DialogsService dialogs)
            {
                _DialogsService = dialogs;
            }
            else throw new ArgumentException(nameof(dialogs));

            if (App.GetService<ConversationSidebarViewModel>() is ConversationSidebarViewModel conversationsSidebar)
            {
                _ConversationsSidebarViewModel = conversationsSidebar;
            }
            else throw new ArgumentException(nameof(conversationsSidebar));

            InitializeComponent();

            ConversationsListView.ItemsSource = _ConversationsSidebarViewModel.Conversations;

            _ConversationsSidebarViewModel.Conversations.CollectionChanged += ConversationItems_CollectionChanged;
            _ConversationsSidebarViewModel.ConversationsLoaded += Conversations_ConversationsLoaded;
            _ConversationsSidebarViewModel.UnhandledException += Conversations_UnhandledException;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                ContentFrame = args.ContentFrame;

                if (_ConversationsSidebarViewModel.Conversations.Count == 0)
                {
                    DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.LoadConversations(); });
                    DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.LoadAvailableModels(); });
                    ContentFrame.Navigate(typeof(BlankPage));
                }

                ConversationsListView.SelectedIndex = -1;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (ConversationViewModel viewModel in _ConversationsSidebarViewModel.Conversations)
            {
                viewModel.StartOfRequest -= Conversation_StartOfMessage;
                viewModel.EndOfResponse -= Conversation_EndOfMessasge;
            }
            base.OnNavigatedFrom(e);
        }

        private void Conversation_StartOfMessage(object? sender, EventArgs e)
        {
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.Save(); });
        }

        private void Conversation_EndOfMessasge(object? sender, EventArgs e)
        {
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.Save(); });
        }

        private void Conversations_ConversationsLoaded(object? sender, EventArgs e)
        {
            if (_ConversationsSidebarViewModel.Conversations.Count == 0) ContentFrame?.Navigate(typeof(BlankPage));
            else
            {
                foreach (ConversationViewModel viewModel in _ConversationsSidebarViewModel.Conversations)
                {
                    viewModel.StartOfRequest += Conversation_StartOfMessage;
                    viewModel.EndOfResponse += Conversation_EndOfMessasge;
                }
            }
        }

        private void ConversationItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_ConversationsSidebarViewModel.Conversations.Count == 0)
            {
                ContentFrame?.Navigate(typeof(BlankPage));
            }
        }

        private void Conversations_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue.TryEnqueue(async () => { await _DialogsService.ShowDialog(dialog); });
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationsListView.SelectedItem is ConversationViewModel conversation)
            {
                DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.LoadAvailableModels(); });

                ConversationPage.NavArgs args = new(_ConversationsSidebarViewModel.AvailableModels, conversation);

                ContentFrame?.Navigate(typeof(ConversationPage), args);
            }
        }

        private void DeleteConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.DataContext is ConversationViewModel c)
            {
                c.Cancel();
                c.StartOfRequest -= Conversation_StartOfMessage;
                c.EndOfResponse -= Conversation_EndOfMessasge;
                _ConversationsSidebarViewModel.Conversations.Remove(c);
                DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.Save(); });
            }
        }

        private void AddConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.GetService<ConversationViewModel>() is ConversationViewModel viewModel)
            {
                viewModel.StartOfRequest += Conversation_StartOfMessage;
                viewModel.EndOfResponse += Conversation_EndOfMessasge;
                _ConversationsSidebarViewModel.Conversations.Add(viewModel);
                DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.Save(); });
            }
        }

        private void RefreshConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.LoadAvailableModels(); });
            DispatcherQueue.TryEnqueue(async () => { await _ConversationsSidebarViewModel.LoadConversations(); });
        }
    }
}
