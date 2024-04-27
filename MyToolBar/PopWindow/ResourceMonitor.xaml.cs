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
using System.Windows.Shapes;

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// ResourceMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class ResourceMonitor : PopWindowBase
    {
        public ResourceMonitor()
        {
            InitializeComponent();
        }

        private void OpenTaskMonitorBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("taskmgr");
        }
    }
}
