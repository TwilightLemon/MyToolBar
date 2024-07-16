using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common.WinAPI;
using MyToolBar.Common.Behaviors;

namespace MyToolBar.Common.UIBases
{
    public class PopupWindowBase:Window
    {
        public PopupWindowBase()
        {
            Top = 0;

            //Set basic style for popup window
            SetResourceReference(BackgroundProperty,"MaskColor");
            SetResourceReference(ForegroundProperty, "ForeColor");
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            Topmost = true;
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                GlassFrameThickness = new Thickness(1),
                CaptionHeight = 1
            });

            Activate();
            Deactivated += PopWindowBase_Deactivated;
            this.Initialized += PopWindowBase_Initialized;
            this.Loaded += PopWindowBase_Loaded;
            this.ContentRendered += PopWindowBase_ContentRendered;
        }

        /// <summary>
        /// 指示PopupWindow是否总是保持打开，失去焦点时不自动关闭
        /// </summary>
        public bool AlwaysShow
        {
            get { return (bool)GetValue(AlwaysShowProperty); }
            set { SetValue(AlwaysShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AlwaysShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlwaysShowProperty =
            DependencyProperty.Register("AlwaysShow", typeof(bool), typeof(PopupWindowBase), new PropertyMetadata(false));



        private void PopWindowBase_Initialized(object? sender, EventArgs e)
        {
            //remove local resource dic, reflect ThemeConf to main Appdomain
            if (Resources.MergedDictionaries.FirstOrDefault(d => d.Source.ToString().Contains("ThemeColor.xaml"))
                is ResourceDictionary defResDic)
            {
                Resources.MergedDictionaries.Remove(defResDic);
            }
        }

        private void PopWindowBase_ContentRendered(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(0, 35, TimeSpan.FromMilliseconds(200));
            da.EasingFunction = new CubicEase();
            BeginAnimation(TopProperty, da);
            this.ContentRendered -= PopWindowBase_ContentRendered;
        }

        private void PopWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowAPI.SetToolWindow(this);
            BehaviorCollection behaviors = Interaction.GetBehaviors(this);
            behaviors.Add(new BlurWindowBehavior());
        }

        private void PopWindowBase_Deactivated(object? sender, EventArgs e)
        {
            if (AlwaysShow) return;
            var da = new DoubleAnimation(this.Top, -1 * this.Height, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            da.Completed += Da_Completed;
            BeginAnimation(TopProperty, da);
        }
        private void Da_Completed(object? sender, EventArgs e)
        {
            Close();
        }

        private LoadingIcon? _loadingIcon = null;
        public void SetLoadingStatus(bool isLoading)
        {
            if(Content is Grid container)
            {
                if (isLoading)
                {
                    _loadingIcon = new LoadingIcon();
                    container.Children.Add(_loadingIcon);
                }
                else if(_loadingIcon != null)
                {
                    container.Children.Remove(_loadingIcon);
                }
            }
        }
    }
}
