using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class ConversationPage : Page
    {
        public class NavArgs(List<string> availableModels, ConversationViewModel conversation)
        {
            public List<string> AvailableModels { get; set; } = availableModels;
            public ConversationViewModel ConversationViewModel { get; set; } = conversation;
        }

        private ConversationViewModel? _ConversationViewModel { get; set; }
        private List<string>? _AvailableModels { get; set; }
        private bool _EnableAutoScroll { get; set; }

        public ConversationPage()
        {
            InitializeComponent();

            _EnableAutoScroll = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                _AvailableModels = args.AvailableModels;
                _ConversationViewModel = args.ConversationViewModel;

                ChatMessagesControl.ItemsSource = _ConversationViewModel.ChatMessages;
                ModelsComboBox.ItemsSource = _AvailableModels;

                SubjectTextBlock.DataContext = _ConversationViewModel;

                ScrollLockButton.IsChecked = true;
                _EnableAutoScroll = true;

                int index = _AvailableModels.IndexOf(_ConversationViewModel.SelectedModel ?? "");
                if (index == -1)
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

        private void ChatBubbleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChatMessagesControl_AutoScrollToBottom(sender, new());
        }

        private void SendChatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_ConversationViewModel?.IsSendingMessage == true)
            {
                _ConversationViewModel?.Cancel();
            }
            else if (_ConversationViewModel is not null)
            {
                string text = ChatInputTextBox.Text;

                if (_ConversationViewModel.Subject == "New Conversation")
                {
                    _ConversationViewModel.GenerateSubject(text);
                }

                _ConversationViewModel.SendUserChatMessage(text);

                ChatInputTextBox.Text = "";
            }
        }

        private void ChatMessagesControl_AutoScrollToBottom(object? sender, RoutedEventArgs e)
        {
            if (_EnableAutoScroll && ChatMessagesScrollView.State == ScrollingInteractionState.Idle)
            {
                ChatMessagesScrollView.ScrollTo(ChatMessagesScrollView.HorizontalOffset, ChatMessagesScrollView.ScrollableHeight);
            }
        }

        private void ModelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelsComboBox.SelectedItem is string selectedModel && _ConversationViewModel != null)
            {
                _ConversationViewModel.SelectedModel = selectedModel;
            }
        }

        private void ChatMessagesGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ChatMessagesControl_AutoScrollToBottom(sender, e);
        }

        private void SendChatButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_ConversationViewModel?.IsSendingMessage == true)
            {
                SendChatButton.Opacity = 0;
                SendChatButton.Icon = new SymbolIcon(Symbol.Cancel);
                SendChatButton.Opacity = 1;
            }
        }

        private void SendChatButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_ConversationViewModel?.IsSendingMessage == true)
            {
                SendChatButton.Opacity = 0;
                SendChatButton.Icon = new SymbolIcon(Symbol.Send);
                SendChatButton.Opacity = 1;
            }
        }

        private void ScrollLockButton_Click(object sender, RoutedEventArgs e)
        {
            _EnableAutoScroll = ScrollLockButton.IsChecked == true;
        }
    }
}
