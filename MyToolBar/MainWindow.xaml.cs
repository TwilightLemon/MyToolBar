using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MyToolBar.WinApi;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MyToolBar.WinApi.WindowBlur;
using MyToolBar.Func;
using OpenHardwareMonitor.Hardware;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.Pipes;
using System.IO;

namespace MyToolBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        bool DarkMode = true;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 窗口样式
            ToolWindowApi.SetToolWindow(this);
            AppBarFunctions.SetAppBar(this, ABEdge.Top);
            WindowAccentCompositor wac = new(this, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = DarkMode ?
            Color.FromArgb(200, 0, 0, 0) :
            Color.FromArgb(200, 255, 255, 255);
            wac.IsEnabled = true;
            #endregion

            ms.Start();
            ms.MsgReceived += (str) => {
                Dispatcher.Invoke(() => {
                    OutterFuncText.Text = str;
                });
            };

            CPUInfo.Load();
            Tick();
        }
        MsgHelper ms=new MsgHelper();
        NetworkInfo ni = new NetworkInfo();
        private async void Tick() {
            TitleView.Text=ActiveWindow.GetActiveWindowTitle();
            Meo_text.Text = (int)MemoryInfo.GetUsedPercent() + "%";
            Cpu_text.Text = CPUInfo.GetCPUUsedPercent();
            Cpu_temp.Text = CPUInfo.GetCPUTemperature().ToString()+"℃";
            var data = ni.GetNetworkspeed();
            Network_text.Text = $"↑ {data[1]}/s\r\n↓ {data[0]}/s";
            await Task.Delay(500);
            Tick();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
