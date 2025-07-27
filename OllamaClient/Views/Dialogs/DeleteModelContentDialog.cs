using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace OllamaClient.Views.Dialogs
{
    internal class DeleteModelContentDialog : ContentDialog
    {
        public DeleteModelContentDialog(XamlRoot xamlRoot, string modelName)
        {
            Title = $"Delete '{modelName}'";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Delete";
            CloseButtonText = "Cancel";
            Content = $"Are you sure you want to delete this model?";
            XamlRoot = xamlRoot;
        }
    }
}
