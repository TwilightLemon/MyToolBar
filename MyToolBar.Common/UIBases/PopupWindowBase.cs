using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common.WinApi;
using MyToolBar.Common.Behaviors;

namespace MyToolBar.Common.UIBases
{
    public class PopupWindowBase : Window
    {
        static PopupWindowBase()
        {
            // override property default values
            WindowStyleProperty.OverrideMetadata(typeof(PopupWindowBase), new FrameworkPropertyMetadata(WindowStyle.None));
            ShowInTaskbarProperty.OverrideMetadata(typeof(PopupWindowBase), new FrameworkPropertyMetadata(false));
            TopmostProperty.OverrideMetadata(typeof(PopupWindowBase), new FrameworkPropertyMetadata(true));
        }


        private LoadingIcon? _loadingIcon = null;


        public PopupWindowBase()
        {
            // property defaults
            Top = 0;

            //Set basic style for popup window
            SetResourceReference(BackgroundProperty, "MaskColor");
            SetResourceReference(ForegroundProperty, "ForeColor");

            // startup animation
            this.ContentRendered += PopWindowBase_ContentRendered;

            // acrylic backdrop
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                GlassFrameThickness = new Thickness(-1),
                CaptionHeight = 1
            });

            EleCho.WpfSuite.WindowOption.SetBackdrop(this, EleCho.WpfSuite.WindowBackdrop.Acrylic);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //remove local resource dic, reflect ThemeConf to main Appdomain
            if (Resources.MergedDictionaries.FirstOrDefault(d => d.Source.ToString().Contains("ThemeColor.xaml"))
                is ResourceDictionary defResDic)
            {
                Resources.MergedDictionaries.Remove(defResDic);
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            var da = new DoubleAnimation(this.Top, -1 * this.Height, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            da.Completed += DeactiveAnimationCompleted;
            BeginAnimation(TopProperty, da);
        }

        private void PopWindowBase_ContentRendered(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(0, 35, TimeSpan.FromMilliseconds(200));
            da.EasingFunction = new CubicEase();
            da.Completed += (s, e) => this.IsEnabled = true;

            BeginAnimation(TopProperty, da);
            this.ContentRendered -= PopWindowBase_ContentRendered;
        }

        private void DeactiveAnimationCompleted(object? sender, EventArgs e)
        {
            Close();
        }

        public void SetLoadingStatus(bool isLoading)
        {
            if (Content is Grid container)
            {
                if (isLoading)
                {
                    _loadingIcon = new LoadingIcon();
                    container.Children.Add(_loadingIcon);
                }
                else if (_loadingIcon != null)
                {
                    container.Children.Remove(_loadingIcon);
                }
            }
        }
    }
}
