using CommunityToolkit.Mvvm.ComponentModel;

namespace MyToolBar.ViewModels
{
    public partial class AppBarViewModel : ObservableObject
    {
        [ObservableProperty]
        private float _windowAccentCompositorOpacity;
    }
}
