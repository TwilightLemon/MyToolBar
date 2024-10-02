using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyToolBar.Common.UIBases
{
    /// <summary>
    /// 为Capsule提供基类
    /// </summary>
    public class CapsuleBase : ItemBase
    {
        public virtual void Install() { }
        public virtual void Uninstall() { }

        public Type? PopupWindowType { get; set; }
        public bool IsPopupWindowOpen { get; set; } = false;
        public WeakReference<PopupWindowBase>? PopupWindowInstance { get; set; }

        protected virtual void SetPopupProperty()
        {
            if(PopupWindowInstance!=null&&PopupWindowInstance.TryGetTarget(out var window))
            {
                window.Closed += delegate { IsPopupWindowOpen = false; };
                window.Left = GlobalService.GetPopupWindowLeft(this, window);
            }
        }
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
