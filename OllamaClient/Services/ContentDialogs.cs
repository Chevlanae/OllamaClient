using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using OllamaClient.Views.Dialogs;
using System;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    namespace Dialogs
    {
        internal static class DialogService
        {
            private static bool IsDialogOpen { get; set; } = false;

            public static async Task<ContentDialogResult?> ShowDialog(ContentDialog dialog)
            {
                ContentDialogResult? result = null;

                if (!IsDialogOpen)
                {
                    IsDialogOpen = true;
                    result = await dialog.ShowAsync();
                    IsDialogOpen = false;
                }

                return result;
            }
        }

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

        internal class ErrorPopupContentDialog : ContentDialog
        {
            public ErrorPopupContentDialog(XamlRoot xamlRoot, Exception e)
            {
                Paragraph paragraph = new();
                paragraph.Inlines.Add(new Run() { Text = e.Message });

                Title = "Error occurred";
                CloseButtonText = "Close";
                Content = new ErrorPopupDialog(paragraph);
                XamlRoot = xamlRoot;
            }
        }
    }
}
