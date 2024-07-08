using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common;
using MyToolBar.Common.Behaviors;
using MyToolBar.Common.WinApi;

namespace MyToolBar.PenPackages
{
    /// <summary>
    /// DrawboardWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DrawboardWindow : Window
    {
        public DrawboardWindow()
        {
            GlobalService.EnableHideWhenFullScreen = false;
            InitializeComponent();
            this.Loaded += DrawboardWindow_Loaded;
            this.PreviewStylusInRange += DrawboardWindow_PreviewStylusInRange;
            this.PreviewStylusOutOfRange += DrawboardWindow_PreviewStylusOutOfRange;
            this.PreviewStylusButtonDown += DrawboardWindow_PreviewStylusButtonDown;
            this.PreviewMouseDoubleClick += DrawboardWindow_PreviewMouseDoubleClick;
            this.StylusSystemGesture += DrawboardWindow_StylusSystemGesture;
            this.Closing += DrawboardWindow_Closing;
        }

        private void DrawboardWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalService.EnableHideWhenFullScreen = true;
        }

        private void DrawboardWindow_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            //双指点击清空选中项 若没有选中则视为插入
            if (e.SystemGesture == SystemGesture.TwoFingerTap)
            {
                Drawboard_ClearSeleted(() => AddBtn_MouseUp(null, null));
            }
        }

        /// <summary>
        /// 删除已选中的元素和笔画
        /// </summary>
        /// <param name="None">没有选中项时发生</param>
        private void Drawboard_ClearSeleted(Action? None=null)
        {
            bool clearAny = false;
            var es = ink.GetSelectedElements();
            var ss = ink.GetSelectedStrokes();
            if (es.Count != 0)
            {
                clearAny = true;
                for (int i = es.Count - 1; i >= 0; i--)
                    ink.Children.Remove(es[i]);
            }
            if (ss.Count != 0)
            {
                clearAny = true;
                for (int i = ss.Count - 1; i >= 0; i--)
                    ink.Strokes.Remove(ss[i]);
            }
            if (!clearAny)
                None?.Invoke();
        }

        /// <summary>
        /// 侧键实例 用于判断选择模式
        /// </summary>
        private StylusButton _SideBtn = null;
        private void DrawboardWindow_PreviewStylusButtonDown(object sender, StylusButtonEventArgs e)
        {
            if (e.StylusDevice.TabletDevice.Type != TabletDeviceType.Stylus) return;
           //判断是否为侧键
           if (e.StylusButton.Guid == StylusPointProperties.BarrelButton.Id)
            {
                _SideBtn = e.StylusButton;
                ink.EditingMode = InkCanvasEditingMode.Select;
            }
            else if(_SideBtn?.StylusButtonState == StylusButtonState.Up)
            {
                //有选中时，即使侧键放开也保持选择模式
                if(!(ink.GetSelectedElements().Count != 0 || ink.GetSelectedStrokes().Count!=0))
                    ink.EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        /// <summary>
        /// 绘画模式 or 穿透模式
        /// </summary>
        private bool _isDrawingMode = false;
        private void DrawboardWindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //防止在使用笔时或有选中内容时触发穿透
            if (e.StylusDevice?.TabletDevice.Type == TabletDeviceType.Stylus || 
                ink.GetSelectedElements().Count>0||ink.GetSelectedStrokes().Count>0)
                return;
            Drawboard_Penetrate();
        }
        /// <summary>
        ///绘画->穿透
        /// </summary>
        private void Drawboard_Penetrate()
        {
            _isDrawingMode = false;
            SolidColorBrush bg = new SolidColorBrush();
            Background = bg;
            var ca = new ColorAnimation(Color.FromArgb(20, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), TimeSpan.FromMilliseconds(400));
            ca.Completed += delegate
            {
                Background = null;
                if (_CurrentPen != null)
                {
                    _CurrentPen.BorderThickness = new Thickness(0);
                    _CurrentPen = null;
                }
            };
            bg.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            Title= "Drawboard - Penetrating Mode";
        }
        private void Drawboard_ClearAll()
        {
            ink.Children.Clear();
            ink.Strokes.Clear();
        }

        private void DrawboardWindow_PreviewStylusOutOfRange(object sender, StylusEventArgs e)
        {
            //笔离开时可用鼠标或触摸进行选择
            if (e.StylusDevice.TabletDevice.Type == TabletDeviceType.Stylus)
            {
                ink.EditingMode = InkCanvasEditingMode.Select;
                Debug.WriteLine("StylusOutOfRange --Select ");
            }
        }

        private void DrawboardWindow_PreviewStylusInRange(object sender, StylusEventArgs e)
        {
            //进入绘画区域时启用笔绘画
            if (e.StylusDevice.TabletDevice.Type == TabletDeviceType.Stylus)
            {
              //  ink.IsEnabled= true;
                //没有按下侧键时才切换到墨迹
                if (_SideBtn==null||_SideBtn.StylusButtonState == StylusButtonState.Up){
                    Debug.WriteLine("StylusInRange --Ink ");
                    ink.EditingMode = InkCanvasEditingMode.Ink;
                }
            }
        }

        private void DrawboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowApi.SetToolWindow(this);
            Topmost = true;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Left = 0;
            this.Top = 0;
            var beh = Interaction.GetBehaviors(ToolPanel);
            beh.Add(new DraggableUIBehavior(this,ViewMask));
            

            //响应粘贴
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (s, e) =>
            {
                AddBtn_MouseUp(null, null);
            }));
            //响应删除
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, (s, e) => {
                Drawboard_ClearSeleted();
            }));

            //启动时根据主题选择默认笔刷颜色
            foreach (Border item in PenColors.Children)
            {
                item.MouseUp += PenColor_MouseUp;
            }
            PenColor_MouseUp(PenColors.Children[GlobalService.IsDarkMode ? 1 : 0], null);
        }
        /// <summary>
        /// 当前选择的笔刷颜色
        /// </summary>
        private Border _CurrentPen = null;
        private void PenColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border b) return;
            if (_CurrentPen != null)
            {
                _CurrentPen.BorderThickness = new Thickness(0);
                _CurrentPen.Child = null;
            }
            b.BorderThickness = new Thickness(2);

            _CurrentPen = b;
            var penColor = ((SolidColorBrush)_CurrentPen.Background).Color;
            b.BorderBrush=new SolidColorBrush(
                GlobalService.IsDarkMode ? (penColor==Colors.White?Colors.SkyBlue:Colors.White):(penColor==Colors.Black?Colors.SkyBlue:Colors.Black)
                );
            if (!_isDrawingMode)
            {
                SolidColorBrush bg = new SolidColorBrush();
                Background = bg;
                bg.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(20, 0, 0, 0), TimeSpan.FromMilliseconds(400)));
            }
            _isDrawingMode = true;
            ink.DefaultDrawingAttributes.Color = penColor;
            Title = "Drawboard - Drawing Mode";
        }
        private bool _dragMoveElement = false;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            //非绘画模式或没有选中元素时不捕获鼠标
            if (!_isDrawingMode || ink.GetSelectedElements().Count == 0) return; 

            // 检查点击位置是否在选中的元素上
            Point clickedPoint = e.GetPosition(ink);
            foreach (UIElement element in ink.Children)
            {
                if (ink.GetSelectedElements().Contains(element))
                {
                    Rect elementBounds = VisualTreeHelper.GetDescendantBounds(element);
                    Point elementPosition = element.TranslatePoint(new Point(0, 0), ink);

                    Rect absoluteBounds = new Rect(elementPosition, elementBounds.Size);
                    if (absoluteBounds.Contains(clickedPoint))
                    {
                        ink.CaptureMouse();
                        _dragMoveElement = true;
                        break;
                    }
                }
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (ink.IsMouseCaptured&& _dragMoveElement)
            {
                ink.ReleaseMouseCapture();
                _dragMoveElement = false;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (_dragMoveElement&&ink.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(ink);
                foreach (UIElement element in ink.GetSelectedElements())
                {
                    InkCanvas.SetLeft(element, mousePos.X - element.RenderSize.Width / 2);
                    InkCanvas.SetTop(element, mousePos.Y - element.RenderSize.Height / 2);
                }
            }
        }

        private void CloseBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void AddBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //从剪切板中读取图像
            if (Clipboard.ContainsImage())
            {
                Border img = new();
                int offset = ink.Children.Count * 20+100;
                InkCanvas.SetTop(img, offset);
                InkCanvas.SetLeft(img, offset);
                img.IsHitTestVisible = true;
                var image = Clipboard.GetImage();
                img.Background =new ImageBrush(Clipboard.GetImage());
                //img高宽减半：
                img.Width =image.Width / 2;
                img.Height = image.Height / 2;
                ink.Children.Add(img);
            }
            else if(Clipboard.ContainsText())
            {
                Border txt = new();
                int offset = ink.Children.Count * 20 + 100;
                InkCanvas.SetTop(txt, offset);
                InkCanvas.SetLeft(txt, offset);
                txt.IsHitTestVisible = true;
                txt.SetResourceReference(BackgroundProperty, "BackgroundColor");
                var t= new TextBlock()
                {
                    Text = Clipboard.GetText(),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16,
                    Padding = new Thickness(10)
                };
                t.SetResourceReference(ForegroundProperty, "ForeColor");
                txt.Child = t;
                ink.Children.Add(txt);
            }
        }

        private void SwitchBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Drawboard_Penetrate();
        }

        private void ClearBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Drawboard_ClearAll();
        }
    }
}
