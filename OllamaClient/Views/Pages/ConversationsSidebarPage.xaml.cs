using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models.Ollama;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private Client OllamaClient { get; set; }

        public ConversationsSidebarPage()
        {
            Conversations = new();
            Conversations.Items.CollectionChanged += ConversationItems_CollectionChanged;
            Conversations.ModelTagsLoaded += Conversations_ModelTagsLoaded;
            Conversations.UnhandledException += Conversations_UnhandledException;
            OllamaClient = new();

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ConversationsSideBarPageNavigationArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadSavedConversations(); });
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadAvailableModels(OllamaClient); });

                ConversationsListView.ItemsSource = Conversations.Items;

                if(Conversations.Items.Count == 0)
                {
                    ContentFrame?.Navigate(typeof(ConversationsBlankPage));
                }
                else
                {
                    ConversationsListView.SelectedIndex = 0;
                }
            }
            base.OnNavigatedTo(e);
        }

        private void ConversationItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Conversations.Items.Count == 0)
            {
                ContentFrame?.Navigate(typeof(ConversationsBlankPage));
            }
        }

        private async void Conversations_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            await Task.Run(() =>
            {
                DispatcherQueue?.TryEnqueue(() =>
                {
                    new ErrorPopupWindow("An error occurred", e.ExceptionObject.ToString() ?? "").Activate();
                });
            });
        }

        private void Conversations_ModelTagsLoaded(object? sender, EventArgs e)
        {
            if (Conversations.Items.Count > 0)
            {
                ConversationsListView.SelectedIndex = 0;
            }
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationsListView.SelectedItem is Conversation conversation && DispatcherQueue != null)
            {
                ConversationPageNavigationArgs args = new(conversation, DispatcherQueue, OllamaClient, Conversations.AvailableModels.ToList());

                ContentFrame?.Navigate(typeof(ConversationPage), args);
            }
        }

        private void DeleteConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.DataContext is Conversation c)
            {
                c.Cancel();
                Conversations.Items.Remove(c);
                DispatcherQueue?.TryEnqueue(async () => { await Conversations.Save(); });
            }
        }

        private void AddConversationButton_Click(object sender, RoutedEventArgs e)
        {
            Conversations.Create();
            ConversationsListView.SelectedIndex = Conversations.Items.Count - 1;
        }
    }
}
