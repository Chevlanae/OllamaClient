using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ToolSidebarViewModel
    {
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }
        private IDialogsService _DialogsService { get; set; }

        public List<Models.Tool> Tools { get; set; } = new();
        public ObservableCollection<ToolViewModel> ToolViewModelCollection { get; set; } = new();

        public ToolSidebarViewModel(XamlRoot xamlRoot, DispatcherQueue dispatcherQueue)
        {
            _XamlRoot = xamlRoot;
            _DispatcherQueue = dispatcherQueue;
            _DialogsService = App.GetRequiredService<IDialogsService>();
        }

        public void ShowCreateNewToolDialog()
        {
            ContentDialog dialog = new CreateNewToolContentDialog(_XamlRoot);

            dialog.Closed += CreateNewToolContentDialog_Closed;

            _DispatcherQueue.TryEnqueue(() => _DialogsService.QueueDialog(dialog));
        }

        private void CreateNewToolContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (sender.Content is CreateNewToolDialog dialog && dialog.ToolType is string type)
            {
                Models.Tool tool = new(dialog.ToolName, type);

                Tools.Add(tool);
                Refresh();
            }
        }

        public void Refresh()
        {
            ToolViewModelCollection.Clear();
            foreach (Models.Tool tool in Tools)
            {
                ToolViewModelCollection.Add(new(tool));
            }
        }
    }
}
