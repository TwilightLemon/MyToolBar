using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common.WinAPI;
using MyToolBar.Common.Behaviors;
using EleCho.WpfSuite;
using System.Windows.Media;

namespace MyToolBar.Common.UIBases
{
    public class PopupWindowBase:Window
    {
        public PopupWindowBase()
        {
            Top = 0;

            //Set basic style for popup window
            SetResourceReference(BackgroundProperty, "WindowBackgroundColor");
            SetResourceReference(ForegroundProperty, "ForeColor");
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowOption.SetCorner(this, WindowCorner.Round);
            BehaviorCollection behaviors = Interaction.GetBehaviors(this);
            behaviors.Add(new BlurWindowBehavior());
            ShowInTaskbar = false;
            Topmost = true;
            Activate();
            Deactivated += PopWindowBase_Deactivated;
            this.SourceInitialized += PopupWindowBase_SourceInitialized;
            this.Initialized += PopWindowBase_Initialized;
            this.ContentRendered += PopWindowBase_ContentRendered;
        }

        private void PopupWindowBase_SourceInitialized(object? sender, EventArgs e)
        {
            WindowLongAPI.SetToolWindow(this);
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
            //remove local resource dic, reflect ThemeConf to main AppDomain
            while (true)
            {
                if (Resources.MergedDictionaries.FirstOrDefault(d => d.Source.ToString().Contains("pack://application:,,,/MyToolBar.Common;component/Styles/"))
                    is ResourceDictionary defResDic)
                {
                    Resources.MergedDictionaries.Remove(defResDic);
                }
                else break;
            }
        }

        private void PopWindowBase_ContentRendered(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(0, 35, TimeSpan.FromMilliseconds(200));
            da.EasingFunction = new CubicEase();
            BeginAnimation(TopProperty, da);
            this.ContentRendered -= PopWindowBase_ContentRendered;
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
        /// <summary>
        /// 设置加载状态
        /// </summary>
        /// <param name="isLoading"></param>
        public void SetLoadingStatus(bool isLoading)
        {
            if(Content is System.Windows.Controls.Grid {} container)
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
