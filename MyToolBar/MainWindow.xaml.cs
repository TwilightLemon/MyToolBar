using System;
using System.Threading.Tasks;
using System.Windows;
using MyToolBar.WinApi;
using System.Windows.Input;
using System.Windows.Media;
using static MyToolBar.WinApi.WindowBlur;
using MyToolBar.Func;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;

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
        private WindowAccentCompositor wac;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 窗口样式
            ToolWindowApi.SetToolWindow(this);
            AppBarFunctions.SetAppBar(this, ABEdge.Top);
            wac = new(this, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            UpdataWindowBlurMode();
            Width = SystemParameters.WorkArea.Width;
            #endregion

            ms.Start();
            ms.MsgReceived += Ms_MsgReceived;

            CPUInfo.Load();
            Tick();
        }

        private void Ms_MsgReceived(string str)
        {
            Dispatcher.Invoke(async () => {
                var obj = JObject.Parse(str);
                if (str.Contains("LemonAppLyricData"))
                {
                    if (str.Contains("Handle"))
                        MsgHelper.ConnectedWindowHandle = int.Parse(obj["Handle"].ToString());
                    string data = obj["Data"].ToString();
                    OutterFuncText.Text = data;
                }
                else if (str.Contains("LemonAppOrd"))
                {
                    string data = obj["Data"].ToString();
                    if (data == "Start")
                    {
                        if (str.Contains("Handle"))
                            MsgHelper.ConnectedWindowHandle = int.Parse(obj["Handle"].ToString());
                    }else if(data=="Exit")
                    {
                        MsgHelper.ConnectedWindowHandle = 0;
                    }
                    OutterFuncText.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.3)));
                    OutterFuncText.Text = data switch
                    {
                        "Exit" => "LemonApp Exited.",
                        "Start" => "LemonApp Connected."
                    };
                    await Task.Delay(1000);
                    OutterFuncText.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, TimeSpan.FromSeconds(0.3)));
                    await Task.Delay(400);
                    OutterFuncText.Text = "";
                    OutterFuncText.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0)));
                }
            });
        }

        MsgHelper ms=new MsgHelper();
        NetworkInfo ni = new NetworkInfo();
        private IntPtr MaxedWindow = IntPtr.Zero;
        private async void Tick() {
            TitleView.Text=ActiveWindow.GetActiveWindowTitle();
            Meo_text.Text = (int)MemoryInfo.GetUsedPercent() + "%";
            Cpu_text.Text = CPUInfo.GetCPUUsedPercent();
            Cpu_temp.Text = CPUInfo.GetCPUTemperature()+"℃";
            var data = ni.GetNetworkspeed();
            Network_text.Text = $"↑ {data[1]}/s\r\n↓ {data[0]}/s";

            Width=SystemParameters.WorkArea.Width;
            var fore= ActiveWindow.GetForegroundWindow();
            if (MaxedWindow == IntPtr.Zero&&fore.IsZoomedWindow())
            {
                MaxedWindow = fore;
                UpdataWindowBlurMode(240);
            }
            if(!fore.IsZoomedWindow()&&MaxedWindow!=IntPtr.Zero)
            {
                MaxedWindow = IntPtr.Zero;
                UpdataWindowBlurMode();
            }

            await Task.Delay(1200);
            Tick();
        }
        public void UpdataWindowBlurMode(byte opacity = 150)
        {
            wac.Color = DarkMode ?
            Color.FromArgb(opacity, 0, 0, 0) :
            Color.FromArgb(opacity, 255, 255, 255);
            wac.DarkMode = DarkMode;
            wac.IsEnabled = true;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Func_Left_TouchDown(object sender, TouchEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_LAST, MsgHelper.ConnectedWindowHandle);
        }

        private void Func_Center_TouchDown(object sender, TouchEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_PAUSE, MsgHelper.ConnectedWindowHandle);
        }

        private void Func_Right_TouchDown(object sender, TouchEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_NEXT, MsgHelper.ConnectedWindowHandle);
        }

        private void TitleView_TouchDown(object sender, TouchEventArgs e)
        {
            SendHotKey.ShowTaskView();
        }
    }
}
