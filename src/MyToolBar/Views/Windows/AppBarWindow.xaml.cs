using System;
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
using System.Linq;

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// [Main] AppBar Window
    /// </summary>
    public partial class AppBarWindow : Window,INotificationReceiver
    {
        private const int DEFAULT_APPBAR_WINDOW_HEIGHT = 32;
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
            _appSettingsService.Settings.OnWindowModeChanged += Settings_OnWindowModeChanged;
            _appSettingsService.Settings.OnBackgroundModeChanged += Settings_OnBackgroundModeChanged;
            _appSettingsService.Settings.OnImmerseModeChanged += Settings_OnImmerseModeChanged;
            _appSettingsService.Settings.OnEnableWindowControlChanged += Settings_OnEnableWindowControlChanged;
            _appSettingsService.Settings.OnTargetMonitorChanged += Settings_OnTargetMonitorChanged;
            _appSettingsService.Settings.OnAppBarHeightChanged += Settings_OnAppBarHeightChanged;
            _appSettingsService.Settings.OnFloatingMarginChanged += Settings_OnFloatingMarginChanged;
            _appSettingsService.Settings.OnEnableHighlightChanged += Settings_OnEnableHighlightChanged;

            InitializeComponent();

            // 从设置中读取初始 ReservedHeight（替代 XAML 中的静态值）
            var appBar = AppBarCreator.GetAppBar(this);
            if (appBar != null)
            {
                appBar.ReservedHeight = _appSettingsService.Settings.AppBarHeight;
            }

            //应用目标显示器设置（必须在AppBar定位之前执行，InitializeComponent 会覆盖 Left/Top）
            ApplyTargetMonitor();
        }

        private BlurWindowBehavior? _blurWindowBehavior;

        private void Settings_OnWindowModeChanged()
        {
            UpdateWindowMode();
        }

        private void Settings_OnBackgroundModeChanged()
        {
            UpdateBackgroundMode();
            // 背景模式变更后立即刷新背景
            UpdateBackground();
        }

        private void Settings_OnImmerseModeChanged()
        {
            // 沉浸模式变更后立即刷新背景
            UpdateBackground();
        }

        private void Settings_OnEnableIslandChanged()
        {
            UpdateEnableIsland();
        }

        private void Settings_OnMainMenuIconChanged()
        {
            UpdateMainMenuIcon();
        }
        private void Settings_OnEnableWindowControlChanged()
        {
            // 设置变更后立即刷新窗口控制面板状态
            UpdateWindowControlPanel();
        }

        private void Settings_OnFloatingMarginChanged()
        {
            // 边距变更时重新应用窗口位置
            AppBar_OnWindowLocationApplied();
        }

        private void Settings_OnAppBarHeightChanged()
        {
            // 运行时更新 ReservedHeight 并通过 SetAppBarPosition 重新注册
            var appBar = AppBarCreator.GetAppBar(this);
            if (appBar != null)
            {
                appBar.ReservedHeight = _appSettingsService.Settings.AppBarHeight;
                appBar.SetAppBarPosition(Size.Empty);
            }
        }

        private void Settings_OnEnableHighlightChanged()
        {
            UpdateHighlightEffect();
        }

        private void Settings_OnTargetMonitorChanged()
        {
            // 运行时切换目标显示器
            // 先注销 AppBar，移动窗口，再重新注册
            var appBar = AppBarCreator.GetAppBar(this);
            var loc = appBar?.Location ?? AppBarLocation.None;
            if (appBar != null && loc != AppBarLocation.None && loc != AppBarLocation.RegisterOnly)
            {
                appBar.Location = AppBarLocation.None; // 触发 DisableAppBar + RestoreWindowInfo
            }

            ApplyTargetMonitor();

            if (appBar != null && loc != AppBarLocation.None && loc != AppBarLocation.RegisterOnly)
            {
                appBar.Location = loc; // 重新注册到新显示器（SetAppBarPosition → AppBar_OnWindowLocationApplied 会设置正确的 Width/Left）
            }
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

        private void UpdateWindowMode()
        {
            AppBar_OnWindowLocationApplied();
            var isFloating = _appSettingsService.Settings.CurrentWindowMode == AppSettings.WindowMode.Floating;
            if (isFloating)
            {
                WindowOption.SetCorner(this, WindowCorner.RoundSmall);
            }
            else
            {
                WindowOption.SetCorner(this, WindowCorner.DoNotRound);
            }
        }

        /// <summary>
        /// 根据当前背景模式更新 BlurWindowBehavior 的 Mode 和窗口 Background
        /// </summary>
        private void UpdateBackgroundMode()
        {
            if (_blurWindowBehavior == null) return;

            // 清除可能残留的沉浸模式背景
            AppBarBackground.Background = null;
            _lastEvaColor = null;

            switch (_appSettingsService.Settings.CurrentBackgroundMode)
            {
                case AppSettings.BackgroundMode.Transparent:
                    _blurWindowBehavior.Mode = MaterialType.Transparent;
                    CurrentAppBarBgStyle = AppBarBgStyleType.Transparent;
                    break;

                case AppSettings.BackgroundMode.Acrylic:
                default:
                    _blurWindowBehavior.Mode = MaterialType.Acrylic;
                    CurrentAppBarBgStyle = AppBarBgStyleType.Acrylic;
                    break;
            }
        }

        #region Window Init & Service Events
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            //应用目标显示器设置（现在 _hwnd 有效，使用当前窗口 DPI 精确计算）
            ApplyTargetMonitor();
            //标记WS_EX_TOOLWINDOW 窗口不在任务视图中显示
            WindowLongAPI.SetToolWindow(this);
            WindowLongAPI.SetNoActivate(this);
            // 获取 BlurWindowBehavior 引用
            _blurWindowBehavior = Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(this)
                .OfType<BlurWindowBehavior>().FirstOrDefault();
            Width = ScreenAPI.GetScreenArea(_hwnd).Width;

            // 初始化防抖更新调度器（200ms 延迟，窗口事件停歇后触发刷新）
            _debounceUpdate = new DebounceDispatcher(() =>
            {
                UpdateBackground();
                UpdateWindowControlPanel();
            }, delayMs: 200);

            // 注册窗口事件 Hook：实时响应下方窗口变化
            _windowEventHook = WindowEventHook.Register((_, eventType, hwnd, _, _, _, _) =>
            {
                // 过滤掉 AppBar 自身的窗口事件
                if (hwnd == _hwnd) return;

                _debounceUpdate.Trigger();
            });

            //初始化AppBar样式
            UpdateBackgroundMode();
            UpdateBackground();
            UpdateColorMode();
            UpdateMainMenuIcon();
            UpdateWindowMode();
            UpdateEnableIsland();
            UpdateAppBarForeground();
            OnSystemColorChanged();
        }
        /// <summary>
        /// Hwnd for this window
        /// </summary>
        private IntPtr _hwnd;
        private IntPtr _activeWindowHook;
        private IntPtr _windowEventHook;
        private volatile IntPtr _activeObjHandle;

        // 防抖更新调度器：窗口事件触发后延迟 200ms 执行，避免高频重复刷新
        private DebounceDispatcher? _debounceUpdate;

        // 字体颜色缓存：避免相同颜色结果重复触发渐变动画
        private bool? _lastForegroundLeft, _lastForegroundCenter, _lastForegroundRight;
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
            //注册ActiveWindowHook（窗口标题变化 → 更新标题 & 触发背景刷新）
            _activeWindowHook = ActiveWindow.RegisterActiveWindowHook((hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime) =>
            {
                if (hwnd != 0&& _activeObjHandle!=hwnd)
                {
                    Debug.WriteLine("Active Hwnd: " + hwnd+"\t Title: "+hwnd.GetWindowTitle());
                    Dispatcher.Invoke(() =>
                    {
                        OnActiveWindowUpdated();
                        _debounceUpdate?.Trigger();
                    });
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
            // 初始检查窗口状态
            UpdateWindowControlPanel();
            await _pluginReactiveService.Load();
            #endregion
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (_appSettingsService.Settings.CurrentWindowMode == AppSettings.WindowMode.Floating)
            {
                WindowOption.SetCorner(this, WindowCorner.Round);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (_appSettingsService.Settings.CurrentWindowMode == AppSettings.WindowMode.Floating)
            {
                WindowOption.SetCorner(this, WindowCorner.RoundSmall);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ActiveWindow.UnregisterActiveWindowHook(_activeWindowHook);
            WindowEventHook.Unregister(_windowEventHook);
            _debounceUpdate?.Cancel();
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
                    AppBarBackground.SetResourceReference(BackgroundProperty, "BackgroundColor");
                    _themeResourceService.SetAppBarFontColor(!IsDarkMode);
                    CurrentAppBarBgStyle = AppBarBgStyleType.EnergySaving;
                }
                // 省电模式下强制关闭高光渲染
                UpdateHighlightEffect();
            }
            else if (CurrentAppBarBgStyle == AppBarBgStyleType.EnergySaving){
                AppBarBackground.Background = null;
                // 恢复用户选择的背景模式
                UpdateBackgroundMode();
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
        /// WindowControlPanel是否已显示
        /// </summary>
        private bool _isWindowControlShown = false;

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

        #region WindowControlPanel

        /// <summary>
        /// 根据前台窗口最大化状态与用户设置，显示或隐藏窗口控制面板
        /// </summary>
        private void UpdateWindowControlPanel()
        {
            bool enabled = _appSettingsService.Settings.EnableWindowControl;
            if (!enabled)
            {
                if (_isWindowControlShown)
                {
                    WindowControlPanel.ClearTarget();
                    ShowWindowControlPanel(false);
                }
                return;
            }

            var fg = ActiveWindow.GetForegroundWindow();

            // 检查前台窗口是否与 AppBarWindow 在同一个显示器上
            if (fg != IntPtr.Zero)
            {
                IntPtr fgMonitor = ScreenAPI.GetHmonitorForHwnd(fg);
                IntPtr appBarMonitor = ScreenAPI.GetHmonitorForHwnd(_hwnd);
                if (fgMonitor != appBarMonitor)
                {
                    // 前台窗口在其他显示器上，隐藏面板
                    if (_isWindowControlShown)
                    {
                        WindowControlPanel.ClearTarget();
                        ShowWindowControlPanel(false);
                    }
                    return;
                }
            }

            bool isMaximized = fg != IntPtr.Zero && fg.IsZoomedWindow();

            if (isMaximized && !_isWindowControlShown)
            {
                WindowControlPanel.SetTarget(fg, isMaximized);
                ShowWindowControlPanel(true);
            }
            else if (!isMaximized && _isWindowControlShown)
            {
                WindowControlPanel.ClearTarget();
                ShowWindowControlPanel(false);
            }
            else if (isMaximized && _isWindowControlShown)
            {
                // 已显示时刷新按钮图标（可能在最大化/还原之间切换）
                WindowControlPanel.SetTarget(fg, isMaximized);
            }
        }

        /// <summary>
        /// 窗口控制面板滑入/滑出动画
        /// </summary>
        /// <param name="show">true 显示，false 隐藏</param>
        private void ShowWindowControlPanel(bool show)
        {
            WindowControlPanel.BeginAnimation(System.Windows.Controls.UserControl.MarginProperty, null);

            Storyboard sb = new();
            ThicknessAnimation ta;

            if (show && !_isWindowControlShown)
            {
                WindowControlPanel.Visibility = Visibility.Visible;
                double panelWidth = WindowControlPanel.ActualWidth;
                if (panelWidth < 1) panelWidth = 96; // fallback if layout not yet done

                ta = new ThicknessAnimation(
                    new Thickness(0, 0, -panelWidth, 0),
                    new Thickness(0),
                    TimeSpan.FromSeconds(0.35))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _isWindowControlShown = true;
            }
            else if (!show && _isWindowControlShown)
            {
                double panelWidth = WindowControlPanel.ActualWidth;
                if (panelWidth < 1) panelWidth = 96;

                ta = new ThicknessAnimation(
                    new Thickness(0),
                    new Thickness(0, 0, -panelWidth, 0),
                    TimeSpan.FromSeconds(0.3))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                _isWindowControlShown = false;
                sb.Completed += delegate
                {
                    WindowControlPanel.Visibility = Visibility.Collapsed;
                    WindowControlPanel.BeginAnimation(System.Windows.Controls.UserControl.MarginProperty, null);
                };
            }
            else
                return;

            sb.Children.Add(ta);
            Storyboard.SetTarget(ta, WindowControlPanel);
            Storyboard.SetTargetProperty(ta, new PropertyPath("Margin"));
            sb.Begin();
        }

        #endregion

        #region Window Style

        /// <summary>
        /// 根据设置将窗口移动到目标显示器
        /// </summary>
        private void ApplyTargetMonitor()
        {
            var targetDevice = _appSettingsService.Settings.TargetMonitorDeviceName;
            if (string.IsNullOrEmpty(targetDevice))
                return; // 使用默认行为（最近的显示器）

            var monitors = MonitorAPI.EnumerateMonitors();
            var target = monitors.FirstOrDefault(m => m.DeviceName == targetDevice);
            if (target == null)
                return; // 目标显示器未找到，使用默认行为

            // 获取用于坐标转换的 DPI：
            // - 窗口已有 HWND：使用窗口当前所在显示器的 DPI
            // - 窗口尚未创建（构造阶段）：使用主显示器 DPI（WPF 新窗口以此 DPI 解释坐标）
            IntPtr currentHmonitor;
            if (_hwnd != IntPtr.Zero)
                currentHmonitor = ScreenAPI.GetHmonitorForHwnd(_hwnd);
            else
                currentHmonitor = ScreenAPI.GetPrimaryMonitor();
            (double currentDpiX, double currentDpiY) = ScreenAPI.GetDPI(currentHmonitor);

            // 将目标显示器的物理像素坐标转换为当前 DPI 环境下的 WPF 逻辑坐标
            // 这样 HWND 会移动到正确的物理位置，随后 WPF 检测到显示器切换并调整 DPI
            Left = target.WorkAreaBounds.left / currentDpiX;
            Top = target.WorkAreaBounds.top / currentDpiY;
        }

        private void AppBar_OnWindowLocationApplied()
        {
            var isFloating = _appSettingsService.Settings.CurrentWindowMode == AppSettings.WindowMode.Floating;
            // 相对于显示器的偏移量（嵌入=0，悬浮使用用户设置的边距）
            double horizontalMargin = isFloating ? _appSettingsService.Settings.FloatingMarginHorizontal : 0;
            double verticalMargin = isFloating ? _appSettingsService.Settings.FloatingMarginVertical : 0;
            //double appBarHeight = _appSettingsService.Settings.AppBarHeight;

            // 使用 AppBar 已计算好的停靠位置（DockedSize 由 SetAppBarPosition 以正确的 DPI 计算）
            var appBar = AppBarCreator.GetAppBar(this);
            double baseLeft = appBar?.DockedSize?.Left ?? 0;
            double baseTop = appBar?.DockedSize?.Top ?? 0;

            // 设置绝对 WPF 位置（AppBar 停靠原点 + 偏移）
            if (appBar?.DockedSize != null)
            {
                Left = baseLeft + horizontalMargin;
                Top = baseTop + verticalMargin;
            }

            // Width 公式使用显示器相对偏移量（小值：0 或 horizontalMargin），而非绝对坐标
            Width = ScreenAPI.GetScreenArea(_hwnd).Width - horizontalMargin * 2;
            Height = DEFAULT_APPBAR_WINDOW_HEIGHT;
        }

        /// <summary>
        /// 更新前台窗口标题
        /// </summary>
        private void OnActiveWindowUpdated()
        {
            TitleView.Text = ActiveWindow.GetActiveWindowTitle();
        }
        /// <summary>
        /// AppBar样式的周期刷新任务（作为事件驱动更新的兜底，处理滚动/内容变化等无 WinEvent 的场景）。
        /// 统一走防抖通道，避免与窗口事件触发的 UpdateBackground 产生竞争抖动。
        /// </summary>
        private void TimerTask()
        {
            _debounceUpdate?.Trigger();
        }
       
        enum AppBarBgStyleType { EnergySaving, ImmerseMode, Acrylic, Transparent };
        private AppBarBgStyleType CurrentAppBarBgStyle { get; set; }
        private void DisableImmerseMode()
        {
            //退出沉浸模式，通过渐变动画恢复用户选择的背景
            var currentBg = AppBarBackground.Background;
            AppBarBackground.Background = null;
            _lastEvaColor = null;

            // 先恢复 BlurWindowBehavior 模式，使透明/模糊效果在动画下方就绪
            UpdateBackgroundMode();

            if (currentBg != null)
            {
                // 用 BgImgEffector 做淡出动画，平滑过渡
                BgImgEffector.BeginAnimation(OpacityProperty, null);
                BgImgEffector.Background = currentBg;
                BgImgEffector.Opacity = 1;
                BgImgEffector.Visibility = Visibility.Visible;

                var ani = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
                ani.Completed += delegate
                {
                    BgImgEffector.Visibility = Visibility.Collapsed;
                };
                BgImgEffector.BeginAnimation(OpacityProperty, ani);
            }
        }

        /// <summary>
        /// 更新AppBar背景
        /// </summary>
        private void UpdateBackground()
        {
            // 节能模式优先
            if (IsEnergySaverModeOn)
            {
                UpdateEnergySaverMode();
                return;
            }

            var immerseMode = _appSettingsService.Settings.CurrentImmerseMode;
            switch (immerseMode)
            {
                case AppSettings.ImmerseMode.Off:
                    // 关闭沉浸模式：始终使用所选背景（透明/模糊）
                    if (CurrentAppBarBgStyle == AppBarBgStyleType.ImmerseMode)
                    {
                        DisableImmerseMode();
                    }
                    break;

                case AppSettings.ImmerseMode.Auto:
                    // 自动模式：有最大化窗口时启用沉浸，否则使用所选背景
                    new MaxedWindowAPI((found) => {
                        if (found)
                        {
                            // 存在最大化窗口（同一显示器） → 沉浸模式
                            ImmerseMode_UpdateBackground();
                            CurrentAppBarBgStyle = AppBarBgStyleType.ImmerseMode;
                        }
                        else if (CurrentAppBarBgStyle == AppBarBgStyleType.ImmerseMode)
                        {
                            // 最大化窗口消失 → 恢复用户选择的背景
                            DisableImmerseMode();
                        }
                    }, _hwnd).Find();
                    break;

                case AppSettings.ImmerseMode.Always:
                    // 始终开启沉浸模式
                    ImmerseMode_UpdateBackground();
                    CurrentAppBarBgStyle = AppBarBgStyleType.ImmerseMode;
                    break;
            }
            //无论什么时候都要更新字体颜色
            UpdateAppBarForeground();
            Debug.WriteLine("Background Update Triggered");
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
                AppBarBackground.Background = imgBrush;
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

            // 颜色结果与上次相同则跳过，避免重复触发渐变动画
            if (_lastForegroundLeft == left && _lastForegroundCenter == center && _lastForegroundRight == right)
                return;
            _lastForegroundLeft = left;
            _lastForegroundCenter = center;
            _lastForegroundRight = right;

            _themeResourceService.SetAppBarFontColor(left, center, right);
            UpdateHighlightEffect();
        }

        private static readonly Brush _highlightOpacityMask = new SolidColorBrush(Color.FromArgb(0x54, 0x00, 0x00, 0x00));

        /// <summary>
        /// 根据字体颜色、用户设置和省电模式更新三个区域的高光渲染
        /// 只有字体为亮色（即背景暗）的区域才启用高光
        /// </summary>
        private void UpdateHighlightEffect()
        {
            bool settingEnabled = _appSettingsService.Settings.EnableHighlight;
            bool energySaver = IsEnergySaverModeOn;

            // left: checkColor返回true表示深色文字（亮背景），false表示亮色文字（暗背景）
            // 高光只在亮色文字时启用
            bool enableLeft = settingEnabled && !energySaver && _lastForegroundLeft == false;
            bool enableCenter = settingEnabled && !energySaver && _lastForegroundCenter == false;
            bool enableRight = settingEnabled && !energySaver && _lastForegroundRight == false;

            SetHighlightState(HighlightBorderLeft, enableLeft);
            SetHighlightState(HighlightBorderCenter, enableCenter);
            SetHighlightState(HighlightBorderRight, enableRight);
        }

        private static void SetHighlightState(System.Windows.Controls.Border border, bool enable)
        {
            if (enable)
            {
                if (border.Effect == null)
                    border.Effect = new Shaders.Impl.HighlightEffect { HighlightIntensity = 2 };
                if (border.OpacityMask == null)
                    border.OpacityMask = _highlightOpacityMask;
            }
            else
            {
                border.Effect = null;
                border.OpacityMask = null;
            }
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
                t.Owner = this;
                t.Left = GlobalService.GetPopupWindowLeft(MainMenuButton, t);
                t.Show();
            }
        }
        #endregion
    }
}
