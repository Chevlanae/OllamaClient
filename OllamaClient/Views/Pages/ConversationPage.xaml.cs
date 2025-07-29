using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class ConversationPage : Page
    {
        public class NavArgs(ObservableCollection<string> availableModels, ConversationViewModel viewModel)
        {
            public ObservableCollection<string> AvailableModels { get; set; } = availableModels;
            public ConversationViewModel ViewModel { get; set; } = viewModel;
        }

        private DialogsService _DialogsService;
        private ConversationViewModel? _ConversationViewModel { get; set; }
        private List<string>? _AvailableModels { get; set; }
        private bool _EnableAutoScroll { get; set; }
        private bool _SendingMessage { get; set; }

        public ConversationPage()
        {
            if (App.GetService<DialogsService>() is DialogsService dialogs)
            {
                _DialogsService = dialogs;
            }
            else throw new ArgumentException(nameof(dialogs));

            InitializeComponent();

            _EnableAutoScroll = false;
            _SendingMessage = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                _AvailableModels = args.AvailableModels.ToList();
                _ConversationViewModel = args.ViewModel;

                _ConversationViewModel.StartOfRequest += ConversationViewModel_StartOfRequest;
                _ConversationViewModel.EndOfResponse += ConversationViewModel_EndOfResponse;
                _ConversationViewModel.UnhandledException += ConversationViewModel_UnhandledException;

                ChatMessagesControl.ItemsSource = _ConversationViewModel.ChatMessages;
                ModelsComboBox.ItemsSource = _AvailableModels;

                ScrollLockButton.IsChecked = true;
                _EnableAutoScroll = true;

                int index = _AvailableModels.IndexOf(_ConversationViewModel.SelectedModel ?? "");
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
            if (_ConversationViewModel is not null)
            {
                _ConversationViewModel.StartOfRequest -= ConversationViewModel_StartOfRequest;
                _ConversationViewModel.EndOfResponse -= ConversationViewModel_EndOfResponse;
                _ConversationViewModel.UnhandledException -= ConversationViewModel_UnhandledException;
            }
            base.OnNavigatedFrom(e);
        }

        private void ChatBubbleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChatMessagesControl_AutoScrollToBottom(sender, new());
        }

        private void ConversationViewModel_StartOfRequest(object? sender, EventArgs e)
        {
            _SendingMessage = true;
            ChatInputTextBox.IsEnabled = false;
        }

        private void ConversationViewModel_EndOfResponse(object? sender, EventArgs e)
        {
            _SendingMessage = false;
            ChatInputTextBox.IsEnabled = true;
            SendChatButton.Icon = new SymbolIcon(Symbol.Send);
        }

        private void ConversationViewModel_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue.TryEnqueue(async () =>
            {
                await _DialogsService.ShowDialog(dialog);

            });
        }

        private void SendChatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_SendingMessage)
            {
                _ConversationViewModel?.Cancel();
            }
            else if (_ConversationViewModel is not null)
            {
                string text = ChatInputTextBox.Text;

                if(_ConversationViewModel.Subject == "New Conversation")
                {
                    DispatcherQueue.TryEnqueue(async () => { await _ConversationViewModel.GenerateSubject(text); });
                }

                DispatcherQueue.TryEnqueue(async () => { await _ConversationViewModel.SendUserMessage(text); });

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
            if(ModelsComboBox.SelectedItem is string selectedModel && _ConversationViewModel != null)
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
            if (_SendingMessage)
            {
                SendChatButton.Opacity = 0;
                SendChatButton.Icon = new SymbolIcon(Symbol.Cancel);
                SendChatButton.Opacity = 1;
            }
        }

        private void SendChatButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if(_SendingMessage)
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
