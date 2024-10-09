using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using MyToolBar.Common;
using MyToolBar.Common.Func;
using MyToolBar.Common.UIBases;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            Loaded += AboutPage_Loaded;
        }

        private async void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            var loading = new LoadingIcon();
            ContentGrid.Children.Add(loading);
            string data = await HttpHelper.Get($"https://gitee.com/TwilightLemon/MyToolBarDynamics/raw/master/AboutPage{
                LocalCulture.Current switch {
                    LocalCulture.Language.zh_cn=>".Zh-CN",
                    LocalCulture.Language.en_us=>".En-US",
                    _ => ""
                }
                }.xaml");
            ContentGrid.Children.Add((Grid)XamlReader.Parse(data));
            ContentGrid.Children.Remove(loading);
        }
    }
}
