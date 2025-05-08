using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelSidebarPage : Page
    {
        public class NavArgs(Frame contentFrame, DispatcherQueue dispatcherQueue)
        {
            public Frame ContentFrame { get; set; } = contentFrame;
            public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        }

        private Frame? ContentFrame { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelSidebar ModelList { get; set; } = new();

        public ModelSidebarPage()
        {
            InitializeComponent();
            ModelsListView.ItemsSource = ModelList.Items;

            ModelList.UnhandledException += ModelList_UnhandledException;
            ModelList.ModelDeleted += ModelList_ModelDeleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                
                if(ModelList.LastUpdated == null || ModelList.LastUpdated < DateTime.Now.AddMinutes(-5))
                {
                    Refresh();
                }
            }
        }

        private void Refresh()
        {
            DispatcherQueue?.TryEnqueue(async () =>
            {
                await ModelList.LoadModels();
                foreach (Model item in ModelList.Items)
                {
                    item.LastUpdated = null;
                }
            });
        }

        private void ModelList_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue?.TryEnqueue(async () =>
            {
                await Services.Dialogs.ShowDialog(dialog);
            });
        }

        private void ModelList_ModelDeleted(object? sender, EventArgs e)
        {
            ContentFrame?.Navigate(typeof(BlankPage));
        }

        private void ModelsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelsListView.SelectedItem is Model item && DispatcherQueue is not null)
            {
                ContentFrame?.Navigate(typeof(ModelItemPage), new ModelItemPage.NavArgs(DispatcherQueue, item, ModelList));
            }
        }

        private void CreateModelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DispatcherQueue is not null)
            {
                ContentFrame?.Navigate(typeof(CreateModelPage), new CreateModelPage.NavArgs(DispatcherQueue, ModelList));
            }
        }

        private async void PullModelButton_Click(object sender, RoutedEventArgs e)
        {
            PullModelContentDialog dialog = new(XamlRoot);

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && (dialog.Content as TextBoxDialog)?.InputText is string modelName)
            {
                DispatcherQueue?.TryEnqueue(async () => { await ModelList.PullModel(modelName); });
            }
        }

        private void RefreshModelsButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
