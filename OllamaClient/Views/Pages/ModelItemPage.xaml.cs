using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ModelItemPageNavigationArgs(DispatcherQueue dispatcherQueue, ModelItem modelItem, ModelCollection collection)
    {
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        public ModelItem SelectedItem { get; set; } = modelItem;
        public ModelCollection Collection { get; set; } = collection;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelItemPage : Page
    {
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelItem? Item { get; set; }
        private ModelCollection? ParentCollection { get; set; }

        public ModelItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ModelItemPageNavigationArgs args)
            {
                DispatcherQueue = args.DispatcherQueue;
                Item = args.SelectedItem;
                ParentCollection = args.Collection;

                ItemGrid.DataContext = Item;

                Paragraph detailsParagraph = new();
                detailsParagraph.Inlines.Add(new Run() { Text = Item.ToSummaryString() });
                DetailsTextBox.Blocks.Add(detailsParagraph);

                Paragraph modelInfoParagraph = new();
                modelInfoParagraph.Inlines.Add(new Run() { Text = Item.ModelInfo });
                ModelInfoTextBox.Blocks.Add(modelInfoParagraph);

                Paragraph licenseParagraph = new();
                if (Item.License is not null)
                {
                    licenseParagraph.Inlines.Add(new Run() { Text = Item.License });
                    LicenseTextBox.Blocks.Add(licenseParagraph);
                }
                else
                {
                    LicenseTextBox.Blocks.Add(new Paragraph() { Inlines = { new Run() { Text = "No license information available." } } });
                }
                if (Item.ModelFile is not null)
                {
                    Paragraph modelFileParagraph = new();
                    modelFileParagraph.Inlines.Add(new Run() { Text = Item.ModelFile.ToString() });
                    ModelFileTextBox.Blocks.Add(modelFileParagraph);
                }
            }
        }

        private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new()
            {
                Title = $"Delete '{Item?.Name}'",
                DefaultButton = ContentDialogButton.Primary,
                Content = $"Are you sure you want to delete this model?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && ParentCollection is not null && Item is not null)
            {
                DispatcherQueue?.TryEnqueue(async () => { await ParentCollection.DeleteModel(Item.Model); });
            }
        }

        private async void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Paragraph dialogParagraph = new();
            dialogParagraph.Inlines.Add(new Run() { Text = "Enter a name for the new copy" });

            ContentDialog copyDialog = new()
            {
                Title = $"Copy '{Item?.Name}'",
                DefaultButton = ContentDialogButton.Primary,
                Content = new TextBoxDialog(dialogParagraph, "Model name"),
                PrimaryButtonText = "Copy",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };

            ContentDialogResult result = await copyDialog.ShowAsync();

            if
            (
                result == ContentDialogResult.Primary
                &&
                (copyDialog.Content as TextBoxDialog)?.InputText is string newModelName
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
