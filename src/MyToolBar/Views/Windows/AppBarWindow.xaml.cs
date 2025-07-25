﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static MyToolBar.Common.GlobalService;
using System.Timers;
using MyToolBar.Common.WinAPI;
using MyToolBar.Services;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin;
using MyToolBar.Common.Func;
using System.Windows.Interop;
using System.Diagnostics;
using MyToolBar.Common.Behaviors;
using System.Windows.Threading;
using System.Threading.Tasks;
using MyToolBar.Common;
using Microsoft.Extensions.DependencyInjection;
using EleCho.WpfSuite;

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// [Main] AppBar Window
    /// </summary>
    public partial class AppBarWindow : Window,INotificationReceiver
    {
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

            _appSettingsService.Settings.OnMainMenuIconChanged += Settings_OnMainMenuIconChanged;
            _appSettingsService.Settings.OnEnableIslandChanged += Settings_OnEnableIslandChanged;
            _appSettingsService.Settings.OnEnableNewStyleChanged += Settings_OnEnableNewStyleChanged;

            InitializeComponent();
        }

        private void Settings_OnEnableNewStyleChanged()
        {
            UpdateEnableNewStyle();
        }

        private void Settings_OnEnableIslandChanged()
        {
            UpdateEnableIsland();
        }

        private void Settings_OnMainMenuIconChanged()
        {
            UpdateMainMenuIcon();
        }
        private void UpdateEnableIsland()
        {
            Island.Visibility = _appSettingsService.Settings.EnableIsland ? Visibility.Visible : Visibility.Collapsed;
        }
        private void UpdateMainMenuIcon()
        {
            MainMenuIcon.Data = (Geometry)FindResource($"MenuIcon_{_appSettingsService.Settings.MainMenuIcon}");
            InvalidateVisual();
        }

        private void UpdateEnableNewStyle()
        {
            AppBar_OnWindowLocationApplied();
            if (_appSettingsService.Settings.EnableNewStyle)
            {
                WindowOption.SetCorner(this, WindowCorner.RoundSmall);
            }
            else
            {
                WindowOption.SetCorner(this, WindowCorner.DoNotRound);
            }
        }

        #region Window Init & Service Events
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            //标记WS_EX_TOOLWINDOW 窗口不在任务视图中显示
            WindowLongAPI.SetToolWindow(this);
            Width = SystemParameters.WorkArea.Width;
            //初始化AppBar样式
            UpdateBackground();
            UpdateColorMode();
            UpdateMainMenuIcon();
            UpdateEnableNewStyle();
            UpdateEnableIsland();
            UpdateAppBarForeground();
            OnSystemColorChanged();
        }
        /// <summary>
        /// Hwnd for this window
        /// </summary>
        private IntPtr _hwnd;
        private IntPtr _activeWindowHook;
        private volatile IntPtr _activeObjHandle;
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
                if (hwnd != 0&& _activeObjHandle!=hwnd)
                {
                    Debug.WriteLine("Active Hwnd: " + hwnd+"\t Title: "+hwnd.GetWindowTitle());
                    Dispatcher.Invoke(OnActiveWindowUpdated);
                    _activeObjHandle = hwnd;
                }
            });
            //注册NotificationReceiver
            NotificationManager.RegisterReceiver(this);
            #endregion

            #region Load Plugin
            _pluginReactiveService.OuterControlChanged += _pluginReactiveService_OuterControlChanged;
            _pluginReactiveService.OuterControlRemoved += _pluginReactiveService_OuterControlRemoved;
            _pluginReactiveService.CapsuleRemoved += _pluginReactiveService_CapsuleRemoved;
            _pluginReactiveService.CapsuleAdded += _pluginReactiveService_CapsuleAdded;
            await _pluginReactiveService.Load();
            #endregion
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (_appSettingsService.Settings.EnableNewStyle)
            {
                WindowOption.SetCorner(this, WindowCorner.Round);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (_appSettingsService.Settings.EnableNewStyle)
            {
                WindowOption.SetCorner(this, WindowCorner.RoundSmall);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ActiveWindow.UnregisterActiveWindowHook(_activeWindowHook);
        }

        /// <summary>
        /// 节能模式优化
        /// </summary>
        /// <param name="isOn"></param>
        private void _powerOptimizeService_OnEnergySaverStatusChanged(bool isOn)
        {
            SetEnergySaverMode(isOn);
            Dispatcher.Invoke(UpdateEnergySaverMode);
        }
        /// <summary>
        /// 更新于节能模式的AppBar样式
        /// </summary>
        private void UpdateEnergySaverMode()
        {
            if (IsEnergySaverModeOn)
            {
                if (CurrentAppBarBgStyle != AppBarBgStyleType.EnergySaving)
                {
                    MainBarGrid.SetResourceReference(BackgroundProperty, "BackgroundColor");
                    _themeResourceService.SetAppBarFontColor(!IsDarkMode);
                    CurrentAppBarBgStyle = AppBarBgStyleType.EnergySaving;
                }
            }
            else if (CurrentAppBarBgStyle == AppBarBgStyleType.EnergySaving){
                MainBarGrid.Background = null;
                CurrentAppBarBgStyle = AppBarBgStyleType.Acrylic;
            }
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
            //?从全屏中恢复时不再绘制内容，强制刷新一下...
/*            InvalidateVisual();  //没用？
            UpdateLayout();*/
            Width--;
            Width++;
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
                //先隐藏旧的OuterControl
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
        private  void _pluginReactiveService_OuterControlRemoved()
        {
            if(_oc != null)
            {
                ShowOuter(false);
                _oc.Dispose();
                _oc = null;
                OuterFunc.Children.Clear();
            }
        }
        #endregion

        #region Notification

        private void NotificationBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(_isNotificationShown)
            {
                Dispatcher.Invoke(_notificationCallback);
                _notificationCallback = null;
            }
        }

        private bool _isNotificationShown = false;
        private async Task ShowNotificationBox(Notification notification, Action? callback)
        {
            NotificationContent.Text = notification.Msg;
            _notificationCallback = callback;
            NotificationBox.Background= notification.Type switch
            {
                NotificationType.Msg => Brushes.Black,
                NotificationType.Warning => Brushes.HotPink,
                _ => Brushes.LightGreen
            };

            if (_isNotificationShown)
            {
                return;
            }
            _isNotificationShown = true;

            NotificationBox.BeginAnimation(WidthProperty, null);
            //Show Notification
            NotificationBox.Visibility = Visibility.Visible;
            double shownWidth = Math.Min(ActualWidth / 3,680);
            double minWidth = shownWidth / 2;
            {
                Storyboard sb = new();
                ThicknessAnimationUsingKeyFrames ta = new();
                ta.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 30), TimeSpan.FromMilliseconds(0)));
                ta.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(0), TimeSpan.FromMilliseconds(200)) {EasingFunction=new CubicEase()});
                Storyboard.SetTarget(ta, NotificationBox);
                Storyboard.SetTargetProperty(ta, new PropertyPath(MarginProperty));
                sb.Children.Add(ta);

                DoubleAnimationUsingKeyFrames da = new();
                da.KeyFrames.Add(new EasingDoubleKeyFrame(minWidth, TimeSpan.FromMilliseconds(100)));
                da.KeyFrames.Add(new EasingDoubleKeyFrame(shownWidth, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase() });
                Storyboard.SetTarget(da, NotificationBox);
                Storyboard.SetTargetProperty(da, new PropertyPath(WidthProperty));
                sb.Children.Add(da);
                sb.Begin();
            }

            //wait
            await Task.Delay(notification.TimeSpan switch { 
                NotificationTimeSpan.Short=>3000,
                NotificationTimeSpan.Long=>5000,
                _=> 3000
            });

            //Hide Notification
            {
                Storyboard sb = new();
                ThicknessAnimationUsingKeyFrames ta = new();
                ta.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(0), TimeSpan.FromMilliseconds(0)));
                ta.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(0), TimeSpan.FromMilliseconds(450)));
                ta.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(0,-30,0,30), TimeSpan.FromMilliseconds(700)) { EasingFunction = new CubicEase() });
                Storyboard.SetTarget(ta, NotificationBox);
                Storyboard.SetTargetProperty(ta, new PropertyPath(MarginProperty));
                sb.Children.Add(ta);

                DoubleAnimationUsingKeyFrames da = new();
                da.KeyFrames.Add(new EasingDoubleKeyFrame(shownWidth, TimeSpan.FromMilliseconds(0)));
                da.KeyFrames.Add(new EasingDoubleKeyFrame(minWidth, TimeSpan.FromMilliseconds(500)) { EasingFunction= new ElasticEase() { Oscillations = 1, Springiness = 5, EasingMode = EasingMode.EaseIn }});
                Storyboard.SetTarget(da, NotificationBox);
                Storyboard.SetTargetProperty(da, new PropertyPath(WidthProperty));
                sb.Children.Add(da);
                sb.Completed += delegate {
                    NotificationBox.Visibility = Visibility.Collapsed;
                    _isNotificationShown = false;
                    _notificationCallback = null;
                };
                sb.Begin();
            }
        }

        private Action? _notificationCallback;
        public void OnNotificationReceived(Notification notification,Action? callback)
        {
            Dispatcher.Invoke(async() =>
            {
                await ShowNotificationBox(notification,callback);
            });
        }
        #endregion

        #region OuterControl

        private OuterControlBase? _oc;
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
                OuterControlCol.Width = new GridLength(1.2, GridUnitType.Star);
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
            //OuterControl不显示时不再占用空间
            OuterControlCol.Width = new GridLength(0, GridUnitType.Star);
        }
        #endregion

        #region Window Style
        private void AppBar_OnWindowLocationApplied()
        {
            if (_appSettingsService.Settings.EnableNewStyle)
            {
                Top = 4; Left = 8;
            }
            else
            {
                Top = Left = 0;
            }
            Width = ScreenAPI.GetScreenArea(_hwnd).Width - Left * 2;
            Height = 32;
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
            Width = ScreenAPI.GetScreenArea(_hwnd).Width - Left * 2;
            Height = 32;
        }
       
        enum AppBarBgStyleType { EnergySaving,ImmerseMode, Acrylic };
        private AppBarBgStyleType CurrentAppBarBgStyle { get; set; }
        private void DisableImmerseMode()
        {
            //退出沉浸模式
            MainBarGrid.Background = null;
            _lastEvaColor = null;
            CurrentAppBarBgStyle = AppBarBgStyleType.Acrylic;
        }

        /// <summary>
        /// 更新AppBar背景
        /// </summary>
        private void UpdateBackground()
        {
            new MaxedWindowAPI((found) => {
                if (found)
                {
                    //存在最大化窗口
                    if (!IsEnergySaverModeOn)
                    {
                        if (_appSettingsService.Settings.UseImmerseMode)
                        {
                            ImmerseMode_UpdateBackground();
                            CurrentAppBarBgStyle = AppBarBgStyleType.ImmerseMode;
                        }
                        else if(CurrentAppBarBgStyle == AppBarBgStyleType.ImmerseMode)
                        {
                            DisableImmerseMode();
                        }
                        UpdateAppBarForeground();
                    }
                    else UpdateEnergySaverMode();
                }
                else
                {
                    //不存在最大化窗口
                    bool immerse = false;
                    if (_appSettingsService.Settings.AlwaysUseImmerseMode && !IsEnergySaverModeOn) 
                    {
                        //始终启用沉浸模式
                        immerse = true;
                        CurrentAppBarBgStyle = AppBarBgStyleType.ImmerseMode;
                        ImmerseMode_UpdateBackground();
                        UpdateAppBarForeground();
                    }
                    else
                    {
                        UpdateEnergySaverMode();
                    }

                    if(CurrentAppBarBgStyle==AppBarBgStyleType.ImmerseMode && !immerse)
                    {
                        DisableImmerseMode();
                    }
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
            (double dpiX, double dpiY) = ScreenAPI.GetDPI(ScreenAPI.GetHmonitorForHwnd(_hwnd));
            var capHeight = 6;
            using System.Drawing.Bitmap bmp = ScreenAPI.CaptureScreenArea((int)(Left*dpiX), (int)((ActualHeight+Top)*dpiY), (int)(ActualWidth*dpiX), capHeight);
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
        }

        /// <summary>
        /// 分块更新AppBar字体颜色
        /// </summary>
        private void UpdateAppBarForeground()
        {
            //截取AppBar所在位置的真实图像
            (double dpiX, double dpiY) = ScreenAPI.GetDPI(ScreenAPI.GetHmonitorForHwnd(_hwnd));
            var capHeight = 6;
            using System.Drawing.Bitmap bmp = ScreenAPI.CaptureScreenArea((int)(Left*dpiX), (int)(Top*dpiY), (int)(ActualWidth * dpiX), capHeight);
            if (bmp == null) return;
            //由OuterControlCol划分为三个部分，单独计算颜色值
            double outerWidth = OuterControlCol.ActualWidth * dpiX;
            double edgeWidth = (ActualWidth * dpiX - outerWidth) / 2.0d;
            
            bool checkColor(int begin,int end) // true: black text
            {
                if (begin >= end) return false;
                int r=0, g = 0, b = 0;
                int count = 0;
                for(int x=begin; x < end; x+=10)
                {
                    for(int y=0; y < capHeight; y+=2)
                    {
                        var  c = bmp.GetPixel(x, y);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        count++;
                    }
                }
                r/= count;g/= count;b /= count;
                //判断颜色深浅
                return ImageHelper.WSAGColorCheck(r, g, b);
            }
            bool left= checkColor(0, (int)edgeWidth);
            bool center=checkColor((int)edgeWidth, (int)(edgeWidth + outerWidth));
            bool right=checkColor((int)(edgeWidth + outerWidth), (int)(ActualWidth * dpiX));
            _themeResourceService.SetAppBarFontColor(left, center, right);
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
                var t=App.Host.Services.GetRequiredService<MainTitleMenu>();
                t.Closing += delegate { _isMainMenuOpen = false; };
                t.Left = 0;
                t.Show();
            }
        }
        #endregion
    }
}
