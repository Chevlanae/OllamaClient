using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
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
            Conversations.UnhandledException += Conversations_UnhandledException;

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ConversationsSideBarPageNavigationArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadModels(); });

                if (Conversations.Items.Count == 0)
                {
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Conversations.LoadSaved();

                        foreach (Conversation conversation in Conversations.Items)
                        {
                            conversation.StartOfRequest += Conversation_StartOfMessage;
                            conversation.EndOfResponse += Conversation_EndOfMessasge;
                        }
                    });

                    ContentFrame?.Navigate(typeof(ConversationsBlankPage));
                }
            }

            ConversationsListView.ItemsSource = Conversations.Items;
            ConversationsListView.SelectedIndex = -1;

            base.OnNavigatedTo(e);
        }

        private void Conversation_StartOfMessage(object? sender, System.EventArgs e)
        {
            DispatcherQueue?.TryEnqueue(async () =>
            {
                await Conversations.Save();
            });
        }

        private void Conversation_EndOfMessasge(object? sender, System.EventArgs e)
        {
            DispatcherQueue?.TryEnqueue(async () =>
            {
                await Conversations.Save();
            });
        }

        private void ConversationItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Conversations.Items.Count == 0)
            {
                ContentFrame?.Navigate(typeof(ConversationsBlankPage));
            }
        }

        private void Conversations_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            DispatcherQueue?.TryEnqueue(() =>
            {
                new ErrorPopupWindow("An error occurred", e.ExceptionObject.ToString() ?? "").Activate();
            });
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationsListView.SelectedItem is Conversation conversation && DispatcherQueue != null)
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
    }
}
