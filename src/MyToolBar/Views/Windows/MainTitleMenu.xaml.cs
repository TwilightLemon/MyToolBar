using MyToolBar.Common.UIBases;
using MyToolBar.ViewModels;

namespace MyToolBar.Views.Windows
{
    /// <summary>
    /// 主菜单
    /// </summary>
    public partial class MainTitleMenu : PopupWindowBase
    {
        public MainTitleMenu(MainTitleMenuViewModel viewModel)
        {
            DataContext = this;
            ViewModel = viewModel;
            InitializeComponent();
        }
        public MainTitleMenuViewModel ViewModel { get; }
    }
}
