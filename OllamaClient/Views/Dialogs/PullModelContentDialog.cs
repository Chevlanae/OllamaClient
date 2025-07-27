using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace OllamaClient.Views.Dialogs
{
    internal class PullModelContentDialog : ContentDialog
    {
        public PullModelContentDialog(XamlRoot xamlRoot)
        {
            Title = "Pull Model";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Pull";
            CloseButtonText = "Cancel";
            Content = new TextBoxDialog(CreatePullDialogParagraph(), "Model name");
            XamlRoot = xamlRoot;
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
    }
}
