using Microsoft.UI.Dispatching;
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
    public sealed partial class ModelItemPage : Page
    {
        public class NavArgs(ModelViewModel modelItem, ModelSidebarViewModel viewModel)
        {
            public ModelViewModel SelectedItem { get; set; } = modelItem;
            public ModelSidebarViewModel ModelSidebarViewModel { get; set; } = viewModel;
        }

        private DialogsService _DialogsService { get; set; }
        private ModelViewModel? Item { get; set; }
        private ModelSidebarViewModel? ModelSidebarViewModel { get; set; }

        public ModelItemPage()
        {
            if (App.GetService<DialogsService>() is DialogsService dialogsService)
            {
                _DialogsService = dialogsService;
            }
            else throw new ArgumentException(nameof(dialogsService));

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                Item = args.SelectedItem;
                ModelSidebarViewModel = args.ModelSidebarViewModel;

                Item.UnhandledException += Item_UnhandledException;

                ItemGrid.DataContext = Item;

                DetailsTextBox.Blocks.Add(Item.DetailsParagraph);
                ModelInfoTextBox.Blocks.Add(Item.ModelInfoParagraph);
                LicenseTextBox.Blocks.Add(Item.LicenseParagraph);
                ModelFileTextBox.Blocks.Add(Item.ModelFileParagraph);

                if (Item.LastUpdated == null || Item.LastUpdated < DateTime.Now.AddMinutes(-5))
                {
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Item.GetDetails();
                        Item.GenerateParagraphText();
                    });
                }
                else
                {
                    DispatcherQueue.TryEnqueue(() => Item.GenerateParagraphText());
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Item is not null)
            {
                Item.UnhandledException -= Item_UnhandledException;

                DetailsTextBox.Blocks.Clear();
                ModelInfoTextBox.Blocks.Clear();
                LicenseTextBox.Blocks.Clear();
                ModelFileTextBox.Blocks.Clear();
            }
        }

        private void Item_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }

        private void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DeleteModelContentDialog dialog = new(XamlRoot, Item?.Source?.Model ?? "");

            dialog.Closed += (s, args) =>
            {
                if (args.Result == ContentDialogResult.Primary && ModelSidebarViewModel is not null && Item?.Source is not null)
                {
                    DispatcherQueue.TryEnqueue(async () => { await ModelSidebarViewModel.DeleteModel(Item.Source.Model); });
                }
            };

            DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }

        private void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            CopyModelContentDialog dialog = new(XamlRoot, Item?.Source?.Model ?? "");

            dialog.Closed += (s, args) =>
            {
                if
                (
                args.Result == ContentDialogResult.Primary
                &&
                (dialog.Content as TextBoxDialog)?.InputText is string newModelName
                &&
                ModelSidebarViewModel is not null
                &&
                Item?.Source is not null
                )
                {
                    DispatcherQueue?.TryEnqueue(async () => { await ModelSidebarViewModel.CopyModel(Item.Source.Name, newModelName); });
                }
            };

            DispatcherQueue.TryEnqueue(async () => { await _DialogsService.QueueDialog(dialog); });
        }
    }
}
