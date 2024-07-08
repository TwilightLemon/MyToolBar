﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static MyToolBar.Common.GlobalService;
using System.Timers;
using MyToolBar.Common.WinApi;
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

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// [Main] AppBar Window
    /// </summary>
    public partial class AppBarWindow : Window
    {
        private OuterControlBase? oc;
        private PenControlWindow pcw;

        private readonly ThemeResourceService _themeResourceService;
        private readonly PluginReactiveService _pluginReactiveService;

        public AppBarWindow(
            PluginReactiveService pluginReactiveService,
            ThemeResourceService themeResourceService,
            AppBarViewModel viewModel)
        {
            _pluginReactiveService = pluginReactiveService;
            _themeResourceService = themeResourceService;

            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Window Style
            ToolWindowApi.SetToolWindow(this);
            var abf=AppBarFunctions.SetAppBar(this, ABEdge.Top);
            abf.OnFullScreenChanged += Abf_OnFullScreenChanged;
            Width = SystemParameters.WorkArea.Width;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            UpdateColorMode();
            ShowOuter(false);
            #endregion

            #region GlobalTimer
            GlobalTimer = new Timer();
            GlobalTimer.Interval = 1200;
            GlobalTimer.Elapsed += (o, e) => Dispatcher.Invoke(Tick);
            GlobalTimer.Start();
            #endregion

            pcw = new PenControlWindow();
            pcw.Show();

            #region Load Plugin
            _pluginReactiveService.OuterControlChanged += _pluginReactiveService_OuterControlChanged;
            _pluginReactiveService.CapsuleRemoved += _pluginReactiveService_CapsuleRemoved;
            _pluginReactiveService.CapsuleAdded += _pluginReactiveService_CapsuleAdded;
            await _pluginReactiveService.Load();
            #endregion
        }

        private void Abf_OnFullScreenChanged(bool FullScreen)
        {
            Visibility= FullScreen && EnableHideWhenFullScreen ? Visibility.Collapsed : Visibility.Visible;
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
            oc?.Dispose();
            oc = null;
            OuterFunc.Children.Clear();
            oc = obj;
            oc.IsShownChanged += Oc_IsShownChanged;
            OuterFunc.Children.Add(oc);
        }

        #region OuterControl
        /// <summary>
        /// 最后一个最大化窗口 用于判断是否全屏 以改变OuterFunc样式
        /// </summary>
        private IntPtr MaxedWindow = IntPtr.Zero;
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
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
            oc?.MaxStyleAct?.Invoke(true, null);
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

            oc?.MaxStyleAct?.Invoke(false, foreground);
        }

        /// <summary>
        /// 更新ForeWindow->Tittle & 自身窗口样式
        /// </summary>
        private void Tick()
        {
            TitleView.Text = ActiveWindow.GetActiveWindowTitle();
            Width = SystemParameters.WorkArea.Width;
            new MaxedWindowAPI((found) => {
                if (found)
                {
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
            //取平均值   粗浅的判断颜色变化，有变化时才更新
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
            if (themeColor.R * 0.299 + themeColor.G * 0.578 + themeColor.B * 0.114 > 192)
            {
                //浅色
                _themeResourceService.SetAppBarFontColor(true);
            }
            else
            {
                //深色
                _themeResourceService.SetAppBarFontColor(false);
            }
            oc?.MaxStyleAct?.Invoke(CurrentAppBarStyle==0, null);
            /* 
            MainBarGrid.Background = new SolidColorBrush(themeColor);
            */
        }

        private void UpdateColorMode()
        {
            var isDarkMode = !ToolWindowApi.GetIsLightTheme();
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
