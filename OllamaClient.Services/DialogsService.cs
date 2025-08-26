using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    public class DialogsService : IDialogsService
    {
        private readonly ILogger _Logger;
        private bool IsDialogOpen { get; set; } = false;
        private List<ContentDialog> QueuedDialogs = [];

        public DialogsService(ILogger<DialogsService> logger)
        {
            _Logger = logger;
        }

        private async void QueuedDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            int index = QueuedDialogs.IndexOf(sender);

            if (index == -1)
            {
                _Logger.LogError("Queued dialog {Title} not found in QueuedDialogs list", sender.Title);
            }
            else if (QueuedDialogs.ElementAtOrDefault(index + 1) is ContentDialog dialog)
            {
                await ShowDialog(dialog);
            }

            sender.Closed -= QueuedDialog_Closed;
            QueuedDialogs.Remove(sender);
            _Logger.LogDebug("Dialog {Title} removed from QueuedDialogs list", sender.Title);
        }

        public async Task QueueDialog(ContentDialog dialog)
        {
            dialog.Closed += QueuedDialog_Closed;
            QueuedDialogs.Add(dialog);
            _Logger.LogDebug("Queued content dialog {Title}", dialog.Title);
            if (!IsDialogOpen) await ShowDialog(dialog);
        }

        private async Task ShowDialog(ContentDialog dialog)
        {
            _Logger.LogDebug("Showing content dialog {Title}", dialog.Title);
            IsDialogOpen = true;
            await dialog.ShowAsync();
            IsDialogOpen = false;
            _Logger.LogDebug("Dialog {Title} closed", dialog.Title);
        }
    }
}
