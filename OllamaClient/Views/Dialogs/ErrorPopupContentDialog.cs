using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;

namespace OllamaClient.Views.Dialogs
{
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
