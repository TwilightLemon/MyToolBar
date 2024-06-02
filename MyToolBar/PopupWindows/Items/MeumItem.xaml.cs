using System;
using System.Collections.Generic;
using System.Windows.Media;
using MyToolBar.Common.UIBases;
namespace MyToolBar.PopupWindows.Items
{
    /// <summary>
    /// MeumItem.xaml 的交互逻辑
    /// </summary>
    public partial class MeumItem : ItemBase
    {
        public MeumItem()
        {
            InitializeComponent();
        }
        public string MeumContent {
            get => ContentTb.Text; set=>ContentTb.Text = value;
        }
    
        public Geometry? Icon
        {
            get => IconPt.Data;
            set => IconPt.Data = value;
        }
    }
}
