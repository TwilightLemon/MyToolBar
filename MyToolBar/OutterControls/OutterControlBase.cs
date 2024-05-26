using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyToolBar.OutterControls
{
    /// <summary>
    /// 为OutterControl提供基类，基本的显示操作和UI样式
    /// </summary>
    public class OutterControlBase:UserControl
    {
        public OutterControlBase()
        {
            Loaded += OutterControlBase_Loaded;
        }

        private void OutterControlBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //初始化前景色
            if (GlobalService.DarkMode)
            {
                // DarkMode下 OutterControl区域高亮显示
                Foreground = GlobalService.OutterControlNormalDarkModeForeColor;
            }
            else
            {
                //LightMode下 OutterControl区域跟随全局前景色
                SetResourceReference(ForegroundProperty, "ForeColor");
            }
        }
        /// <summary>
        /// 通知MainWindow需要显示或隐藏OutterControl
        /// </summary>
        public event EventHandler<bool> IsShownChanged;
        /// <summary>
        /// 响应最大化样式 Brush为推荐的前景色
        /// </summary>
        public Action<bool,Brush>? MaxStyleAct;
        private bool isShown;
        /// <summary>
        /// 指示是否需要显示OutterControl
        /// </summary>
        protected bool IsShown
        {
            get=>isShown;
            set=>IsShownChanged?.Invoke(this, isShown = value);
        }
    }
}
