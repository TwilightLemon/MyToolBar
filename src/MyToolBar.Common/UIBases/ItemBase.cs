using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ws = EleCho.WpfSuite;

namespace MyToolBar.Common.UIBases;

/// <summary>
/// 为可交互Item提供基类：鼠标悬停遮罩、Click事件与Command绑定。
/// 遮罩 Border 作为视觉树底层元素，在鼠标悬停时显示。
/// </summary>
public class ItemBase:UserControl
{
    public ItemBase()
    {
        Initialized += ItemBase_Initialized;
        Background = Brushes.Transparent;
    }

    protected bool EnableClickEvent = true;

    private Border? _maskBorder;

    /// <summary>
    /// 获取或设置鼠标悬停遮罩的圆角半径。
    /// </summary>
    public CornerRadius MaskCornerRadius
    {
        get => (CornerRadius)GetValue(MaskCornerRadiusProperty);
        set => SetValue(MaskCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty MaskCornerRadiusProperty =
        DependencyProperty.Register(
            nameof(MaskCornerRadius),
            typeof(CornerRadius),
            typeof(ItemBase),
            new PropertyMetadata(new CornerRadius(12), OnMaskCornerRadiusChanged));

    private static void OnMaskCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemBase item && item._maskBorder != null)
        {
            item._maskBorder.CornerRadius = (CornerRadius)e.NewValue;
        }
    }

    #region MaskBackgroundKey

    /// <summary>
    /// 获取或设置遮罩的背景资源键。
    /// CapsuleBase 覆盖为 "SystemThemeColor"，普通 Item 使用 "MaskColor"。
    /// </summary>
    public string MaskBackgroundKey
    {
        get => (string)GetValue(MaskBackgroundKeyProperty);
        set => SetValue(MaskBackgroundKeyProperty, value);
    }

    public static readonly DependencyProperty MaskBackgroundKeyProperty =
        DependencyProperty.Register(
            nameof(MaskBackgroundKey),
            typeof(string),
            typeof(ItemBase),
            new PropertyMetadata("MaskColor", OnMaskBackgroundKeyChanged));

    private static void OnMaskBackgroundKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemBase item && item._maskBorder != null)
        {
            item._maskBorder.SetResourceReference(Border.BackgroundProperty, (string)e.NewValue);
        }
    }

    #endregion

    #region IsPinned

    /// <summary>
    /// 当为 true 时，遮罩保持常亮，鼠标进入/离开不切换可见性。
    /// 由 CapsuleBase 在 PopupWindow 打开/关闭时控制。
    /// </summary>
    public bool IsPinned
    {
        get => (bool)GetValue(IsPinnedProperty);
        set => SetValue(IsPinnedProperty, value);
    }

    public static readonly DependencyProperty IsPinnedProperty =
        DependencyProperty.Register(
            nameof(IsPinned),
            typeof(bool),
            typeof(ItemBase),
            new PropertyMetadata(false, OnIsPinnedChanged));

    private static void OnIsPinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemBase item && item._maskBorder != null)
        {
            bool pinned = (bool)e.NewValue;
            item._maskBorder.Opacity = pinned ? 1 : 0;
        }
    }

    #endregion

    #region IgnoreTouch

    /// <summary>
    /// 是否忽略触摸笔输入。默认 true，触摸笔悬停不触发遮罩。
    /// </summary>
    public bool IgnoreTouch
    {
        get => (bool)GetValue(IgnoreTouchProperty);
        set => SetValue(IgnoreTouchProperty, value);
    }

    public static readonly DependencyProperty IgnoreTouchProperty =
        DependencyProperty.Register(
            nameof(IgnoreTouch),
            typeof(bool),
            typeof(ItemBase),
            new PropertyMetadata(true));

    #endregion

    private void ItemBase_Initialized(object? sender, EventArgs e)
    {
        this.Cursor = Cursors.Hand;

        // 将原有Content包裹在一个Grid中，底层放mask Border
        var originalContent = this.Content;
        // 先断开原有Content与UserControl的逻辑树关系，否则无法将其加入wrapper
        this.Content = null;

        _maskBorder = new Border
        {
            CornerRadius = MaskCornerRadius,
            Opacity = IsPinned ? 1 : 0,
            IsHitTestVisible = false,
        };
        _maskBorder.SetResourceReference(Border.BackgroundProperty, MaskBackgroundKey);

        var wrapper = new ws.BoxPanel();
        wrapper.Children.Add(_maskBorder);
        if (originalContent is UIElement uiElement)
        {
            wrapper.Children.Add(uiElement);
        }
        this.Content = wrapper;
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

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (IsPinned)
            return;

        // 触摸笔输入时忽略
        if (IgnoreTouch
            && e.StylusDevice is not null
            && e.StylusDevice.TabletDevice?.Type == TabletDeviceType.Touch)
            return;

        if (_maskBorder != null)
            _maskBorder.Opacity = 1;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (IsPinned)
            return;

        if (_maskBorder != null)
            _maskBorder.Opacity = 0;
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
