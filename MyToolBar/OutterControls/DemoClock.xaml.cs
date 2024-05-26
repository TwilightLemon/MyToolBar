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

namespace MyToolBar.OutterControls
{
    /// <summary>
    /// DemoClock.xaml 的交互逻辑
    /// </summary>
    public partial class DemoClock : OutterControlBase
    {
        private readonly string FormatStr = "MM-dd tt h:mm dddd",
                                            Sign="  ❤  Have a nice day.";
        public DemoClock()
        {
            InitializeComponent();
            Loaded += DemoClock_Loaded;
        }

        private void DemoClock_Loaded(object sender, RoutedEventArgs e)
        {
            IsShown = true;
            MaxStyleAct = maxStyleAct;
            GlobalService.GlobalTimer.Elapsed += RefreshTime;
        }
        private void maxStyleAct(bool max, Brush foreColor)
        {
            if (foreColor != null)
                TimeTb.Foreground = foreColor;
            else TimeTb.SetResourceReference(ForegroundProperty, "ForeColor");
        }
        private void RefreshTime(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TimeTb.Text = DateTime.Now.ToString(FormatStr) +Sign;
            });
        }
    }
}
