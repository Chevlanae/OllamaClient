using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using OllamaClient.ViewModels;
using OllamaClient.Views.Pages;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
        private SerilogObserverViewModel SerilogObserver;

        public MainWindow()
        {
            InitializeComponent();

            ContentFrame.Navigate(typeof(BlankPage));

            SerilogObserver = new(DispatcherQueue);

            App.LogEvents?.Subscribe(SerilogObserver);

            LogsItemsView.ItemsSource = SerilogObserver.Logs;
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
            LogsPopup.IsOpen = !LogsPopup.IsOpen;
        }

        private async void LogsFolderHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(App.LogsDirectoryMsixPath);

            await Launcher.LaunchFolderAsync(folder);
        }

        private void LogsPopup_Opened(object sender, object e)
        {
            LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
        }

        private void LogsItemsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LogsScrollViewer.VerticalOffset > LogsScrollViewer.ScrollableHeight - 150)
            {
                LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
            }
        }

        private void CopyKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            StringBuilder builder = new();
            foreach(var item in LogsItemsView.SelectedItems)
            {
                switch (item)
                {
                    case char character:
                        if (character == '\u00A0')
                        {
                            builder.Append("\n");
                        }
                        else
                        {
                            builder.Append(character);
                        }
                        break;
                    case string stringObj:
                        builder.Append(stringObj);
                        break;
                }
            }
            DataPackage package = new()
            {
                RequestedOperation = DataPackageOperation.Copy
            };

            package.SetText(builder.ToString());

            Clipboard.SetContent(package);
        }

        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

