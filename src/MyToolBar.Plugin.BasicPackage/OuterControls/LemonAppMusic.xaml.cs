using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MyToolBar.Common;
using MyToolBar.Common.Func;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;

namespace MyToolBar.Plugin.BasicPackage.OuterControls
{
    /// <summary>
    /// LemonAppMusic.xaml 的交互逻辑
    /// </summary>
    public partial class LemonAppMusic : OuterControlBase
    {
        public static SMTCHelper Smtc;
        public LemonAppMusic()
        {
            InitializeComponent();
            MaxStyleAct = maxStyleAct;
            Loaded += LemonAppMusic_Loaded;
            this.StylusSystemGesture += LemonAppMusic_StylusSystemGesture;
            this.StylusDown += LemonAppMusic_StylusDown;
        }
        public override void Dispose()
        {
            base.Dispose();
            Dispatcher.Invoke(()=> _musicServier?.Stop());
            if (_musicServier != null)
            {
                _musicServier.OnMsgReceived -= _musicServier_OnMsgReceived;
                _musicServier.OnClientExited -= _musicServier_OnClientExited;
            }
            Smtc.MediaPropertiesChanged -= Smtc_MediaPropertiesChanged;
            Smtc.SessionExited -= Smtc_SessionExited;
        }

        private Point _touchStart;
        private void LemonAppMusic_StylusDown(object sender, StylusDownEventArgs e)
        {
            _touchStart=e.GetPosition(this);
        }

        private async void LemonAppMusic_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (e.StylusDevice.TabletDevice.Type!=TabletDeviceType.Touch) return;
            Debug.WriteLine(e.SystemGesture);
            if (e.SystemGesture == SystemGesture.Tap)
            {
                (Resources["PlayOrPauseAni"] as Storyboard).Begin();
                await Smtc.PlayOrPause();
                e.Handled = true;
            }
            else if (e.SystemGesture == SystemGesture.Drag)
            {
                Point end = e.GetPosition(this);
                Debug.WriteLine(end.X - _touchStart.X);
                //按区域滑动手势 左 中 右 为上一曲 播放 下一曲
                if (Math.Abs(end.X - _touchStart.X) <= 5&&end.Y-_touchStart.Y>0)
                {
                    double regionLeft = ActualWidth / 3, regionRight = ActualWidth * 2 / 3;
                    if (end.X < regionLeft)
                        await Smtc.Previous();
                    else if (end.X >= regionLeft && end.X <= regionRight)
                        await Smtc.PlayOrPause();
                    else if (end.X > regionRight)
                        await Smtc.Next();
                    //都是向下滑的手势，暂且用这个动画
                    (Resources["PlayOrPauseAni"] as Storyboard).Begin();
                    return;
                }

                //左右滑动手势
                if (end.X - _touchStart.X > 5)
                {
                    (Resources["DragRight"] as Storyboard).Begin();
                    //右滑切换上一曲
                    await Smtc.Previous();
                    e.Handled = true;
                }
                else if (_touchStart.X - end.X > 5)
                {
                    (Resources["DragLeft"] as Storyboard).Begin();
                    //左滑切换下一曲
                    await Smtc.Next();
                    e.Handled = true;
                }
            }
        }

        private async void LemonAppMusic_Loaded(object sender, RoutedEventArgs e)
        {
            IsShown = false;
            Smtc =await SMTCHelper.CreateInstance();
            Smtc.MediaPropertiesChanged += Smtc_MediaPropertiesChanged;
            Smtc.SessionExited += Smtc_SessionExited;
            Smtc_MediaPropertiesChanged(null, null);
        }

        private void Smtc_SessionExited(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsShown = false;
            });
        }

        private void Smtc_MediaPropertiesChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(async() =>
            {
                var info = await Smtc.GetMediaInfoAsync();
                if (info == null) return;
                IsShown = true;
                if (Smtc.GetAppMediaId() == "LemonApp.exe")
                {
                    BeginLemonAppLyric();
                }
                LyricTb.Text = info.Title + " - " + info.Artist;
            });
        }
        private LemonAppMusicServier? _musicServier = null;
        private async void BeginLemonAppLyric()
        {
            if (_musicServier == null)
            {
                _musicServier = new();
                _musicServier.OnMsgReceived += _musicServier_OnMsgReceived;
                _musicServier.OnClientExited += _musicServier_OnClientExited;
                await _musicServier.StartAsync();
            }
        }

        private async void _musicServier_OnClientExited()
        {
            if (await Smtc.GetMediaInfoAsync() is { } info)
                LyricTb.Text = info.Title + " - " + info.Artist;
        }

        private void _musicServier_OnMsgReceived(string str)
        {
            Dispatcher.Invoke(() => {
                LyricTb.Text = str;
            });
        }

        private void maxStyleAct(bool max,Brush? foreColor) {
            if(foreColor!=null)
                Foreground = foreColor;
            else SetResourceReference(ForegroundProperty, "AppBarFontColor");
            if (max)
            {
                FontWeight = FontWeights.Normal;
            }
            else
            {
                FontWeight = FontWeights.Bold;
            }
        }

        bool _popShown = false;
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //判断设备为鼠标或笔而不是触摸
            if (e.StylusDevice!=null&&e.StylusDevice.TabletDevice.Type==TabletDeviceType.Touch) return;
            if (_popShown) return;
            _popShown = true;
            var w = new PopupWindows.LemonAppControlBox();
            w.Left = GlobalService.GetPopupWindowLeft(this, w);
            w.Closing += delegate { _popShown = false; };
            w.Show();
        }
    }
}
