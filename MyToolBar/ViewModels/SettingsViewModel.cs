using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MyToolBar.Common;
using MyToolBar.Views.Pages.Settings;

namespace MyToolBar.ViewModels
{

    public partial class SettingsViewModel : ObservableObject
    {
        //public record struct SettingsPage(string NameId, Geometry Icon, Type PageType,string Name="");
        public partial class SettingsPage(string nameId, Geometry icon, Type pageType):ObservableObject
        {
            public string NameId { get; private set; } = nameId;
            public Geometry Icon { get; private set; } = icon;
            public Type PageType { get; private set; } = pageType;
            [ObservableProperty]
            public string _name = "";
        }
        public SettingsViewModel()
        {
            UpdateData();
            LocalCulture.OnLanguageChanged += LocalCulture_OnLanguageChanged;
        }
        private void UpdateData()
        {
            foreach (var page in SettingsPages)
            {
                page.Name = (string)App.Current.FindResource(page.NameId);
            }
        }
        private void LocalCulture_OnLanguageChanged(object? sender, LocalCulture.Language e)
        {
            UpdateData();
        }

        public ObservableCollection<SettingsPage> SettingsPages { get; private set; } =
        [
            new SettingsPage("SettingsWindow_Tab_Gerneral", (Geometry)App.Current.FindResource("Icon_Gerneral"), typeof(AppSettingsPage)),
            new SettingsPage("SettingsWindow_Tab_Capsules", (Geometry)App.Current.FindResource("Icon_Capsule"), typeof(CapsulesSettingsPage)),
            new SettingsPage("SettingsWindow_Tab_OuterControl", (Geometry)App.Current.FindResource("Icon_OuterControl"), typeof(OuterControlSettingsPage)),
            new SettingsPage("SettingsWindow_Tab_Services",(Geometry)App.Current.FindResource("Icon_Service"),typeof(ServicesSettingsPage)),
            new SettingsPage("SettingsWindow_Tab_Components", (Geometry)App.Current.FindResource("Icon_Component"), typeof(ComponentsSettingsPage)),
            new SettingsPage("SettingsWindow_Tab_About", (Geometry)App.Current.FindResource("Icon_About"), typeof(AboutPage)),
        ];

        [ObservableProperty]
        private SettingsPage _selectedPage;

        [ObservableProperty]
        private object? _currentPageContent;

        partial void OnSelectedPageChanged(SettingsPage value)
        {
            var scope = App.Host.Services.CreateScope();
            var pageContent = scope.ServiceProvider.GetRequiredService(value.PageType);

            CurrentPageContent = pageContent;
        }
    }
}
