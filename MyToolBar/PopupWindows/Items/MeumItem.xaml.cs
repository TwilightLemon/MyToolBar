using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    
        public Geometry Icon
        {
            get => IconPt.Data;
            set => IconPt.Data = value;
        }
    }
}
