using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models;
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

        public CreateModelPage()
        {
            InitializeComponent();

            NewModelParametersItemsControl.ItemsSource = NewModelParameters;

            NewModelParameters.Add(new());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CreateModelPageNavigationArgs args)
            {
                ModelList = args.ModelList;
                DispatcherQueue = args.DispatcherQueue;
                TemplateSelectorComboBox.ItemsSource = ModelList.Items;
            }
        }

        private void CreateModelClearButton_Click(object sender, RoutedEventArgs e)
        {
            NewModelNameTextBox.Text = "";
            NewModelFromTextBox.Text = "";
            NewModelParameters.Clear();
            NewModelParameters.Add(new());
            NewModelSystemTextBox.Document.SetText(TextSetOptions.None, "");
            NewModelTemplateTextBox.Document.SetText(TextSetOptions.None, "");
        }

        private void CreateModelSendButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewModelNameTextBox.Text is not "" && ModelList is not null)
            {
                string system;
                string template;
                NewModelSystemTextBox.Document.GetText(TextGetOptions.None, out system);
                NewModelTemplateTextBox.Document.GetText(TextGetOptions.None, out template);

                DispatcherQueue?.TryEnqueue(async () =>
                {
                    await ModelList.CreateModel(NewModelNameTextBox.Text, NewModelFromTextBox.Text, system, template, null, NewModelParameters);
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

        private void TemplateSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateSelectorComboBox.SelectedItem is ModelItem selectedModel && selectedModel.ModelFile is not null)
            {
                CreateModelCodeEditorControl.Editor.SetText(selectedModel.ModelFile.ToString());

                if(selectedModel.ParentModel is not null and not "")
                {
                    NewModelFromTextBox.Text = selectedModel.ParentModel;
                }
                else
                {
                    NewModelFromTextBox.Text = selectedModel.Model;
                }

                NewModelSystemTextBox.Document.SetText(TextSetOptions.None, selectedModel.ModelFile.System);
                NewModelTemplateTextBox.Document.SetText(TextSetOptions.None, selectedModel.ModelFile.Template);

                if(selectedModel.ModelFile.Parameters is not null)
                {
                    NewModelParameters.Clear();

                    foreach (ModelParameter parameter in selectedModel.ModelFile.Parameters)
                    {
                        NewModelParameters.Add(new() { Key = parameter.Key, Value = parameter.Value});
                    }
                }
                else
                {
                    NewModelParameters.Clear();
                    NewModelParameters.Add(new());
                }
            }
        }
    }
}
