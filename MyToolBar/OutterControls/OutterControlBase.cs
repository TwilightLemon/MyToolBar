using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyToolBar.OutterControls
{
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
                Foreground=GlobalService.OutterControlNormalDarkModeForeColor;
            }
            else
            {
                SetResourceReference(ForegroundProperty, "ForeColor");
            }
        }

        public event EventHandler<bool> IsShownChanged;
        /// <summary>
        /// 响应最大化样式
        /// </summary>
        public Action<bool,Brush>? MaxStyleAct;
        private bool isShown;
        /// <summary>
        /// 指示是否需要显示OutterControl
        /// </summary>
        public bool IsShown
        {
            get=>isShown;
            set=>IsShownChanged?.Invoke(this, isShown = value);
        }
    }
}
