using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using OllamaClient.Api;
using OllamaClient.Pages;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleSidbarButton_Click(object sender, RoutedEventArgs e)
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
        }

        private void ConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            if(SidebarFrame.CurrentSourcePageType != typeof(ConversationsSidebarPage))
            {
                SidebarFrame.Navigate(typeof(ConversationsSidebarPage), ContentFrame);
            }
            ToggleSidbarButton_Click(sender, e);
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

