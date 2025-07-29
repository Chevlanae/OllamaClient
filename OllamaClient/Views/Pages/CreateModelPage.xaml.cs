using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using System;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateModelPage : Page
    {
        public class NavArgs(DispatcherQueue dispatcherQueue, ModelSidebarViewModel viewModel)
        {
            public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
            public ModelSidebarViewModel ModelSideBarViewModel { get; set; } = viewModel;
        }

        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ObservableCollection<ModelParameterViewModel> NewModelParameters { get; set; } = [];
        private ModelSidebarViewModel? ModelSideBarViewModel { get; set; }

        public CreateModelPage()
        {
            InitializeComponent();

            NewModelParametersItemsControl.ItemsSource = NewModelParameters;

            NewModelParameters.Add(new());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                ModelSideBarViewModel = args.ModelSideBarViewModel;
                DispatcherQueue = args.DispatcherQueue;
                FromComboBox.ItemsSource = ModelSideBarViewModel.Items;

                ModelSideBarViewModel.ModelCreated += ModelList_ModelCreated;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (ModelSideBarViewModel is not null)
            {
                ModelSideBarViewModel.ModelCreated -= ModelList_ModelCreated;
            }
        }

        private void ModelList_ModelCreated(object? sender, EventArgs e)
        {
            CreateModelClearButton_Click(this, new());
        }

        private void CreateModelClearButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelNameTextBox.Text = "";
            NewModelParameters.Clear();
            NewModelParameters.Add(new());
            NewModelSystemTextBox.Document.SetText(TextSetOptions.None, "");
            NewModelTemplateTextBox.Document.SetText(TextSetOptions.None, "");
        }

        private void CreateModelSendButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewModelNameTextBox.Text != "" && ModelSideBarViewModel is not null)
            {
                string name = NewModelNameTextBox.Text;
                string? from = (FromComboBox.SelectedItem as ModelViewModel)?.Source?.Name;
                string? system;
                string? template;

                NewModelSystemTextBox.Document.GetText(TextGetOptions.None, out system);
                NewModelTemplateTextBox.Document.GetText(TextGetOptions.None, out template);

                system = system?.Trim();
                template = template?.Trim();

                if (system == string.Empty) system = null;
                if (template == string.Empty) template = null;

                DispatcherQueue?.TryEnqueue(async () =>
                {
                    await ModelSideBarViewModel.CreateModel(name, from, system, template, null, NewModelParameters);
                });
            }
        }

        private void AddModelParameterButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelParameters.Add(new());

            NewModelParametersScrollViewer.ScrollToVerticalOffset(NewModelParametersScrollViewer.ScrollableHeight);
        }
    }
}
