using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static MyToolBar.Common.GlobalService;
using System.Timers;
using MyToolBar.Common.WinAPI;
using Microsoft.Win32;
using MyToolBar.PenPackages;
using MyToolBar.PopupWindows;
using MyToolBar.Services;
using MyToolBar.ViewModels;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin;
using System.Windows.Documents;
using MyToolBar.Common.Func;
using System.Windows.Interop;
using System.Diagnostics;

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// [Main] AppBar Window
    /// </summary>
    public partial class AppBarWindow : Window
    {
        private OuterControlBase? _oc;
        private PenControlWindow? _pcw;
        private IntPtr _activeWindowHook;

        private readonly ThemeResourceService _themeResourceService;
        private readonly PluginReactiveService _pluginReactiveService;
        private readonly PowerOptimizeService _powerOptimizeService;

        public AppBarWindow(
            PluginReactiveService pluginReactiveService,
            ThemeResourceService themeResourceService,
            PowerOptimizeService powerOptimizeService,
            AppBarViewModel viewModel)
        {
            _pluginReactiveService = pluginReactiveService;
            _themeResourceService = themeResourceService;
            _powerOptimizeService = powerOptimizeService;

            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Window Style
            //标记WS_EX_TOOLWINDOW 窗口不在任务视图中显示
            ToolWindowAPI.SetToolWindow(this);
            Width = SystemParameters.WorkArea.Width;
            //初始化AppBar样式
            UpdateColorMode();
            ShowOuter(false);
            #endregion

            #region Timers
            GlobalTimer = new Timer();
            GlobalTimer.Interval = 1000;
            GlobalTimer.Elapsed += delegate {
                Dispatcher.Invoke(() => { UpdateBackground(); });
            };
            GlobalTimer.Start();
            #endregion

            #region Services
            //响应系统颜色模式变化
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            //响应节能模式变化
            _powerOptimizeService.OnEnergySaverStatusChanged += _powerOptimizeService_OnEnergySaverStatusChanged;
            //注册ActiveWindowHook
            _activeWindowHook = ActiveWindow.RegisterActiveWindowHook((hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime) =>
            {
                Dispatcher.Invoke(OnActiveWindowUpdated);
            });
            #endregion

            #region Pen Package
            //存在触摸设备时才启动PenControlWindow
            if (Tablet.TabletDevices.Count > 0)
            {
                _pcw = new PenControlWindow();
                _pcw.Owner = this;//防止Appbar将其覆盖
                _pcw.Show();
            }
            #endregion

            #region Load Plugin
            _pluginReactiveService.OuterControlChanged += _pluginReactiveService_OuterControlChanged;
            _pluginReactiveService.CapsuleRemoved += _pluginReactiveService_CapsuleRemoved;
            _pluginReactiveService.CapsuleAdded += _pluginReactiveService_CapsuleAdded;
            await _pluginReactiveService.Load();
            #endregion
        }

        private void _powerOptimizeService_OnEnergySaverStatusChanged(bool PowerModeOn)
        {
            IsPowerModeOn=PowerModeOn;
            //疑似这段会间歇性造成UI线程卡死
            Dispatcher.Invoke(() => {
                if (IsPowerModeOn)
                {
                    MainBarGrid.SetResourceReference(BackgroundProperty, "BackgroundColor");
                    _themeResourceService.SetAppBarFontColor(!IsDarkMode);
                }
                else MainBarGrid.Background = null;
            });
        }

        /// <summary>
        /// 响应OuterControl显示/隐藏动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Oc_IsShownChanged(object? sender, bool e) => ShowOuter(e);

        private void _pluginReactiveService_CapsuleRemoved(IPlugin obj)
        {
            if (_pluginReactiveService.Capsules[obj] is var cap){
                cap.Uninstall();
                CapsulePanel.Children.Remove(cap);
                cap = null;
            }
        }

        private void _pluginReactiveService_CapsuleAdded(CapsuleBase cap)
        {
            CapsulePanel.Children.Add(cap);
        }

        private void _pluginReactiveService_OuterControlChanged(OuterControlBase obj) {
            _oc?.Dispose();
            _oc = null;
            OuterFunc.Children.Clear();
            _oc = obj;
            _oc.IsShownChanged += Oc_IsShownChanged;
            OuterFunc.Children.Add(_oc);
        }

        #region OuterControl
        /// <summary>
        /// OuterFuncStatus是否开启
        /// </summary>
        static bool isOuterShow = true;

        public AppBarViewModel ViewModel { get; }

        /// <summary>
        /// 打开或关闭OuterFunc (Animation)
        /// </summary>
        /// <param name="show">open or close</param>
        private void ShowOuter(bool show = true)
        {
            Storyboard sb = new();
            if (OuterFunc.ActualWidth == 0)
                OuterFunc.Visibility = Visibility.Visible;
            double hWidth = OuterFunc.ActualWidth/2;
            DoubleAnimation da;
            ThicknessAnimation de;
            if (show && !isOuterShow)
            {
                de = new(new Thickness(hWidth, 0, hWidth, 0), new Thickness(0), TimeSpan.FromSeconds(0.5));
                da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                isOuterShow = true;
            }
            else if (!show && isOuterShow)
            {
                de = new(new Thickness(0), new Thickness(hWidth, 0, hWidth, 0), TimeSpan.FromSeconds(0.5));
                da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));
                isOuterShow = false;
            }
            else
                return;
            de.EasingFunction = da.EasingFunction = new QuarticEase();
            sb.Children.Add(da);
            sb.Children.Add(de);
            Storyboard.SetTarget(da, OuterFuncStatus);
            Storyboard.SetTarget(de, OuterFuncStatus);
            Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(de, new PropertyPath(MarginProperty));
            sb.Completed += OuterControlClosingAni;
            sb.Begin();
        }
        private void OuterControlClosingAni(object? sender, EventArgs e)
        {
            OuterFunc.Visibility = isOuterShow ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region OuterFuncStatus & WindowStyle
        private void AppBar_OnFullScreenStateChanged(object sender, bool e)
        {
            Visibility = e && EnableHideWhenFullScreen ? Visibility.Collapsed : Visibility.Visible;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ActiveWindow.UnregisterActiveWindowHook(_activeWindowHook);
        }

        private void MaxWindStyle()
        {
            //全屏样式  整体变暗
            CurrentAppBarStyle = 1;
            ViewModel.WindowAccentCompositorOpacity = 0.95f;
            if (IsDarkMode)
            {
                OuterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            }
            else
            {
                OuterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
            }
            _oc?.MaxStyleAct?.Invoke(true, null);
        }

        private void NormalWindStyle()
        {
            CurrentAppBarStyle = 0;
            ViewModel.WindowAccentCompositorOpacity = 0.6f;
            Brush? foreground = null;
            _themeResourceService.SetAppBarFontColor(!IsDarkMode);
            if (IsDarkMode)
            {
                OuterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
                foreground = OuterControlNormalDarkModeForeColor;
            }
            else
            {
                OuterFuncStatus.SetResourceReference(BackgroundProperty, "MaskColor");
            }

            _oc?.MaxStyleAct?.Invoke(false, foreground);
        }

        /// <summary>
        /// 更新ForeWindow->Tittle & 自身窗口样式
        /// </summary>
        private void OnActiveWindowUpdated()
        {
            TitleView.Text = ActiveWindow.GetActiveWindowTitle();
            Width = SystemParameters.WorkArea.Width;
        }
        /// <summary>
        /// 沉浸模式下更新AppBar背景
        /// </summary>
        private void UpdateBackground()
        {
            new MaxedWindowAPI((found) => {
                if (found)
                {
                    if (!IsPowerModeOn)
                        UpdateWindowColor();
                    if (CurrentAppBarStyle == 0)
                        MaxWindStyle();
                }
                else
                {
                    if (CurrentAppBarStyle == 1)
                    {
                        NormalWindStyle();
                        MainBarGrid.Background = null;
                    }
                }
            }).Find();

        }
        private Color? _lastEvaColor = null;
        private void UpdateWindowColor()
        {
            //实验性功能：根据下方窗口颜色调整AppBar颜色
            //屏幕截图
            using var source = new HwndSource(new HwndSourceParameters());
            var dpiX = source.CompositionTarget.TransformToDevice.M11;
            var dpiY = source.CompositionTarget.TransformToDevice.M22;
            var capHeight = 6;
            using System.Drawing.Bitmap bmp = ScreenAPI.CaptureScreenArea(0, (int)(ActualHeight*dpiX), (int)(ActualWidth*dpiY), capHeight);
            if (bmp == null) return;
            //取平均值
            long _r = 0, _g = 0, _b = 0;
            int total = 0;
            for (int x = 0; x < ActualWidth; x += 20)
            {
                for (int y = 0; y < capHeight; y += 2)
                {
                    System.Drawing.Color c = bmp.GetPixel(x, y);
                    _r += c.R;
                    _g += c.G;
                    _b += c.B;
                    total++;
                }
            }
            Color themeColor = Color.FromRgb((byte)(_r / total), (byte)(_g / total), (byte)(_b / total));
            //粗浅的判断颜色变化，有变化时才更新 (不好用)
            if (_lastEvaColor.HasValue && _lastEvaColor.Value.Equals(themeColor))
                return;
            _lastEvaColor = themeColor;
            //高斯模糊处理
            var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            bmp.GaussianBlur(ref rect,100);
            var imgBrush = BgImgEffector.Background = new ImageBrush(bmp.ToImageSource());
            BgImgEffector.Opacity = 0;
            BgImgEffector.Visibility = Visibility.Visible;
            var ani = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2));
            ani.Completed += delegate
            {
                MainBarGrid.Background = imgBrush;
                BgImgEffector.Visibility = Visibility.Collapsed;
            };
            BgImgEffector.BeginAnimation(OpacityProperty, ani);

            //判断颜色深浅
            var sel = themeColor.R * 0.299 + themeColor.G * 0.578 + themeColor.B * 0.114;
            Debug.WriteLine(sel);
            if (sel > 150)
            {
                //浅色
                _themeResourceService.SetAppBarFontColor(true);
            }
            else
            {
                //深色
                _themeResourceService.SetAppBarFontColor(false);
            }
            _oc?.MaxStyleAct?.Invoke(CurrentAppBarStyle==0, null);
            /*  纯色
            MainBarGrid.Background = new SolidColorBrush(themeColor);
            */
        }

        /// <summary>
        /// 自动根据系统颜色模式切换主题
        /// </summary>
        private void UpdateColorMode()
        {
            var isDarkMode = !ToolWindowAPI.GetIsLightTheme();
            _themeResourceService.SetThemeMode(isDarkMode);

            if (CurrentAppBarStyle == 0)
                NormalWindStyle();
            else
                MaxWindStyle();
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                UpdateColorMode();
            }
        }

        #endregion

        #region Left Part - Menu & Window Title

        bool _isMainMenuOpen = false;
        private void MainMenuButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isMainMenuOpen)
            {
                _isMainMenuOpen = true;
                var t = new MainTitleMenu();
                t.Closing += delegate { _isMainMenuOpen = false; };
                t.Left = 0;
                t.Show();
            }
        }

        private void TitleView_TouchDown(object sender, TouchEventArgs e)
        {
            //下滑显示任务视图
            SendHotKey.ShowTaskView();
        }
        #endregion
    }
}
