using MyToolBar.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyToolBar.PenPackages
{
    /// <summary>
    /// DrawboardWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DrawboardWindow : Window
    {
        public DrawboardWindow()
        {
            InitializeComponent();
            this.Loaded += DrawboardWindow_Loaded;
        }

        private void DrawboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowApi.SetToolWindow(this);
            Topmost = true;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Left = 0;
            this.Top = 0;

            foreach(UIElement item in PenColors.Children)
            {
                item.MouseUp += PenColor_MouseUp;
            }

        }

        private void PenColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ink.DefaultDrawingAttributes.Color = ((SolidColorBrush)((Border)sender).Background).Color;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
