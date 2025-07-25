using System.Windows;
using System.Windows.Input;

namespace MyToolBar.Common.UIBases
{
    /// <summary>
    /// Capsule基类，提供Install, Uninstall方法和PopupWindow响应
    /// </summary>
    public class CapsuleBase : ItemBase
    {
        public virtual void Install() { }
        public virtual void Uninstall() { }

        /// <summary>
        /// 获取或设置Capsule的弹出窗口类型，CapsuleBase会自动创建并在用户请求时唤出窗口
        /// </summary>
        protected Type? PopupWindowType { get; set; }
        protected bool IsPopupWindowOpen { get; set; } = false;
        protected WeakReference<PopupWindowBase>? PopupWindowInstance { get; set; }

        /// <summary>
        /// 创建PopupWindow实例后，设置其必要属性
        /// </summary>
        protected virtual void SetPopupProperty()
        {
            if(PopupWindowInstance?.TryGetTarget(out var window) is true)
            {
                window.Closed += delegate { IsPopupWindowOpen = false; };
                window.Left = GlobalService.GetPopupWindowLeft(this, window);
            }
        }
        /// <summary>
        /// 请求创建PopupWindow
        /// </summary>
        protected virtual void RequestPopup()
        {
            if (IsPopupWindowOpen|| PopupWindowType==null ||
                PopupWindowType.IsAssignableFrom(typeof(PopupWindowBase)))
                return;

            if(Activator.CreateInstance(PopupWindowType) is PopupWindowBase window)
            {
                PopupWindowInstance = new WeakReference<PopupWindowBase>(window);
                SetPopupProperty();
                window.Show();
                IsPopupWindowOpen = true;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            PreviewStylusDown += CapsuleBase_PreviewStylusDown;
            StylusMove += CapsuleBase_StylusMove;
            Click += delegate { RequestPopup(); };
        }

        private void CapsuleBase_StylusMove(object sender, StylusEventArgs e)
        {
            if (e.InAir) return;
            var currentPosition = e.GetPosition(this);

            if (currentPosition.X < 0 || currentPosition.Y < 0 ||
                currentPosition.X > ActualWidth || currentPosition.Y > ActualHeight)
            {
                // Stylus已经离开了元素，处理相应的逻辑
                if (currentPosition.Y - _startPosition.Y > 0)
                    RequestPopup();

                // 释放Stylus设备
                Stylus.Capture(null);
            }
        }

        private Point _startPosition;
        private void CapsuleBase_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.InAir) return;
            _startPosition = e.GetPosition(this);

            Stylus.Capture(this);
        }
    }
}
