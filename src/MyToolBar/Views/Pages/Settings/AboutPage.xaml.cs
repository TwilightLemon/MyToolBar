using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public string AppVersion { get; }

        public List<string> ThirdPartyLibraries { get; } =
        [
            "EleCho.WpfSuite",
            "CommunityToolkit.Mvvm",
            "Microsoft.Extensions.Hosting",
            "NLog / NLog.Extensions.Hosting",
        ];

        public AboutPage()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
            DataContext = this;
            InitializeComponent();
        }

        private void GitHubLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/TwilightLemon/MyToolBar",
                UseShellExecute = true
            });
        }
    }
}
