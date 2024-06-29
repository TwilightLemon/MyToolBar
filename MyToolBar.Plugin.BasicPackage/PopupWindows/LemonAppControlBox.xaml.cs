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
using MyToolBar.Common.UIBases;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyToolBar.Common.Func;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// LemonAppControlBox.xaml 的交互逻辑
    /// </summary>
    public partial class LemonAppControlBox : PopupWindowBase
    {
        public LemonAppControlBox()
        {
            InitializeComponent();
        }

        private void PlayLastBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_LAST, MsgHelper.ConnectedWindowHandle);
        }

        private void PlayBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_PAUSE, MsgHelper.ConnectedWindowHandle);
        }

        private void Path_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_NEXT, MsgHelper.ConnectedWindowHandle);
        }
    }
}
