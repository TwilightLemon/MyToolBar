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
using System.Timers;

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
        private Timer timer = new Timer();
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
            UpdateWindowBlurMode();
            Width = SystemParameters.WorkArea.Width;
            ShowOutter(false);
            #endregion

            #region OutterControl
            ms.Start();
            ms.MsgReceived += Ms_MsgReceived;
            #endregion

            CPUInfo.Load();
            timer.Interval = 1200;
            timer.Elapsed += (s, e) => Dispatcher.Invoke(Tick);
            timer.Start();
        }

        private void Ms_MsgReceived(string str)
        {
            Dispatcher.Invoke(async () => {
                var obj = JObject.Parse(str);
                if (str.Contains("LemonAppLyricData"))
                {
                    if (str.Contains("Handle"))
                        MsgHelper.ConnectedWindowHandle = int.Parse(obj["Handle"].ToString());
                    string data = obj["Data"].ToString()+" 🎵";
                    OutterFuncText.Text = data;
                    ShowOutter();
                }
                else if (str.Contains("LemonAppOrd"))
                {
                    string data = obj["Data"].ToString();
                    if (data == "Start")
                    {
                        ShowOutter();
                        if (str.Contains("Handle"))
                            MsgHelper.ConnectedWindowHandle = int.Parse(obj["Handle"].ToString());
                    }else if(data=="Exit")
                    {
                        MsgHelper.ConnectedWindowHandle = 0;
                        ShowOutter(false);
                    }
                }
            });
        }

        MsgHelper ms=new MsgHelper();
        NetworkInfo ni = new NetworkInfo();
        private IntPtr MaxedWindow = IntPtr.Zero;
        private void Tick() {
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
                //全屏样式  整体变暗
                MaxedWindow = fore;
                UpdateWindowBlurMode(240);
                OutterFuncStatus.Background=new SolidColorBrush(Color.FromArgb(20,255,255,255));
                OutterFuncText.Foreground=new SolidColorBrush(Color.FromArgb(240,252,252,252));
                OutterFuncText.FontWeight = FontWeights.Normal;
            }
            if(!MaxedWindow.IsZoomedWindow()&&MaxedWindow!=IntPtr.Zero)
            {
                //退出全屏 高亮
                MaxedWindow = IntPtr.Zero;
                UpdateWindowBlurMode();
                OutterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
                OutterFuncText.Foreground = new SolidColorBrush(Color.FromArgb(250, 3, 3, 3));
                OutterFuncText.FontWeight = FontWeights.Bold;
            }
        }

        static bool isOutterShow = true;
        private void ShowOutter(bool show=true)
        {
            Storyboard sb = new();
            if(OutterFunc.ActualWidth==0)
                OutterFunc.Visibility = Visibility.Visible;
            double width = OutterFunc.ActualWidth;
            DoubleAnimation da,de;
            if (show && !isOutterShow)
            {
                de =new DoubleAnimation(0, width, TimeSpan.FromSeconds(0.5));
                da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                isOutterShow = true;
            }else if(!show && isOutterShow)
            {
                de = new DoubleAnimation(width,0, TimeSpan.FromSeconds(0.5));
                da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));
                isOutterShow = false;
            }
            else return;
            de.EasingFunction = da.EasingFunction = new QuarticEase();
            sb.Children.Add(da);
            sb.Children.Add(de);
            Storyboard.SetTarget(da, OutterFuncStatus);
            Storyboard.SetTarget(de, OutterFuncStatus);
            Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(de, new PropertyPath(WidthProperty));
            sb.Completed += Sb_Completed;
            sb.Begin();
        }

        private void Sb_Completed(object? sender, EventArgs e)
        {
            OutterFunc.Visibility = isOutterShow ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateWindowBlurMode(byte opacity = 150)
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
