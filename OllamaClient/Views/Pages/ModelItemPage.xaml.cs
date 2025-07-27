using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
        public class NavArgs(DispatcherQueue dispatcherQueue, ModelViewModel modelItem, ModelSidebarViewModel collection)
        {
            public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
            public ModelViewModel SelectedItem { get; set; } = modelItem;
            public ModelSidebarViewModel Collection { get; set; } = collection;
        }

        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelViewModel? Item { get; set; }
        private ModelSidebarViewModel? ParentCollection { get; set; }

        public ModelItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                DispatcherQueue = args.DispatcherQueue;
                Item = args.SelectedItem;
                ParentCollection = args.Collection;

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

            DispatcherQueue?.TryEnqueue(async () => { await Services.DialogsService.ShowDialog(dialog); });
        }

        private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DeleteModelContentDialog dialog = new(XamlRoot, Item?._Model ?? "");

            ContentDialogResult? result = await Services.DialogsService.ShowDialog(dialog);

            if (result == ContentDialogResult.Primary && ParentCollection is not null && Item is not null)
            {
                DispatcherQueue?.TryEnqueue(async () => { await ParentCollection.DeleteModel(Item._Model); });
            }
        }

        private async void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            CopyModelContentDialog dialog = new(XamlRoot, Item?._Model ?? "");

            ContentDialogResult? result = await Services.DialogsService.ShowDialog(dialog);

            if
            (
                result == ContentDialogResult.Primary
                &&
                (dialog.Content as TextBoxDialog)?.InputText is string newModelName
                &&
                ParentCollection is not null
                &&
                Item is not null
            )
            {
                DispatcherQueue?.TryEnqueue(async () => { await ParentCollection.CopyModel(Item.Name, newModelName); });
            }
        }
    }
}
