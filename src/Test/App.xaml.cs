using MyToolBar.Plugin.TabletUtils.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            new SideBarService().Start();
        }
    }

}
