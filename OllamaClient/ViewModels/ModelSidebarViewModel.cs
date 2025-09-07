using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using OllamaClient.Views.Windows;
using System;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ModelSidebarViewModel
    {
        private IModelCollection _ModelCollection { get; set; }
        private IDialogsService _DialogsService { get; set; }
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }
        private CreateModelWindow? _CreateModelWindow { get; set; }

        public ObservableCollection<ModelViewModel> ModelViewModelCollection { get; set; } = [];

        public ModelSidebarViewModel(XamlRoot xamlRoot, DispatcherQueue dispatcherQueue)
        {
            _ModelCollection = App.GetRequiredService<IModelCollection>();
            _DialogsService = App.GetRequiredService<IDialogsService>();
            _XamlRoot = xamlRoot;
            _DispatcherQueue = dispatcherQueue;

            _ModelCollection.UnhandledException += ModelCollection_UnhandledException;
            _ModelCollection.ModelsLoaded += ModelCollection_ModelsLoaded;
        }

        public event EventHandler? ModelsLoaded;
        protected void OnModelsLoaded(EventArgs e) => ModelsLoaded?.Invoke(this, e);

        private void ModelCollection_ModelsLoaded(object? sender, EventArgs e)
        {
            ModelViewModelCollection.Clear();
            foreach (Model item in _ModelCollection.Items)
            {
                ModelViewModelCollection.Add(new(item, _ModelCollection, _XamlRoot, _DispatcherQueue, _DialogsService));
            }
            OnModelsLoaded(EventArgs.Empty);
        }

        private void ModelCollection_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(_XamlRoot, (Exception)e.ExceptionObject);

            _DispatcherQueue.TryEnqueue(async () => await _DialogsService.QueueDialog(dialog));
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

        public void ShowCreateModelWindow()
        {
            if(_CreateModelWindow is null)
            {
                _CreateModelWindow = new(this);
                _CreateModelWindow.Closed += _CreateModelWindow_Closed;
                _CreateModelWindow.Activate();
            }
        }

        private void _CreateModelWindow_Closed(object sender, WindowEventArgs e)
        {
            if (sender is CreateModelWindow window)
            {
                if(window.Reason == CreateModelWindow.ClosedReason.Created)
                {
                    if (window.Results.Name is not null && window.Results.From is not null)
                    {
                        _DispatcherQueue.TryEnqueue(async () =>
                        {
                            await _ModelCollection.CreateModel(window.Results.Name, window.Results.From.Name, window.Results.System, window.Results.Template, parameters: window.Results.Parameters);

                        });
                    }
                }

                window.Closed -= _CreateModelWindow_Closed;
                _CreateModelWindow = null;
            }
        }

        public void ShowPullModelDialog()
        {
            PullModelContentDialog dialog = new(_XamlRoot);

            dialog.Closed += PullModelDialog_Closed;

            _DispatcherQueue.TryEnqueue(async () => await _DialogsService.QueueDialog(dialog));
        }
    }
}

