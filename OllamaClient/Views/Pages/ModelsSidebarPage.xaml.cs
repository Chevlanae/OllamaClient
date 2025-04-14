using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ModelsSidebarPageNavigationArgs(Frame contentFrame, DispatcherQueue dispatcherQueue)
    {
        public Frame ContentFrame { get; set; } = contentFrame;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelsSidebarPage : Page
    {
        private Frame? ContentFrame { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelCollection ModelList { get; set; } = new();

        public ModelsSidebarPage()
        {
            InitializeComponent();
            ModelsListView.ItemsSource = ModelList.Items;

            ModelList.UnhandledException += ModelList_UnhandledException;
            ModelList.ModelDeleted += ModelList_ModelDeleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ModelsSidebarPageNavigationArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                DispatcherQueue.TryEnqueue(async () => { await ModelList.LoadModels(); });
            }
        }

        private Paragraph CreatePullDialogParagraph()
        {
            Paragraph pullModelParagraph = new();
            pullModelParagraph.Inlines.Add(new Run() { Text = "Enter the name of the model to pull from " });
            Hyperlink link = new Hyperlink() { NavigateUri = new("https://ollama.com/library") };
            link.Inlines.Add(new Run() { Text = "https://ollama.com/library" });
            pullModelParagraph.Inlines.Add(link);
            return pullModelParagraph;
        }

        private void ModelList_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            DispatcherQueue?.TryEnqueue(() => { new ErrorPopupWindow("An error occurred", e.ExceptionObject.ToString() ?? "").Activate(); });
        }

        private void ModelList_ModelDeleted(object? sender, EventArgs e)
        {
            ContentFrame?.Navigate(typeof(ConversationsBlankPage));
        }

        private void ModelsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelsListView.SelectedItem is ModelItem item && DispatcherQueue is not null)
            {
                ContentFrame?.Navigate(typeof(ModelItemPage), new ModelItemPageNavigationArgs(DispatcherQueue, item, ModelList));
            }
        }

        private void CreateModelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DispatcherQueue is not null)
            {
                ContentFrame?.Navigate(typeof(CreateModelPage), new CreateModelPageNavigationArgs(DispatcherQueue, ModelList));
            }
        }

        private async void PullModelButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Pull Model",
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "Pull",
                CloseButtonText = "Cancel",
                Content = new TextBoxDialog(CreatePullDialogParagraph(), "Model name"),
                XamlRoot = XamlRoot,
            };

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && (contentDialog.Content as TextBoxDialog)?.InputText is string modelName)
            {
                DispatcherQueue?.TryEnqueue(async () => { await ModelList.PullModel(modelName); });
            }
        }
    }
}
