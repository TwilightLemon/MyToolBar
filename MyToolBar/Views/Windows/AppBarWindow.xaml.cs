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
using MyToolBar.Common.Behaviors;
using System.Windows.Threading;
using System.Threading.Tasks;

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

        private readonly UIResourceService _themeResourceService;
        private readonly PluginReactiveService _pluginReactiveService;
        private readonly PowerOptimizeService _powerOptimizeService;
        private readonly AppSettingsService _appSettingsService;

        public AppBarWindow(
            PluginReactiveService pluginReactiveService,
            UIResourceService themeResourceService,
            PowerOptimizeService powerOptimizeService,
            AppSettingsService appSettingsService)
        {
            _pluginReactiveService = pluginReactiveService;
            _themeResourceService = themeResourceService;
            _powerOptimizeService = powerOptimizeService;
            _appSettingsService = appSettingsService;

            InitializeComponent();
        }

        #region Window Init & Service Events
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //标记WS_EX_TOOLWINDOW 窗口不在任务视图中显示
            WindowLongAPI.SetToolWindow(this);
            Width = SystemParameters.WorkArea.Width;
            //初始化AppBar背景样式
            UpdateBackground();
            UpdateColorMode();
            OnSystemColorChanged();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region GlobalTimer Init&Start
            GlobalTimer = new Timer();
            GlobalTimer.Interval = 1000;
            GlobalTimer.Elapsed += delegate {
                Dispatcher.Invoke(TimerTask);
            };
            GlobalTimer.Start();
            #endregion

            #region Services event register
            //响应系统颜色模式变化
            SystemThemeAPI.RegesterOnThemeChanged(this, OnSystemThemeChanged, OnSystemColorChanged);
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
            _pluginReactiveService.OuterControlRemoved += _pluginReactiveService_OuterControlRemoved;
            _pluginReactiveService.CapsuleRemoved += _pluginReactiveService_CapsuleRemoved;
            _pluginReactiveService.CapsuleAdded += _pluginReactiveService_CapsuleAdded;
            await _pluginReactiveService.Load();
            #endregion
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ActiveWindow.UnregisterActiveWindowHook(_activeWindowHook);
        }

        /// <summary>
        /// 节能模式优化
        /// </summary>
        /// <param name="PowerModeOn"></param>
        private void _powerOptimizeService_OnEnergySaverStatusChanged(bool PowerModeOn)
        {
            IsEnergySaverModeOn=PowerModeOn;
            Dispatcher.Invoke(UpdateEnergySaverMode);
        }
        /// <summary>
        /// 更新于节能模式的AppBar样式
        /// </summary>
        private void UpdateEnergySaverMode()
        {
            if (IsEnergySaverModeOn)
            {
                MainBarGrid.SetResourceReference(BackgroundProperty, "BackgroundColor");
                _themeResourceService.SetAppBarFontColor(!IsDarkMode);
            }
            else MainBarGrid.Background = null;
        }

        /// <summary>
        /// 响应OuterControl显示/隐藏动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Oc_IsShownChanged(object? sender, bool e) => ShowOuter(e);

        /// <summary>
        /// 在全屏时隐藏AppBar 退出全屏时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBar_OnFullScreenStateChanged(object sender, bool e)
        {
            Visibility = e && EnableHideWhenFullScreen ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 响应Capsule组件移除
        /// </summary>
        /// <param name="obj"></param>
        private void _pluginReactiveService_CapsuleRemoved(IPlugin obj)
        {
            if (_pluginReactiveService.Capsules[obj] is var cap){
                cap.Uninstall();
                CapsulePanel.Children.Remove(cap);
                cap = null;
            }
        }
        /// <summary>
        /// 响应Capsule组件添加
        /// </summary>
        /// <param name="cap"></param>
        private void _pluginReactiveService_CapsuleAdded(CapsuleBase cap)
        {
            CapsulePanel.Children.Add(cap);
        }
        /// <summary>
        /// 响应OuterControl组件更换
        /// </summary>
        /// <param name="obj"></param>
        private async void _pluginReactiveService_OuterControlChanged(OuterControlBase obj) {
            if (_oc != null)
            {
                ShowOuter(false);
                _oc.Dispose();
                _oc = null;
                OuterFunc.Children.Clear();
                await Task.Delay(800);
            }
            OuterControlCol.Width = new GridLength(1.2, GridUnitType.Star);
            _oc = obj;
            _oc.IsShownChanged += Oc_IsShownChanged;
            OuterFunc.Children.Add(_oc);
        }
        /// <summary>
        /// OuterControl组件移除
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private async void _pluginReactiveService_OuterControlRemoved()
        {
            if(_oc != null)
            {
                ShowOuter(false);
                _oc.Dispose();
                _oc = null;
                OuterFunc.Children.Clear();
                await Task.Delay(1200);
                OuterControlCol.Width = new GridLength(0, GridUnitType.Star);
            }
        }
        #endregion

        #region OuterControl
        /// <summary>
        /// OuterFuncStatus是否开启
        /// </summary>
        static bool isOuterShow = false;

        /// <summary>
        /// 打开或关闭OuterFunc (Animation)
        /// </summary>
        /// <param name="show">open or close</param>
        private void ShowOuter(bool show = true)
        {
            OuterFuncStatus.BeginAnimation(MarginProperty, null);
            OuterFunc.BeginAnimation(OpacityProperty, null);

            Storyboard sb = new();
            ThicknessAnimation de;
            if (show && !isOuterShow)
            {
                //open
                OuterFuncStatus.Visibility = Visibility.Visible;
                double hWidth = OuterFuncStatus.ActualWidth / 2;

                de = new(new Thickness(hWidth, 0, hWidth, 0), new Thickness(0), TimeSpan.FromSeconds(0.4));
                de.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                isOuterShow = true;
            }
            else if (!show && isOuterShow)
            {
                //close
                double hWidth = OuterFuncStatus.ActualWidth / 2;
                de = new(new Thickness(0), new Thickness(hWidth, 0, hWidth, 0), TimeSpan.FromSeconds(0.5));
                de.EasingFunction = new ElasticEase() { Oscillations = 1, Springiness=5, EasingMode = EasingMode.EaseIn };
                isOuterShow = false;
                sb.Completed += OuterControlClosingAni;
            }
            else
                return;
            sb.Children.Add(de);
            Storyboard.SetTarget(de, OuterFuncStatus);
            Storyboard.SetTargetProperty(de, new PropertyPath(MarginProperty));
            sb.Begin();
        }
        private void OuterControlClosingAni(object? sender, EventArgs e)
        {
            OuterFuncStatus.Visibility = isOuterShow ? Visibility.Visible : Visibility.Hidden;
            OuterFuncStatus.BeginAnimation(MarginProperty, null);
            OuterFunc.BeginAnimation(OpacityProperty, null);
        }
        #endregion

        #region OuterFuncStatus & Window Style

        /// <summary>
        /// 存在窗口最大化时的AppBar样式
        /// </summary>
        private void MaxWindowStyle()
        {
            CurrentAppBarStyle = 1;
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

        /// <summary>
        /// 无最大化窗口时的AppBar样式
        /// </summary>
        private void NormalWindowStyle()
        {
            CurrentAppBarStyle = 0;
            Brush? foreground = null;
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
        /// 更新前台窗口标题
        /// </summary>
        private void OnActiveWindowUpdated()
        {
            TitleView.Text = ActiveWindow.GetActiveWindowTitle();
        }
        /// <summary>
        /// AppBar样式的周期刷新任务
        /// </summary>
        private void TimerTask()
        {
            UpdateBackground();
            Width = SystemParameters.WorkArea.Width;
        }
        private bool IsImmerseMode = false;
        /// <summary>
        /// 更新AppBar背景
        /// </summary>
        private void UpdateBackground()
        {
            new MaxedWindowAPI((found) => {
                if (found)
                {
                    //存在最大化窗口
                    if (!IsEnergySaverModeOn){
                        ImmerseMode_UpdateBackground();
                        IsImmerseMode = true;
                    }
                    else UpdateEnergySaverMode();

                    if (CurrentAppBarStyle == 0)
                        MaxWindowStyle();
                }
                else
                {
                    //不存在最大化窗口
                    bool immerse = false;
                    if (_appSettingsService.Settings.AlwaysUseImmerseMode && !IsEnergySaverModeOn) 
                    {
                        //始终启用沉浸模式
                        immerse = true;
                        ImmerseMode_UpdateBackground();
                    }
                    else if (IsEnergySaverModeOn)
                    {
                        UpdateEnergySaverMode();
                    }
                    if((IsImmerseMode && !immerse))
                    {
                        MainBarGrid.Background = null;
                        _lastEvaColor = null;
                        _themeResourceService.SetAppBarFontColor(!IsDarkMode);
                    }

                    if (CurrentAppBarStyle == 1)
                    {
                        NormalWindowStyle();
                    }
                    IsImmerseMode = immerse;
                }
            }).Find();

        }
        private Color? _lastEvaColor = null;

        /// <summary>
        /// 沉浸模式更新AppBar背景
        /// </summary>
        private void ImmerseMode_UpdateBackground()
        {
            //根据下方窗口调整AppBar背景
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

            /*  纯色
            MainBarGrid.Background = new SolidColorBrush(themeColor);
            */
        }

        /// <summary>
        /// 自动根据系统颜色模式切换主题
        /// </summary>
        private void UpdateColorMode()
        {
            var isDarkMode = !SystemThemeAPI.GetIsLightTheme();
            //更新窗口模糊特效颜色模式
            BlurWindowBehavior.SetDarkMode(isDarkMode);
            //更新主题
            _themeResourceService.SetThemeMode(isDarkMode);
            //更新AppBar颜色
            if (CurrentAppBarStyle == 0){
                NormalWindowStyle();
                if (!IsImmerseMode)
                    _themeResourceService.SetAppBarFontColor(!isDarkMode);
            }
            else
                MaxWindowStyle();
        }
        
        private DateTime _lastSystemEvent = DateTime.MinValue;
        private void OnSystemThemeChanged()
        {
            //防止频繁触发
            if ((DateTime.Now - _lastSystemEvent).TotalSeconds < 1)
                return;
            _lastSystemEvent = DateTime.Now;
            UpdateColorMode();
        }
        private void OnSystemColorChanged()
        {
            _themeResourceService.UpdateDwmColor();
        }

        #endregion

        #region Main Menu & TitleViewer

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
