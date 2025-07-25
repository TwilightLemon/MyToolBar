using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.OuterControls;
using Path = System.Windows.Shapes.Path;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// LemonAppControlBox.xaml 的交互逻辑
    /// </summary>
    public partial class LemonAppControlBox : PopupWindowBase
    {
        public LemonAppControlBox()
        {
            InitializeComponent();
            Closing += LemonAppControlBox_Closing;
            LemonAppMusic.Smtc.PlaybackInfoChanged += Smtc_PlaybackInfoChanged;
            LemonAppMusic.Smtc.MediaPropertiesChanged += Smtc_MediaPropertiesChanged;
            UpdateMediaInfo();
            UpdateStatus();
        }

        private void LemonAppControlBox_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            LemonAppMusic.Smtc.PlaybackInfoChanged -= Smtc_PlaybackInfoChanged;
            LemonAppMusic.Smtc.MediaPropertiesChanged -= Smtc_MediaPropertiesChanged;
        }

        private void Smtc_MediaPropertiesChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(UpdateMediaInfo);
        }

        private void Smtc_PlaybackInfoChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(UpdateStatus);
        }
        private Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties? _info = null;
        private async void UpdateMediaInfo()
        {
            var info= await LemonAppMusic.Smtc.GetMediaInfoAsync();
            if (info == null||(_info!=null&&_info.Title==info.Title)) return;
            _info = info;
            TitleTb.Text = info.Title;
            ArtistTb.Text = info.Artist;
            void NoImg() {
                ThumbnailImg.Background = null;
                InfoTb.Margin = new Thickness(20, 20, 20, 0);
            }
            if(info.Thumbnail != null)
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = (await info.Thumbnail.OpenReadAsync()).AsStream();
                    img.EndInit();
                    ThumbnailImg.Background = new ImageBrush(img);
                    InfoTb.Margin = new Thickness(20, 20, 100, 0);
                }
                catch
                {
                    NoImg();
                }
            }
            else
            {
                NoImg();
            }
        }

        private void UpdateStatus()
        {
            var status = LemonAppMusic.Smtc.GetPlaybackStatus();
            if (status == Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                PlayBtnIcon.SetResourceReference(Path.DataProperty, "Icon_Pause");
            }
            else if (status == Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
            {
                PlayBtnIcon.SetResourceReference(Path.DataProperty, "Icon_Play");
            }
        }

        private void PlayLastBtn_Click(object sender, RoutedEventArgs e)
        {
            _= LemonAppMusic.Smtc.Previous();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            _= LemonAppMusic.Smtc.PlayOrPause();
        }

        private void PlayNextBtn_Click(object sender, RoutedEventArgs e)
        {
            _= LemonAppMusic.Smtc.Next();
        }
    }
}
