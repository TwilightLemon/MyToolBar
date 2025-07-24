using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public virtual void Dispose() { }

        private void OuterControlBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //初始化前景色
            SetResourceReference(ForegroundProperty, "AppBarForegroundCenter");
        }

        /// <summary>
        /// 通知MainWindow需要显示或隐藏OutterControl
        /// </summary>
        public event EventHandler<bool>? IsShownChanged;

        /// <summary>
        /// 指示是否需要显示OutterControl
        /// </summary>
        [DefaultValue(false)]
        protected bool IsShown
        {
            get => _isShown;
            set { if (_isShown != value) IsShownChanged?.Invoke(this, _isShown = value); }
        }
    }
}
