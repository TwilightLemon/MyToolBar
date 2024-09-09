using System.Windows;
using EleCho.WpfSuite.Controls;
namespace MyToolBar.Common.UIBases
{
    /// <summary>
    /// FluentWindowBase.xaml 的交互逻辑
    /// </summary>
    public partial class FluentWindowBase : Window
    {
        public FluentWindowBase()
        {
            InitializeComponent();
            CloseBtn = (Button)Template.FindName("CloseBtn", this);
            MaxmizeBtn = (Button)Template.FindName("MaxmizeBtn", this);
            MinimizeBtn = (Button)Template.FindName("MinimizeBtn", this);
            //Loaded += FluentWindowBase_Loaded;
        }
        private readonly Button CloseBtn,MaxmizeBtn, MinimizeBtn;
        private void FluentWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if(ResizeMode==ResizeMode.NoResize)
            {
                MaxmizeBtn.Visibility = Visibility.Collapsed;
                MinimizeBtn.Visibility = Visibility.Collapsed;
            }else if (ResizeMode == ResizeMode.CanMinimize)
            {
                MaxmizeBtn.IsEnabled = false;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaxmizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
