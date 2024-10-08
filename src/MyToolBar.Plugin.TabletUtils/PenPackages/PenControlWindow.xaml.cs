﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MyToolBar.Common.WinAPI;
using System.Windows.Media.Animation;
using MyToolBar.Common;

namespace MyToolBar.Plugin.TabletUtils.PenPackages
{
    /// <summary>
    /// PenControlWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PenControlWindow : Window
    {
        public PenControlWindow()
        {
            InitializeComponent();
            Init();
        }
        ~PenControlWindow()
        {
            GlobalService.GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
        }
        private void Init()
        {
            this.SizeChanged += PenControlWindow_SizeChanged;
            this.Loaded += PenControlWindow_Loaded;
            this.StylusLeave += PenControlWindow_StylusLeave;
            this.Deactivated += PenControlWindow_Deactivated;
            GlobalService.GlobalTimer.Elapsed += GlobalTimer_Elapsed;


            _openAni = Resources["OpenAni"] as Storyboard;
            _openAni.Completed += delegate {
                this.IsEnabled = true;
            };
            _closeAni = Resources["CloseAni"] as Storyboard;
            _closeAni.Completed += delegate {
                Width = 40;
                Height = 40;
                this.IsEnabled = true;
                _isOpen = false;
            };
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                ResetWindowLocation();
            });
        }

        private void PenControlWindow_Deactivated(object? sender, System.EventArgs e)
        {
            ClosePanel();
        }

        private void PenControlWindow_StylusLeave(object sender, StylusEventArgs e)
        {
            if (!e.StylusDevice.InAir)
            {
                Point endPosition = e.GetPosition(this);
                if (endPosition.X < _startPosition.X && endPosition.Y > _startPosition.Y)
                {
                    //向左下滑动
                    OpenPanel();
                }
            }
        }

        private Storyboard _openAni,_closeAni;
        private bool _isOpen = false;
        private void PenControlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResetWindowLocation();
            WindowLongAPI.SetToolWindow(this);
        }

        private void PenControlWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetWindowLocation();
        }

        private void OpenPanel()
        {
            if (!_isOpen)
            {
                _isOpen = true;
                this.IsEnabled=false;//防止执行动画时误触按钮
                this.Width = 200;
                this.Height = 200;
                _openAni.Begin();
            }
        }
        private void ClosePanel()
        {
            if (_isOpen)
            {
                this.IsEnabled = false;//防止执行动画时误触按钮
                _closeAni.Begin();
            }
        }
        private void ResetWindowLocation()
        {

           this.Left = SystemParameters.WorkArea.Width - ActualHeight;
            this.Top = 0;
        }

        private void statusBtn_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            ClosePanel();
        }

        private Point _startPosition;
        private void Window_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            if(e.StylusDevice!=null)//防止mouse触发
                _startPosition = e.GetPosition(this);
        }

        private void PrtScrBtn_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            ClosePanel();
            //TODO: add sth
            SendHotKey.ScreenShot();
        }

        private void DrawBtn_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            ClosePanel();
            new DrawboardWindow().Show();
        }
    }
}
