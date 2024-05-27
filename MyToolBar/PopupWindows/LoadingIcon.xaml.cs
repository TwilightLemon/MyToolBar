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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyToolBar.PopupWindows
{
    /// <summary>
    /// 一个简单的加载动画
    /// </summary>
    public partial class LoadingIcon : UserControl
    {
        public LoadingIcon()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ellipse.Stroke = new SolidColorBrush(SystemParameters.WindowGlassColor);
            while (true)
            {
                for (double x = 3; x <= 9; x += 0.02)
                {
                    ellipse.StrokeDashArray[0] = x;
                    await Task.Delay(10);
                }
                await Task.Delay(500);
                for (double x = 9; x >= 3; x -= 0.02)
                {
                    ellipse.StrokeDashArray[0] = x;
                    await Task.Delay(10);
                }
                await Task.Delay(500);
            }
        }
    }
}
