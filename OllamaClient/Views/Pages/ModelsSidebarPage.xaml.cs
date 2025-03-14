using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System;
using System.Collections.ObjectModel;
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

    public class ModelParameterItem
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelsSidebarPage : Page
    {
        private Frame? ContentFrame { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelCollection ModelList { get; set; } = new();
        private ObservableCollection<ModelParameterItem> CreateModelParameters { get; set; } = [];

        public ModelsSidebarPage()
        {
            InitializeComponent();
            NewModelParametersItemsControl.ItemsSource = CreateModelParameters;
            ModelsListView.ItemsSource = ModelList.Items;

            CreateModelParameters.Add(new());
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

        private void CreateModelDialogButton_LostFocus(object sender, RoutedEventArgs e)
        {
            NewModelNameTextBox.Text = "";
            NewModelFromTextBox.Text = "";
            CreateModelParameters.Clear();
            CreateModelParameters.Add(new());
            NewModelSystemTextBox.Text = "";
            NewModelTemplateTextBox.Text = "";
        }

        private void CreateModelSendButton_Click(object sender, RoutedEventArgs e)
        {
            CreateModelDialogButton_LostFocus(sender, e);
        }

        private void ModelParameterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ComboBox comboBox && comboBox.DataContext is ModelParameterItem item)
            {
                item.Key = comboBox.SelectedItem?.ToString() ?? "";
            }
        }

        private void ModelParameterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(sender is TextBox textBox && textBox.DataContext is ModelParameterItem item)
            {
                item.Value = textBox.Text;
            }
        }

        private void AddModelParameterButton_Click(object sender, RoutedEventArgs e)
        {
            CreateModelParameters.Add(new());
        }
    }
}
