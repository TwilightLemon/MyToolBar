using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyToolBar.ViewModels
{
    public partial class AppBarViewModel : ObservableObject
    {
        [ObservableProperty]
        private Color _windowAccentGradientColor;
    }
}
