using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using MyToolBar.Common.UIBases;
using System.Windows.Controls;
using MyToolBar.Plugin.BasicPackage.API;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// ProcessItem.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessItem : ItemBase
    {
        public ProcessItem()
        {
            InitializeComponent();
            Grid.SetColumnSpan(_ViewMask, 2);
        }
        public Process? _pro = null;
        public ProcessItem(Process p)
        {
            InitializeComponent();
            UpdateData(p);
            Grid.SetColumnSpan(_ViewMask, 2);
        }
        public void UpdateData(Process? p)
        {
            this.Visibility=p==null? Visibility.Collapsed:Visibility.Visible;
            _pro = p;
            ProName.Text = p.ProcessName;
            InfoTb.Text = NetworkInfo.FormatSize(p.WorkingSet64);
        }
    }
}
