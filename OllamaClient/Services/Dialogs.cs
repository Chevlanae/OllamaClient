using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    internal static class Dialogs
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
}
