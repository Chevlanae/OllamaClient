using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using OllamaClient.ViewModels;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ConversationsSideBarPageNavigationArgs(Frame contentFrame, DispatcherQueue dispatcherQueue)
    {
        public Frame ContentFrame { get; set; } = contentFrame;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversationsSidebarPage : Page
    {
        private Frame? ContentFrame { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private Conversations Conversations { get; set; }

        public ConversationsSidebarPage()
        {
            Conversations = new();
            Conversations.Items.CollectionChanged += ConversationItems_CollectionChanged;
            Conversations.ConversationsLoaded += Conversations_ConversationsLoaded;
            Conversations.UnhandledException += Conversations_UnhandledException;

            InitializeComponent();

            ConversationsListView.ItemsSource = Conversations.Items;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ConversationsSideBarPageNavigationArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                if(Conversations.LastUpdated == null || Conversations.LastUpdated < DateTime.Now.AddMinutes(-5))
                {
                    Refresh();
                }
            }

            ConversationsListView.SelectedIndex = -1;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (Conversation conversation in Conversations.Items)
            {
                conversation.StartOfRequest -= Conversation_StartOfMessage;
                conversation.EndOfResponse -= Conversation_EndOfMessasge;
            }
            base.OnNavigatedFrom(e);
        }

        private void Refresh()
        {
            DispatcherQueue?.TryEnqueue(async () => { await Conversations.LoadAvailableModels(); });
            DispatcherQueue?.TryEnqueue(async () => { await Conversations.LoadConversations(); });
        }

        private void Conversation_StartOfMessage(object? sender, EventArgs e)
        {
            DispatcherQueue?.TryEnqueue(async () => { await Conversations.Save(); });
        }

        private void Conversation_EndOfMessasge(object? sender, EventArgs e)
        {
            DispatcherQueue?.TryEnqueue(async () => { await Conversations.Save(); });
        }

        private void Conversations_ConversationsLoaded(object? sender, EventArgs e)
        {
            if(Conversations.Items.Count == 0) ContentFrame?.Navigate(typeof(BlankPage));
            else
            {
                foreach (Conversation conversation in Conversations.Items)
                {
                    conversation.StartOfRequest += Conversation_StartOfMessage;
                    conversation.EndOfResponse += Conversation_EndOfMessasge;
                }
            }
        }

        private void ConversationItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Conversations.Items.Count == 0)
            {
                ContentFrame?.Navigate(typeof(BlankPage));
            }
        }

        private void Conversations_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue?.TryEnqueue(async () => { await Services.Dialogs.ShowDialog(dialog); });
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationsListView.SelectedItem is Conversation conversation && DispatcherQueue is not null)
            {
                ConversationPageNavigationArgs args = new(conversation, DispatcherQueue, Conversations.AvailableModels.ToList());

                ContentFrame?.Navigate(typeof(ConversationPage), args);
            }
        }

        private void DeleteConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.DataContext is Conversation c)
            {
                c.Cancel();
                c.StartOfRequest -= Conversation_StartOfMessage;
                c.EndOfResponse -= Conversation_EndOfMessasge;
                Conversations.Items.Remove(c);
                DispatcherQueue?.TryEnqueue(async () => { await Conversations.Save(); });
            }
        }

        private void AddConversationButton_Click(object sender, RoutedEventArgs e)
        {
            Conversation conversation = new();
            conversation.StartOfRequest += Conversation_StartOfMessage;
            conversation.EndOfResponse += Conversation_EndOfMessasge;
            Conversations.Items.Add(conversation);
            DispatcherQueue?.TryEnqueue(async () => { await Conversations.Save(); });
        }

        private void RefreshConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
