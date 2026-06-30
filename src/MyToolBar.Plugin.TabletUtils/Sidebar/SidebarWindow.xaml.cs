using MyToolBar.Common;
using MyToolBar.Common.WinAPI;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MyToolBar.Plugin.TabletUtils.Sidebar;

/// <summary>
/// SideWindow.xaml 的交互逻辑
/// </summary>
public partial class SidebarWindow : Window
{
    public SidebarWindow()
    {
        InitializeComponent();
        Height = SystemParameters.WorkArea.Height-12;
        Deactivated += SideWindow_Deactivated;
        Activated += SideWindow_Activated;
        Loaded += SidebarWindow_Loaded;
    }

    private IntPtr hwnd;
    private async void SidebarWindow_Loaded(object sender, RoutedEventArgs e)
    {
        hwnd = new WindowInteropHelper(this).Handle;
    }


    private void SideWindow_Activated(object? sender, EventArgs e)
    {
        if (FixTb.IsChecked == true) return;

        Height = ScreenAPI.GetScreenArea(hwnd).Height - 64; //?? 
        var da = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(300));
        da.EasingFunction = new CircleEase();
        this.BeginAnimation(LeftProperty, da);
    }

    private void SideWindow_Deactivated(object? sender, EventArgs e)
    {
        if (FixTb.IsChecked == true) return;

        var da = new DoubleAnimation(-ActualWidth, TimeSpan.FromMilliseconds(300));
        da.EasingFunction = new CircleEase();
        da.Completed += delegate {
            Hide();
        };
        this.BeginAnimation(LeftProperty, da);
    }


    private async void SendBtn_Click(object sender, RoutedEventArgs e)
    {
       
    }
}
