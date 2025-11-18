using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyToolBar.Common;
using MyToolBar.Views.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace MyToolBar.ViewModels
{
    public record struct MenuItem(string Title, Geometry? Icon, Action? Event);
    public partial class MainTitleMenuViewModel:ObservableObject
    {
        #region Constructor
        public MainTitleMenuViewModel()
        {
            LoadMenuItems();
        }

        void SetItem(string contResName, string? iconResName, Action? Event)
        {
            var item = new MenuItem
            {
                Title = (string)App.Current.FindResource($"MenuItem_{contResName}"),
                Icon = iconResName == null ? null : (Geometry)App.Current.FindResource($"Icon_{iconResName}"),
                Event = Event
            };
            MenuItems.Add(item);
        }
        public ObservableCollection<MenuItem> MenuItems { get; } = [];

        [ObservableProperty]
        private MenuItem? _selectedItem;
        partial void OnSelectedItemChanged(MenuItem? value)
        {
            if(value == null) return;
            value?.Event?.Invoke();
            MenuItems.Clear();
        }

        public double MenuHeight => MenuItems.Count * 50;

        private void LoadMenuItems()
        {
            SetItem("Settings", "Settings", MenuItem_Settings);
            SetItem("Exit", null, MenuItem_Exit);
        }
        #endregion

        static bool _isSettingsWindowOpen = false;
        private void MenuItem_Settings()
        {
            if (_isSettingsWindowOpen) return;
            _isSettingsWindowOpen = true;

            var window = App.Host.Services.GetRequiredService<SettingsWindow>();
            window.Closing += delegate {
                _isSettingsWindowOpen = false;
            };
            window.Show();
        }
        private void MenuItem_Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
