using System;
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
        }
        public Process _pro;
        public ProcessItem(Process p)
        {
            InitializeComponent();
            _pro = p;
            ProName.Text = p.ProcessName;
            InfoTb.Text = NetworkInfo.FormatSize(p.WorkingSet64);
            Grid.SetColumnSpan(_ViewMask, 2);
        }
    }
}
