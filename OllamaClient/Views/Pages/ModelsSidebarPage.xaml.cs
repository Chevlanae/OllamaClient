using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models.Ollama;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ModelsSidebarPageNavigationArgs(Frame contentFrame, DispatcherQueue dispatcherQueue)
    {
        public Frame ContentFrame { get; set; } = contentFrame;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelsSidebarPage : Page
    {
        private Frame? ContentFrame { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelCollection ModelList { get; set; } = new();
        private ObservableCollection<ModelParameterItem> NewModelParameters { get; set; } = [];

        public ModelsSidebarPage()
        {
            InitializeComponent();
            NewModelParametersItemsControl.ItemsSource = NewModelParameters;
            ModelsListView.ItemsSource = ModelList.Items;

            ModelList.UnhandledException += ModelList_UnhandledException;

            NewModelParameters.Add(new());
        }

        private void ModelList_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            DispatcherQueue?.TryEnqueue(() => { new ErrorPopupWindow("An error occurred", e.ExceptionObject.ToString() ?? "").Activate(); });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is ModelsSidebarPageNavigationArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                DispatcherQueue.TryEnqueue(async () => { await ModelList.LoadModels(); });
            }
        }

        private void ModelsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ModelsListView.SelectedItem is ModelItem item && DispatcherQueue != null)
            {
                ContentFrame?.Navigate(typeof(ModelItemPage), new ModelItemPageNavigationArgs(DispatcherQueue, item));
            }
        }

        private void CreateModelDialogButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void AddModelParameterButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelParameters.Add(new());
        }

        private void CreateModelClearButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelNameTextBox.Text = "";
            NewModelFromTextBox.Text = "";
            NewModelParameters.Clear();
            NewModelParameters.Add(new());
            NewModelSystemTextBox.Text = "";
            NewModelTemplateTextBox.Text = "";
        }

        private void CreateModelSendButton_Click(object sender, RoutedEventArgs e)
        {
            if(NewModelNameTextBox.Text != "")
            {
                string name = NewModelNameTextBox.Text;
                string? from = null;
                string? system = null;
                string? template = null;

                if (NewModelFromTextBox.Text != "") from = NewModelFromTextBox.Text;
                if (NewModelSystemTextBox.Text != "") system = NewModelSystemTextBox.Text;
                if (NewModelTemplateTextBox.Text != "") template = NewModelTemplateTextBox.Text;

                DispatcherQueue?.TryEnqueue(async () =>
                {
                    await ModelList.CreateModel(name, from, system, template, NewModelParameters);
                    await ModelList.LoadModels();
                });

                CreateModelClearButton_Click(sender, e);
            }
        }
    }
}
