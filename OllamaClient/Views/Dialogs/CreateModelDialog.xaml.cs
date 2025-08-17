using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using OllamaClient.ViewModels;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateModelDialog : Page
    {
        public class DialogArgs
        {
            public ModelSidebarViewModel ViewModel { get; set; }
            public InputResults Results { get; set; }
        }

        public class InputResults
        {
            public string? Name { get; set; }
            public ModelViewModel? From { get; set; }
            public string? Template { get; set; }
            public ObservableCollection<ModelParameterViewModel>? Parameters { get; set; }
            public string? System { get; set; }
        }

        public InputResults Results { get; set; } = new();

        private DialogArgs Arguments { get; set; }
        private int _PreviousSelectedIndex { get; set; } = 0;

        public CreateModelDialog(ModelSidebarViewModel viewModel)
        {
            InitializeComponent();

            Arguments = new()
            {
                ViewModel = viewModel,
                Results = Results
            };

            NavigationSelectorBar.SelectedItem = NavigationSelectorBar.Items[0];
        }


        private void NavigationSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            var transitionEffect = currentSelectedIndex - _PreviousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
            var transitionInfo = new SlideNavigationTransitionInfo() { Effect = transitionEffect };
            _PreviousSelectedIndex = currentSelectedIndex;

            switch (currentSelectedIndex)
            {
                case 0:
                    ContentFrame.Navigate(typeof(FromContentPage), Arguments, transitionInfo);
                    break;
                case 1:
                    ContentFrame.Navigate(typeof(TemplateContentPage), Arguments, transitionInfo);
                    break;
                case 2:
                    ContentFrame.Navigate(typeof(ParametersContentPage), Arguments, transitionInfo);
                    break;
                case 3:
                    ContentFrame.Navigate(typeof(SystemContentPage), Arguments, transitionInfo);
                    break;
                default:
                    ContentFrame.Navigate(typeof(FromContentPage), Arguments, transitionInfo);
                    break;
            }
        }
    }
}
