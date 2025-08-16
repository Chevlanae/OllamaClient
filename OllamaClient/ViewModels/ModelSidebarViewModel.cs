using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using OllamaClient.Views.Pages;
using System;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ModelSidebarViewModel
    {
        private ModelCollection _ModelCollection { get; set; }
        private ListView _ModelsListView { get; set; }
        private Frame _ContentFrame { get; set; }
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }
        private IDialogsService _DialogsService { get; set; }

        public ObservableCollection<ModelViewModel> ModelViewModelCollection { get; set; } = [];

        public ModelSidebarViewModel(ModelCollection modelCollection, ListView modelsListView, Frame contentFrame, XamlRoot xamlRoot, DispatcherQueue dispatcherQueue, IDialogsService dialogsService)
        {
            _ModelCollection = modelCollection;
            _ModelsListView = modelsListView;
            _ContentFrame = contentFrame;
            _XamlRoot = xamlRoot;
            _DispatcherQueue = dispatcherQueue;
            _DialogsService = dialogsService;

            _ModelCollection.UnhandledException += ModelCollection_UnhandledException;
            _ModelCollection.ModelDeleted += ModelCollection_ModelDeleted;
            _ModelCollection.ModelsLoaded += ModelCollection_ModelsLoaded;

            _ModelsListView.SelectedIndex = -1;
        }

        private void ModelCollection_ModelsLoaded(object? sender, EventArgs e)
        {
            ModelViewModelCollection.Clear();
            foreach (Model item in _ModelCollection.Items)
            {
                ModelViewModelCollection.Add(new(item, _ModelCollection, _XamlRoot, _DispatcherQueue, _DialogsService));
            }
        }

        private void ModelCollection_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(_XamlRoot, (Exception)e.ExceptionObject);

            _DispatcherQueue.TryEnqueue(async () =>
            {
                await _DialogsService.QueueDialog(dialog);
            });
        }

        private void ModelCollection_ModelDeleted(object? sender, EventArgs e)
        {
            _ContentFrame.Navigate(typeof(BlankPage));
        }

        private void PullModelDialog_Closed(object sender, ContentDialogClosedEventArgs e)
        {
            if (sender is PullModelContentDialog dialog)
            {
                if (e.Result == ContentDialogResult.Primary)
                {
                    string? modelName = (dialog.Content as TextBoxDialog)?.InputText;

                    if (modelName is not null)
                    {
                        _DispatcherQueue.TryEnqueue(async () => await _ModelCollection.PullModel(modelName));
                    }
                }

                dialog.Closed -= PullModelDialog_Closed;
            }

        }

        public void Refresh()
        {
            _DispatcherQueue?.TryEnqueue(async () => 
            {
                await _ModelCollection.LoadModels();
            });
        }

        public void ShowPullModelDialog()
        {
            PullModelContentDialog dialog = new(_XamlRoot);

            dialog.Closed += PullModelDialog_Closed;

            _DialogsService.QueueDialog(dialog);
        }
    }
}

