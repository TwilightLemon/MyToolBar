using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MyToolBar.Common.UIBases
{
    /// <summary>
    /// 为可交互Item提供基类 ViewMask 基本视觉样式 Click事件与Command绑定
    /// 要求Content为Grid
    /// </summary>
    public class ItemBase:UserControl
    {
        public ItemBase()
        {
            Initialized += ItemBase_Initialized;
        }
        protected Grid? _Container;
        protected bool  EnableClickEvent=true;
        protected Border? _ViewMask;
        private void ItemBase_Initialized(object? sender, EventArgs e)
        {
            if(Content is Grid g)
            {
                this.Cursor = Cursors.Hand;
                _Container = g;
                _ViewMask = new Border()
                {
                    CornerRadius=new CornerRadius(12),
                    Opacity=0
                };
                _ViewMask.SetResourceReference(BackgroundProperty, "MaskColor");
                MouseEnter += (s,e)=>{
                    if (e.StylusDevice != null && e.StylusDevice.TabletDevice.Type == TabletDeviceType.Touch) return;
                    _ViewMask.Opacity = 1;
                };
                MouseLeave += delegate
                {
                    _ViewMask.Opacity = 0;
                    //_ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 0, TimeSpan.FromMilliseconds(300)));
                };
                //插入到最底层
                _Container.Children.Insert(0, _ViewMask);
            }
        }

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object? CommandParameter
        {
            get { return (object?)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }




        public static readonly DependencyProperty CommandProperty =
            ButtonBase.CommandProperty.AddOwner(typeof(ItemBase));

        public static readonly DependencyProperty CommandParameterProperty =
            ButtonBase.CommandParameterProperty.AddOwner(typeof(ItemBase));



        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }



        internal static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsPressed), typeof(bool), typeof(ItemBase), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsPressedProperty =
            IsPressedPropertyKey.DependencyProperty;

        public static readonly RoutedEvent ClickEvent =
            ButtonBase.ClickEvent.AddOwner(typeof(ItemBase));

        private void SetIsPressed(bool value)
        {
            SetValue(IsPressedPropertyKey, value);
        }

        private void UpdateIsPressed()
        {
            Point pos = Mouse.PrimaryDevice.GetPosition(this);

            if ((pos.X >= 0) && (pos.X <= ActualWidth) && (pos.Y >= 0) && (pos.Y <= ActualHeight))
            {
                if (!IsPressed)
                {
                    SetIsPressed(true);
                }
            }
            else if (IsPressed)
            {
                SetIsPressed(false);
            }
        }

        protected virtual void OnClick()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ButtonBase.ClickEvent, this);
            RaiseEvent(newEventArgs);

            if (Command is ICommand command)
            {
                var commandParameter = CommandParameter;
                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (EnableClickEvent)
            {
                Mouse.Capture(this);
                if (this.IsMouseCaptured)
                {
                    SetIsPressed(true);
                    e.Handled = true;
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (EnableClickEvent)
            {
                bool shouldClick = IsPressed && IsMouseOver;

                SetIsPressed(false);

                if (IsMouseCaptured)
                {
                    ReleaseMouseCapture();
                }

                if (shouldClick)
                {
                    OnClick();
                }

                e.Handled = true;
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if (EnableClickEvent&&e.OriginalSource == this)
            {
                SetIsPressed(false);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (EnableClickEvent&&IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                UpdateIsPressed();
            }
        }
    }
}
