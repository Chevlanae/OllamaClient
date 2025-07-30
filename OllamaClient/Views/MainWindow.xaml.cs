using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using OllamaClient.Views.Pages;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ConversationsButton_Click(this, new());
        }

        private void ToggleSidebar()
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
        }

        private void ConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ConversationSidebarPage))
            {
                ConversationSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ConversationSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ModelSidebarPage))
            {
                ModelSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ModelSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSidebar();
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (LogsPopup.IsOpen) LogsPopup.IsOpen = false;
            else
            {
                LogsPopup.IsOpen = true;

                DispatcherQueue.TryEnqueue(async () =>
                {
                    while (LogsPopup.IsOpen)
                    {
                        LogsTextBlock.Text = App.LoggedText.ToString();

                        await Task.Delay(10);
                    }
                });
            }
        }

        private void LogsTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LogsScrollViewer.VerticalOffset > LogsScrollViewer.ScrollableHeight - 100)
            {
                LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
            }
        }

        private async void LogsFolderHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(App.LogsMsixPath);

            await Launcher.LaunchFolderAsync(folder);
        }

        private void LogsPopup_Opened(object sender, object e)
        {
            LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
        }
    }
}

