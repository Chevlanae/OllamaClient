using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelSidebarPage : Page
    {
        public class NavArgs(Frame contentFrame)
        {
            public Frame ContentFrame { get; set; } = contentFrame;
        }

        private Frame? _ContentFrame { get; set; }
        private DialogsService _DialogsService { get; set; }
        private ModelSidebarViewModel _SidebarViewModel { get; set; }

        public ModelSidebarPage()
        {
            if (App.GetService<DialogsService>() is DialogsService dialogsService)
            {
                _DialogsService = dialogsService;
            }
            else throw new ArgumentException(nameof(dialogsService));

            if (App.GetService<ModelSidebarViewModel>() is ModelSidebarViewModel viewModel)
            {
                _SidebarViewModel = viewModel;
            }
            else throw new ArgumentException(nameof(viewModel));

            InitializeComponent();
            ModelsListView.ItemsSource = _SidebarViewModel.Items;

            _SidebarViewModel.UnhandledException += ModelList_UnhandledException;
            _SidebarViewModel.ModelDeleted += ModelList_ModelDeleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                _ContentFrame = args.ContentFrame;
                
                if(_SidebarViewModel.LastUpdated == null || _SidebarViewModel.LastUpdated < DateTime.Now.AddMinutes(-5))
                {
                    Refresh();
                }
            }

            ModelsListView.SelectedIndex = -1;
        }

        private void Refresh()
        {
            DispatcherQueue?.TryEnqueue(async () =>
            {
                await _SidebarViewModel.LoadModels();
                foreach (ModelViewModel item in _SidebarViewModel.Items)
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
                await _DialogsService.ShowDialog(dialog);
            });
        }

        private void ModelList_ModelDeleted(object? sender, EventArgs e)
        {
            _ContentFrame?.Navigate(typeof(BlankPage));
        }

        private void ModelsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelsListView.SelectedItem is ModelViewModel item && DispatcherQueue is not null)
            {
                _ContentFrame?.Navigate(typeof(ModelItemPage), new ModelItemPage.NavArgs(item, _SidebarViewModel));
            }
        }

        private void CreateModelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DispatcherQueue is not null)
            {
                _ContentFrame?.Navigate(typeof(CreateModelPage), new CreateModelPage.NavArgs(DispatcherQueue, _SidebarViewModel));
            }
        }

        private async void PullModelButton_Click(object sender, RoutedEventArgs e)
        {
            PullModelContentDialog dialog = new(XamlRoot);

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && (dialog.Content as TextBoxDialog)?.InputText is string modelName)
            {
                DispatcherQueue?.TryEnqueue(async () => { await _SidebarViewModel.PullModel(modelName); });
            }
        }

        private void RefreshModelsButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
