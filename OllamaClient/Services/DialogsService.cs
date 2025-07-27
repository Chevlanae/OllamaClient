using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    internal class DialogsService
    {
        private readonly ILogger _Logger;
        private bool IsDialogOpen { get; set; } = false;
        private List<ContentDialog> QueuedDialogs = [];

        public DialogsService(ILogger<DialogsService> logger)
        {
            _Logger = logger;
        }

        public async Task ShowDialog(ContentDialog dialog)
        {
            if (IsDialogOpen)
            {
                dialog.Closed += Dialog_Closed;
                QueuedDialogs.Add(dialog);
                _Logger.LogDebug("Queued content dialog {Title}", dialog.Title);
            }
            else
            {
                _Logger.LogDebug("Showing content dialog {Title}", dialog.Title);
                IsDialogOpen = true;
                await dialog.ShowAsync();
                IsDialogOpen = false;
                _Logger.LogDebug("Dialog {Title} closed", dialog.Title);
            }
        }

        private async void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            int index = QueuedDialogs.IndexOf(sender);

            if(index == -1)
            {
                _Logger.LogDebug("Queued dialog {Title} not found in QueuedDialogs list", sender.Title);
            }
            else if(QueuedDialogs.ElementAtOrDefault(index + 1) is ContentDialog dialog)
            {
                _Logger.LogDebug("Showing content dialog {Title}", dialog.Title);
                IsDialogOpen = true;
                await dialog.ShowAsync();
                IsDialogOpen = false;
                _Logger.LogDebug("Dialog {Title} closed", dialog.Title);
            }

            sender.Closed -= Dialog_Closed;
            QueuedDialogs.Remove(sender);
            _Logger.LogDebug("Dialog {Title} removed from QueuedDialogs list", sender.Title);
        }
    }
}
