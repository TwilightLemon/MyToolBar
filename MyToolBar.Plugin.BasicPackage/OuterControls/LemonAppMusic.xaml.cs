using System;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
            Loaded += LemonAppMusic_Loaded;
        }
        private async void LemonAppMusic_Loaded(object sender, RoutedEventArgs e)
        {
            MaxStyleAct= maxStyleAct;
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
                    LyricTb.Text = info.AlbumTitle;
                }
                else
                {
                    LyricTb.Text = info.Title + " - " + info.Artist;
                }
            });
        }

        private void maxStyleAct(bool max,Brush foreColor) {
            if(foreColor!=null)
                LyricTb.Foreground = foreColor;
            else LyricTb.SetResourceReference(ForegroundProperty, "AppBarFontColor");
            if (max)
            {
                LyricTb.FontWeight = FontWeights.Normal;
            }
            else
            {
                LyricTb.FontWeight = FontWeights.Bold;
            }
        }

        private async void Func_Left_TouchDown(object sender, TouchEventArgs e)
        {
            await Smtc.Previous();
        }

        private async void Func_Center_TouchDown(object sender, TouchEventArgs e)
        {
            await Smtc.PlayOrPause();
        }

        private async void Func_Right_TouchDown(object sender, TouchEventArgs e)
        {
            await Smtc.Next();
        }

        bool _popShown = false;
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_popShown) return;
            _popShown = true;
            var w = new PopupWindows.LemonAppControlBox();
            w.Left = GlobalService.GetPopupWindowLeft(this, w);
            w.Closing += delegate { _popShown = false; };
            w.Show();
        }
    }
}
