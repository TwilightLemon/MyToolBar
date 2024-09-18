using System.Windows;
using System.Windows.Input;

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
            StylusLeave += CapsuleBase_StylusLeave;
            Click += delegate { RequestPopup(); };
        }

        private Point _startPosition;
        private void CapsuleBase_StylusLeave(object sender, StylusEventArgs e)
        {
            Point endPoint= e.GetPosition(this);
            //向下滑动
            if (endPoint.Y - _startPosition.Y > 0)
                RequestPopup();
        }

        private void CapsuleBase_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.StylusDevice != null)//防止mouse触发
                _startPosition = e.GetPosition(this);
        }
    }
}
