using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.Views.Dialogs;
using System;
using System.Collections.ObjectModel;

namespace OllamaClient.ViewModels
{
    public class ModelSidebarViewModel
    {
        private IModelCollection _ModelCollection { get; set; }
        private IDialogsService _DialogsService { get; set; }
        private ListView _ModelsListView { get; set; }
        private XamlRoot _XamlRoot { get; set; }
        private DispatcherQueue _DispatcherQueue { get; set; }

        public ObservableCollection<ModelViewModel> ModelViewModelCollection { get; set; } = [];

        public ModelSidebarViewModel(ListView modelsListView, XamlRoot xamlRoot, DispatcherQueue dispatcherQueue)
        {
            _ModelCollection = App.GetRequiredService<IModelCollection>();
            _DialogsService = App.GetRequiredService<IDialogsService>();
            _ModelsListView = modelsListView;
            _XamlRoot = xamlRoot;
            _DispatcherQueue = dispatcherQueue;

            _ModelCollection.UnhandledException += ModelCollection_UnhandledException;
            _ModelCollection.ModelsLoaded += ModelCollection_ModelsLoaded;
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

            _DispatcherQueue.TryEnqueue(async () => await _DialogsService.QueueDialog(dialog));
        }

        private void CreateModelDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs e)
        {
            if (sender is CreateModelContentDialog dialog)
            {
                if (e.Result == ContentDialogResult.Primary && dialog.Content is CreateModelDialog content)
                {
                    if (content.Results.Name is not null && content.Results.From is not null)
                    {
                        _DispatcherQueue.TryEnqueue(async () =>
                        {
                            await _ModelCollection.CreateModel(content.Results.Name, content.Results.From.Name, content.Results.System, content.Results.Template, parameters: content.Results.Parameters);

                        });
                    }

                }

                dialog.Closed -= CreateModelDialog_Closed;
            }
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

        public void ShowCreateModelDialog()
        {
            CreateModelContentDialog dialog = new(_XamlRoot, this);

            dialog.Closed += CreateModelDialog_Closed;

            _DispatcherQueue.TryEnqueue(async () => await _DialogsService.QueueDialog(dialog));
        }

        public void ShowPullModelDialog()
        {
            PullModelContentDialog dialog = new(_XamlRoot);

            dialog.Closed += PullModelDialog_Closed;

            _DispatcherQueue.TryEnqueue(async () => await _DialogsService.QueueDialog(dialog));
        }
    }
}

