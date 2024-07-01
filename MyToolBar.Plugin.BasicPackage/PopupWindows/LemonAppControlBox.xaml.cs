using System.Windows.Input;
using System.Windows.Shapes;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.OuterControls;

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
            LemonAppMusic.Smtc.PlaybackInfoChanged += Smtc_PlaybackInfoChanged;
            UpdateStatus();
        }

        private void Smtc_PlaybackInfoChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                UpdateStatus();
            });
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
        private async void PlayLastBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await LemonAppMusic.Smtc.Previous();
        }

        private async void PlayBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await LemonAppMusic.Smtc.PlayOrPause();
        }

        private async void PlayNextBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await LemonAppMusic.Smtc.Next();
        }
    }
}
