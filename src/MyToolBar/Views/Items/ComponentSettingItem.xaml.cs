using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyToolBar.Plugin;
using MyToolBar.Services;

namespace MyToolBar.Views.Items
{
    /// <summary>
    /// 包级别卡片：显示包信息、启用/禁用开关、配置按钮。
    /// </summary>
    public partial class ComponentSettingItem : UserControl
    {
        private readonly ManagedPackageService _managedPackageService;
        private readonly IPackage _package;

        /// <summary>用户点击"配置"按钮时触发</summary>
        public event Action<IPackage, ManagedPackageService>? ConfigureClicked;

        public ComponentSettingItem(ManagedPackageService managedPackageService, IPackage package)
        {
            InitializeComponent();
            DataContext = package;
            _managedPackageService = managedPackageService;
            _package = package;

            // 同步当前启用状态到 ToggleButton
            bool isEnabled = managedPackageService.ManagedPkg
                .Any(p => p.Key == package.PackageName && p.Value.IsEnabled);
            EnableToggle.IsChecked = isEnabled;
        }

        private void EnableToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_package == null) return;

            bool isEnable = EnableToggle.IsChecked == true;
            // 状态未变化则跳过
            if (_managedPackageService.ManagedPkg.Any(p =>
                    p.Key == _package.PackageName && p.Value.IsEnabled == isEnable))
                return;

            if (isEnable)
                _managedPackageService.EnableInRegistered(_package.PackageName);
            else
                _managedPackageService.UnloadFromRegistered(_package.PackageName);
        }

        private void ConfigureBtn_Click(object sender, RoutedEventArgs e)
        {
            ConfigureClicked?.Invoke(_package, _managedPackageService);
        }
    }
}
