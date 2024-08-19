using System;
using System.Collections.Generic;
using System.Windows.Media;
using MyToolBar.Common.UIBases;
namespace MyToolBar.PopupWindows.Items
{
    /// <summary>
    /// MenuItem.xaml 的交互逻辑
    /// </summary>
    public partial class MenuItem : ItemBase
    {
        public MenuItem()
        {
            InitializeComponent();
        }
        public string MenuContent {
            get => ContentTb.Text; set=>ContentTb.Text = value;
        }
    
        public Geometry? Icon
        {
            get => IconPt.Data;
            set => IconPt.Data = value;
        }
    }
}
