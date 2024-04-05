using System.Windows;
using MyToolBar.Func;

namespace ApiTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var city=await (await WeatherApi.GetPositionByIpAsync()).VerifyCityIdAsync();
        }
    }
}