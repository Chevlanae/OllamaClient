using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace OllamaClient.ViewModels
{
    public class ToolSidebarViewModel
    {
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }
        private Frame _ContentFrame { get; set; }
        private IDialogsService _DialogsService { get; set; }
        private IToolCollection _Tools { get; set; }

        public ObservableCollection<ToolViewModel> ToolViewModelCollection { get; set; } = new();

        public ToolSidebarViewModel(XamlRoot xamlRoot, DispatcherQueue dispatcherQueue, Frame contentFrame)
        {
            _XamlRoot = xamlRoot;
            _DispatcherQueue = dispatcherQueue;
            _ContentFrame = contentFrame;
            _DialogsService = App.GetRequiredService<IDialogsService>();
            _Tools = App.GetRequiredService<IToolCollection>();
        }

        public void ProcessJsFile(StorageFile file)
        {
            _Tools.ProcessJavascriptFile(file.Path);
        }

        public void Refresh()
        {
            ToolViewModelCollection.Clear();
            foreach (Tool tool in _Tools.Items)
            {
                ToolViewModelCollection.Add(new(tool));
            }
        }
    }
}
