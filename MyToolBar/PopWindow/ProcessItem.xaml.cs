using MyToolBar.Func;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// ProcessItem.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessItem : UserControl
    {
        public ProcessItem()
        {
            InitializeComponent();
        }
        public Process _pro;
        public ProcessItem(Process p)
        {
            InitializeComponent();
            _pro = p;
            ProName.Text = p.ProcessName;
            InfoTb.Text = NetworkInfo.FormatSize(p.WorkingSet64);
            MouseEnter += delegate {
                ViewMask.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate {
                ViewMask.Visibility = Visibility.Hidden;
            };
        }
    }
}
