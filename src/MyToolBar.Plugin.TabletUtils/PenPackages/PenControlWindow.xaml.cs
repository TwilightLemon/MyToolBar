using System.Windows;
using System.Windows.Input;
using MyToolBar.Common.WinAPI;
using System.Windows.Media.Animation;
using MyToolBar.Common;
using System.Windows.Interop;

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
            _closeAni = Resources["CloseAni"] as Storyboard;
            _closeAni.Completed += delegate {
                Width = 40;
                Height = 40;
                _isOpen = false;
            };
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(ResetWindowLocation);
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
        private IntPtr _hwnd;
        private void PenControlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResetWindowLocation();
            _hwnd= new WindowInteropHelper(this).Handle;
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
                this.Width = 200;
                this.Height = 200;
                _openAni.Begin();
            }
        }
        private void ClosePanel()
        {
            if (_isOpen)
            {
                _closeAni.Begin();
            }
        }
        private void ResetWindowLocation()
        {
            if (_hwnd == IntPtr.Zero) return;
           this.Left = ScreenAPI.GetScreenArea(_hwnd).Width - ActualHeight;
            this.Top = 0;
        }

        private Point _startPosition;
        private void Window_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            if(e.StylusDevice!=null)//防止mouse触发
                _startPosition = e.GetPosition(this);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            ClosePanel();
        }

        private void PrtScrBtn_Click(object sender, RoutedEventArgs e)
        {
            ClosePanel();
            SendHotKey.ScreenShot();
        }

        private void DrawBtn_Click(object sender, RoutedEventArgs e)
        {
             ClosePanel();
            new DrawboardWindow().Show();
        }
    }
}
