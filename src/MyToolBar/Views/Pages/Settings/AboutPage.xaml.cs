using System.Windows.Controls;
using System.Windows.Markup;
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

        private async void AboutPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var loading = new LoadingIcon();
            ContentGrid.Children.Add(loading);
            string data = await HttpHelper.Get("https://gitee.com/TwilightLemon/MyToolBarDynamics/raw/master/AboutPage.xaml");
            ContentGrid.Children.Add((Grid)XamlReader.Parse(data));
            ContentGrid.Children.Remove(loading);
        }
    }
}
