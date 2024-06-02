using System;
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

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AppBarWindow : Window
    {
        private OuterControlBase? oc;
        private PenControlWindow pcw;

        private readonly ThemeResourceService _themeResourceService;
        private readonly ManagedPackageService _mPackageService;
        private readonly PluginReactiveService _pluginReactiveService;

        public AppBarWindow(
            PluginReactiveService pluginReactiveService,
            ManagedPackageService mPackageService,
            ThemeResourceService themeResourceService,
            AppBarViewModel viewModel)
        {
            _mPackageService = mPackageService;
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
            AppBarFunctions.SetAppBar(this, ABEdge.Top);
            Width = SystemParameters.WorkArea.Width;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            UpdateColorMode();
            ShowOutter(false);
            #endregion

            #region GlobalTimer
            GlobalTimer = new Timer();
            GlobalTimer.Interval = 1200;
            GlobalTimer.Elapsed += (o, e) => Dispatcher.Invoke(Tick);
            GlobalTimer.Start();
            #endregion


            pcw = new PenControlWindow();
            pcw.Show();

            _pluginReactiveService.OuterControlChanged += _pluginReactiveService_OuterControlChanged;
            _pluginReactiveService.CapsuleRemoved += _pluginReactiveService_CapsuleRemoved;
            _pluginReactiveService.CapsuleAdded += _pluginReactiveService_CapsuleAdded;
            await _pluginReactiveService.Load();
        }

        private void Oc_IsShownChanged(object? sender, bool e) => ShowOutter(e);

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
            OutterFunc.Children.Clear();
            oc = obj;
            oc.IsShownChanged += Oc_IsShownChanged;
            OutterFunc.Children.Add(oc);
        }

        #region OutterControl
        /// <summary>
        /// 最后一个最大化窗口 用于判断是否全屏 以改变OutterFunc样式
        /// </summary>
        private IntPtr MaxedWindow = IntPtr.Zero;
        /// <summary>
        /// OutterFuncStatus是否开启
        /// </summary>
        static bool isOutterShow = true;

        public AppBarViewModel ViewModel { get; }

        /// <summary>
        /// 打开或关闭OutterFunc (Animation)
        /// </summary>
        /// <param name="show">open or close</param>
        private void ShowOutter(bool show = true)
        {
            Storyboard sb = new();
            if (OutterFunc.ActualWidth == 0)
                OutterFunc.Visibility = Visibility.Visible;
            double width = OutterFunc.ActualWidth;
            DoubleAnimation da, de;
            if (show && !isOutterShow)
            {
                de = new DoubleAnimation(0, width, TimeSpan.FromSeconds(0.5));
                da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                isOutterShow = true;
            }
            else if (!show && isOutterShow)
            {
                de = new DoubleAnimation(width, 0, TimeSpan.FromSeconds(0.5));
                da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));
                isOutterShow = false;
            }
            else
                return;
            de.EasingFunction = da.EasingFunction = new QuarticEase();
            sb.Children.Add(da);
            sb.Children.Add(de);
            Storyboard.SetTarget(da, OutterFuncStatus);
            Storyboard.SetTarget(de, OutterFuncStatus);
            Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
            Storyboard.SetTargetProperty(de, new PropertyPath(WidthProperty));
            sb.Completed += OutterControlClosingAni;
            sb.Begin();
        }
        private void OutterControlClosingAni(object? sender, EventArgs e)
        {
            OutterFunc.Visibility = isOutterShow ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region OutterFuncStatus & WindowStyle

        private void MaxWindStyle()
        {
            //全屏样式  整体变暗
            CurrentWindowStyle = 1;
            ViewModel.WindowAccentCompositorOpacity = 0.95f;



            SolidColorBrush fore;
            if (IsDarkMode)
            {
                OutterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                fore = new SolidColorBrush(Color.FromArgb(240, 252, 252, 252));
            }
            else
            {
                OutterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
                fore = new SolidColorBrush(Color.FromArgb(240, 3, 3, 3));
            }
            oc?.MaxStyleAct?.Invoke(true, fore);
        }

        private void NormalWindStyle()
        {
            CurrentWindowStyle = 0;
            ViewModel.WindowAccentCompositorOpacity = 0.6f;
            Brush? foreground = null;
            if (IsDarkMode)
            {
                OutterFuncStatus.Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
                foreground = OuterControlNormalDarkModeForeColor;
            }
            else
            {
                OutterFuncStatus.SetResourceReference(BackgroundProperty, "MaskColor");
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

            var fore = ActiveWindow.GetForegroundWindow();
            if (MaxedWindow == IntPtr.Zero && fore.IsZoomedWindow())
            {
                MaxedWindow = fore;
                MaxWindStyle();
            }
            if (!MaxedWindow.IsZoomedWindow() && MaxedWindow != IntPtr.Zero)
            {
                //退出全屏 高亮
                MaxedWindow = IntPtr.Zero;
                NormalWindStyle();
            }
        }


        private void UpdateColorMode()
        {
            var isDarkMode = !ToolWindowApi.GetIsLightTheme();
            _themeResourceService.SetThemeMode(isDarkMode);

            if (CurrentWindowStyle == 0)
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

        #region Click & Touch Control
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var t = new MainTitleMeum();
            t.Left = 0;
            t.Show();
        }

        private void TitleView_TouchDown(object sender, TouchEventArgs e)
        {
            SendHotKey.ShowTaskView();
        }
        #endregion
    }
}
