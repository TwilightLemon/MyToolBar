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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// MeumItem.xaml 的交互逻辑
    /// </summary>
    public partial class MeumItem : UserControl
    {
        public MeumItem()
        {
            InitializeComponent();
            MouseEnter += delegate {
                ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 0.5, TimeSpan.FromMilliseconds(300)));
            };
            MouseLeave += delegate
            {
                ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 0, TimeSpan.FromMilliseconds(300)));
            };
        }
        public string MeumContent {
            get => ContentTb.Text; set=>ContentTb.Text = value;
        }
    
        public Geometry Icon
        {
            get => IconPt.Data;
            set => IconPt.Data = value;
        }
    }
}
