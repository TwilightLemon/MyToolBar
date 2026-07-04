using CommunityToolkit.Mvvm.ComponentModel;
using MyToolBar.Plugin;

namespace MyToolBar.Views.Pages.Settings
{
    public partial class PluginStateData(IPlugin plugin, bool isEnabled) : ObservableObject
    {
        public IPlugin Plugin { get; init; } = plugin;
        [ObservableProperty] private bool _isEnabled = isEnabled;
    }
}
