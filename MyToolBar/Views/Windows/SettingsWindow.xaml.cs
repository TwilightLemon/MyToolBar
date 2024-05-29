using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MyToolBar.ViewModels;
using System.Windows.Navigation;
using MyToolBar.Common;

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
        }

        private  void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private  void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedPage = ViewModel.SettingsPages.FirstOrDefault();

        }

        public SettingsViewModel ViewModel { get; }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WindowFrame.Navigate(ViewModel.CurrentPageContent);

            // Clear history
            if (WindowFrame.CanGoBack || WindowFrame.CanGoForward)
            {
                JournalEntry? history;

                do
                {
                    history = WindowFrame.RemoveBackEntry();
                }
                while (history is not null);
            }
        }
    }
}
