using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace OllamaClient.Views.Dialogs
{
    internal class CopyModelContentDialog : ContentDialog
    {
        public CopyModelContentDialog(XamlRoot xamlRoot, string modelName)
        {
            Title = $"Copy '{modelName}'";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Copy";
            CloseButtonText = "Cancel";
            Content = new TextBoxDialog(CreateCopyDialogParagraph(), "Model name");
            XamlRoot = xamlRoot;
        }

        private Paragraph CreateCopyDialogParagraph()
        {
            Paragraph copyModelParagraph = new();
            copyModelParagraph.Inlines.Add(new Run() { Text = "Enter a name for the new copy" });
            return copyModelParagraph;
        }
    }
}
