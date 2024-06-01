using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyToolBar.Common.UIBases;
namespace MyToolBar.PopupWindows.Items
{
    /// <summary>
    /// MeumItem.xaml 的交互逻辑
    /// </summary>
    public partial class MenuItem : ItemBase
    {
        public MenuItem()
        {
            InitializeComponent();
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
            System.Windows.Controls.MenuItem.CommandProperty.AddOwner(typeof(MenuItem));

        public static readonly DependencyProperty CommandParameterProperty =
            System.Windows.Controls.MenuItem.CommandParameterProperty.AddOwner(typeof(MenuItem));



        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }



        internal static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsPressed), typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsPressedProperty =
            IsPressedPropertyKey.DependencyProperty;

        public static readonly RoutedEvent ClickEvent =
            System.Windows.Controls.MenuItem.ClickEvent.AddOwner(typeof(MenuItem));

        public string MeumContent
        {
            get => ContentTb.Text; set => ContentTb.Text = value;
        }

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
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MenuItem.ClickEvent, this);
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
            Mouse.Capture(this);
            if (this.IsMouseCaptured)
            {
                SetIsPressed(true);
                e.Handled = true;
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
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


            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if (e.OriginalSource == this)
            {
                SetIsPressed(false);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                UpdateIsPressed();
            }
        }

        public Geometry? Icon
        {
            get => IconPt.Data;
            set => IconPt.Data = value;
        }
    }
}
