using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyToolBar.Common.UIBases
{
    /// <summary>
    /// 为OutterControl提供基类，基本的显示操作和UI样式
    /// </summary>
    public class OuterControlBase : UserControl
    {
        private bool _isShown;

        public OuterControlBase()
        {
            Loaded += OuterControlBase_Loaded;
        }

        private void OuterControlBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //初始化前景色
            if (GlobalService.IsDarkMode)
                // DarkMode下 OutterControl区域高亮显示
                Foreground = GlobalService.OuterControlNormalDarkModeForeColor;
            else
            {
                //LightMode下 OutterControl区域跟随全局前景色
                SetResourceReference(ForegroundProperty, "ForeColor");
            }
        }

        /// <summary>
        /// 通知MainWindow需要显示或隐藏OutterControl
        /// </summary>
        public event EventHandler<bool>? IsShownChanged;

        /// <summary>
        /// 响应最大化样式 Brush为推荐的前景色
        /// </summary>
        public Action<bool,Brush>? MaxStyleAct;

        /// <summary>
        /// 指示是否需要显示OutterControl
        /// </summary>
        protected bool IsShown
        {
            get => _isShown;
            set => IsShownChanged?.Invoke(this, _isShown = value);
        }
    }
}
