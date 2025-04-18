using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ConversationPageNavigationArgs(Conversation conversation, DispatcherQueue dispatcherQueue, List<string> availableModels)
    {
        public Conversation Conversation { get; set; } = conversation;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        public List<string> AvailableModels { get; set; } = availableModels;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class ConversationPage : Page
    {
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private Conversation? Conversation { get; set; }
        private List<string>? AvailableModels { get; set; }
        private bool EnableAutoScroll { get; set; }
        private bool SendingMessage { get; set; }

        public ConversationPage()
        {
            InitializeComponent();

            EnableAutoScroll = false;
            SendingMessage = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ConversationPageNavigationArgs args)
            {
                Conversation = args.Conversation;
                DispatcherQueue = args.DispatcherQueue;
                AvailableModels = args.AvailableModels;

                Conversation.StartOfRequest += Conversation_StartOfMessage;
                Conversation.EndOfResponse += Conversation_EndOfMessage;
                Conversation.UnhandledException += Conversation_UnhandledException;

                ChatItemsControl.ItemsSource = Conversation.Items;
                ModelsComboBox.ItemsSource = AvailableModels;

                ScrollLockButton.IsChecked = true;
                EnableAutoScroll = true;

                int index = AvailableModels.IndexOf(Conversation.SelectedModel ?? "");
                if(index == -1)
                {
                    ModelsComboBox.SelectedIndex = 0;
                }
                else
                {
                    ModelsComboBox.SelectedIndex = index;
                }
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Conversation is not null)
            {
                Conversation.StartOfRequest -= Conversation_StartOfMessage;
                Conversation.EndOfResponse -= Conversation_EndOfMessage;
                Conversation.UnhandledException -= Conversation_UnhandledException;
            }
            base.OnNavigatedFrom(e);
        }

        private void ChatBubbleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChatItemsControl_AutoScrollToBottom(sender, new());
        }

        private void Conversation_StartOfMessage(object? sender, EventArgs e)
        {
            SendingMessage = true;
            ChatInputTextBox.IsEnabled = false;
        }

        private void Conversation_EndOfMessage(object? sender, EventArgs e)
        {
            SendingMessage = false;
            ChatInputTextBox.IsEnabled = true;
            SendChatButton.Icon = new SymbolIcon(Symbol.Send);
        }

        private void Conversation_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue?.TryEnqueue(async () =>
            {
                await Services.Dialogs.ShowDialog(dialog);

            });
        }

        private void SendChatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (SendingMessage)
            {
                Conversation?.Cancel();
            }
            else if (Conversation != null)
            {
                string text = ChatInputTextBox.Text;

                if(Conversation.Subject == null)
                {
                    DispatcherQueue?.TryEnqueue(async () => { await Conversation.GenerateSubject(text); });
                }

                DispatcherQueue?.TryEnqueue(async () => { await Conversation.SendUserMessage(text); });

                ChatInputTextBox.Text = "";
            }
        }

        private void ChatItemsControl_AutoScrollToBottom(object? sender, RoutedEventArgs e)
        {
            if (EnableAutoScroll && ChatItemsScrollView.State == ScrollingInteractionState.Idle)
            {
                ChatItemsScrollView.ScrollTo(ChatItemsScrollView.HorizontalOffset, ChatItemsScrollView.ScrollableHeight);
            }
        }

        private void ModelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ModelsComboBox.SelectedItem is string selectedModel && Conversation != null)
            {
                Conversation.SelectedModel = selectedModel;
            }
        }

        private void ChatItemGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ChatItemsControl_AutoScrollToBottom(sender, e);
        }

        private void SendChatButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (SendingMessage)
            {
                SendChatButton.Opacity = 0;
                SendChatButton.Icon = new SymbolIcon(Symbol.Cancel);
                SendChatButton.Opacity = 1;
            }
        }

        private void SendChatButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if(SendingMessage)
            {
                SendChatButton.Opacity = 0;
                SendChatButton.Icon = new SymbolIcon(Symbol.Send);
                SendChatButton.Opacity = 1;
            }
        }

        private void ScrollLockButton_Click(object sender, RoutedEventArgs e)
        {
            EnableAutoScroll = ScrollLockButton.IsChecked == true;
        }
    }
}
