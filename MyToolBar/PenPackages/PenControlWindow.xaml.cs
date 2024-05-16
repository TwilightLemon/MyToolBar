using MyToolBar.WinApi;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyToolBar.PenPackages
{
    /// <summary>
    /// PenControlWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PenControlWindow : Window
    {
        public PenControlWindow()
        {
            InitializeComponent();
            this.SizeChanged += PenControlWindow_SizeChanged;
            this.Loaded += PenControlWindow_Loaded;
        }

        private void PenControlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResetWindowLocation();
            ToolWindowApi.SetToolWindow(this);
        }

        private void PenControlWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetWindowLocation();
        }

        private void OpenPanel()
        {
            this.Width = 200;
            this.Height = 200;
            FunctionPanel.Visibility = Visibility.Visible;
        }
        private void ClosePanel()
        {
            Width = 40;
            Height = 40;
            FunctionPanel.Visibility = Visibility.Collapsed;
        }
        private void ResetWindowLocation()
        {

           this.Left = SystemParameters.WorkArea.Width - ActualHeight;
            this.Top = 0;
        }

        private void statusBtn_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            ClosePanel();
        }

        private void Window_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            OpenPanel();
        }

        private void PrtScrBtn_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            ClosePanel();
        }

        private void DrawBtn_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            ClosePanel();
            new DrawboardWindow().Show();
        }
    }
}
