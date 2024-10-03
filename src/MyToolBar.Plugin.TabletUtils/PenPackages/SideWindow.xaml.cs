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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyToolBar.Plugin.TabletUtils.PenPackages
{
    /// <summary>
    /// SideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SideWindow : Window
    {
        public SideWindow()
        {
            InitializeComponent();
            Loaded += SideWindow_Loaded;
            Height = SystemParameters.WorkArea.Height-12;
            this.Deactivated += SideWindow_Deactivated;
        }

        private void SideWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation(0,20, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase();
            this.BeginAnimation(LeftProperty, da);
        }

        private void SideWindow_Deactivated(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(-ActualWidth, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase();
            da.Completed += delegate {
                Close();
            };
            this.BeginAnimation(LeftProperty, da);
        }
    }
}
