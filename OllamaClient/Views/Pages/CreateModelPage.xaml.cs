using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models.Ollama;
using OllamaClient.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class CreateModelPageNavigationArgs(DispatcherQueue dispatcherQueue, ModelCollection modelCollection)
    {
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        public ModelCollection ModelList { get; set; } = modelCollection;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateModelPage : Page
    {
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ObservableCollection<ModelParameterItem> NewModelParameters { get; set; } = [];
        private ModelCollection? ModelList { get; set; }
        private ObservableCollection<string> TemplateSelectorOptions { get; set; } = [];

        public CreateModelPage()
        {
            InitializeComponent();

            NewModelParametersItemsControl.ItemsSource = NewModelParameters;
            TemplateSelectorComboBox.ItemsSource = TemplateSelectorOptions;

            NewModelParameters.Add(new());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CreateModelPageNavigationArgs args)
            {
                ModelList = args.ModelList;
                DispatcherQueue = args.DispatcherQueue;

                TemplateSelectorOptions.Clear();
                var existingItems = ModelList.Items.Select(m => m.Model);
                if (existingItems is not null)
                {
                    TemplateSelectorOptions.Add("None");

                    foreach (string item in existingItems)
                    {
                        TemplateSelectorOptions.Add(item);
                    }
                }
            }
        }

        private void CreateModelClearButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelNameTextBox.Text = "";
            NewModelFromTextBox.Text = "";
            NewModelParameters.Clear();
            NewModelParameters.Add(new());
            NewModelSystemTextBox.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");
            NewModelTemplateTextBox.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");
        }

        private void CreateModelSendButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewModelNameTextBox.Text is not "" && ModelList is not null)
            {
                string name = NewModelNameTextBox.Text;
                string? from = null;
                string? system = null;
                string? template = null;
                string? license = null;
                ModelParameters? parameters = null;

                if (NewModelFromTextBox.Text is not "") from = NewModelFromTextBox.Text;
                NewModelSystemTextBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out system);
                NewModelTemplateTextBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out template);
                if (NewModelParameters.Any((p) => { return p.Value is not ""; }))
                {
                    ModelParameters modelParameters = new();

                    foreach (ModelParameterItem item in NewModelParameters)
                    {
                        if (item.Value is not "")
                        {
                            ModelFile.AggregateParameter(item, modelParameters);
                        }
                    }

                    parameters = modelParameters;
                }

                DispatcherQueue?.TryEnqueue(async () =>
                {
                    await ModelList.CreateModel(name, from, system, template, license, parameters);
                    await ModelList.LoadModels();
                });

                CreateModelClearButton_Click(this, e);
            }
        }

        private void AddModelParameterButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelParameters.Add(new());

            NewModelParametersScrollViewer.ScrollToVerticalOffset(NewModelParametersScrollViewer.ScrollableHeight);
        }

        private void SwapFormButton_Click(object sender, RoutedEventArgs e)
        {
            if (CreateModelFormGrid.Visibility == Visibility.Visible)
            {
                CreateModelFormGrid.Visibility = Visibility.Collapsed;
                CreateModelTextEditorGrid.Visibility = Visibility.Visible;
            }
            else
            {
                CreateModelFormGrid.Visibility = Visibility.Visible;
                CreateModelTextEditorGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
