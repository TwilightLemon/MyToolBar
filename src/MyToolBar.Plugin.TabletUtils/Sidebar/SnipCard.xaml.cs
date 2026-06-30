using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MyToolBar.Plugin.TabletUtils.Models;
using MyToolBar.Plugin.TabletUtils.Services;

namespace MyToolBar.Plugin.TabletUtils.Sidebar;

/// <summary>
/// 速记卡片控件 — 展示单条 SnipItem
/// </summary>
public partial class SnipCard : UserControl
{
    private SnipItem? _item;
    private bool _isExpanded;
    private bool _isTrashMode;

    public SnipCard()
    {
        InitializeComponent();
        MouseLeftButtonDown += OnCardClicked;
        DeleteBtn.Click += OnActionBtnClicked;
        MouseRightButtonDown += OnCardRightClicked;
    }

    /// <summary>
    /// 设置卡片数据
    /// </summary>
    public void SetItem(SnipItem item)
    {
        _item = item;
        _isExpanded = false;

        bool isText = item.Type == SnipType.Text;
        TextContent.Visibility = isText ? Visibility.Visible : Visibility.Collapsed;
        ImageContent.Visibility = isText ? Visibility.Collapsed : Visibility.Visible;

        // 标签相关
        bool hasTag = item.Tag != null;
        if (hasTag)
        {
            TagStrip.Visibility = Visibility.Visible;
            TagDot.Visibility = Visibility.Visible;
            TagNameLabel.Visibility = Visibility.Visible;
            TagNameLabel.Text = item.Tag!.Name;

            try
            {
                var hex = item.Tag.Color.TrimStart('#');
                byte r = System.Convert.ToByte(hex[..2], 16);
                byte g = System.Convert.ToByte(hex[2..4], 16);
                byte b = System.Convert.ToByte(hex[4..6], 16);
                var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                TagStrip.Background = brush;
                TagDot.Fill = brush;
            }
            catch { }
        }

        // 时间
        TimeLabel.Text = item.TimeDisplay;

        // 文本内容
        if (isText)
        {
            PreviewText.Text = item.PreviewText;
            FullText.Text = item.Content;
        }
        // 图片内容
        else if (!string.IsNullOrEmpty(item.ThumbnailPath))
        {
            var fullPath = Path.Combine(SnipRepository.DataDir, item.ThumbnailPath);
            if (File.Exists(fullPath))
            {
                ThumbnailImage.Source = new BitmapImage(new Uri(fullPath));
            }
        }

        UpdateExpandState();
    }

    /// <summary>
    /// 卡片点击处理
    /// </summary>
    private void OnCardClicked(object sender, MouseButtonEventArgs e)
    {
        if (_item == null) return;

        if (_item.Type == SnipType.Text)
        {
            ToggleExpand();
        }
        else if (_item.Type == SnipType.Image && !string.IsNullOrEmpty(_item.ImagePath))
        {
            SnipboardService.OpenImageWithSystem(_item.ImagePath);
        }
    }

    private void ToggleExpand()
    {
        if (_item?.Type != SnipType.Text) return;
        _isExpanded = !_isExpanded;
        UpdateExpandState();
    }

    private void UpdateExpandState()
    {
        if (_item?.Type != SnipType.Text) return;

        bool isTruncated = (_item.Content?.Length ?? 0) > 200;

        if (_isExpanded)
        {
            PreviewText.Visibility = Visibility.Collapsed;
            FullText.Visibility = Visibility.Visible;
            CardRoot.MaxHeight = double.PositiveInfinity;
            ExpandHint.Text = "点击收起";
            ExpandHint.Visibility = Visibility.Visible;
        }
        else
        {
            PreviewText.Visibility = Visibility.Visible;
            FullText.Visibility = Visibility.Collapsed;
            CardRoot.MaxHeight = 160;

            if (isTruncated)
            {
                ExpandHint.Text = "点击查看全文";
                ExpandHint.Visibility = Visibility.Visible;
            }
            else
            {
                ExpandHint.Visibility = Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// 设置回收站模式（更改按钮行为和图标）
    /// </summary>
    public void SetTrashMode(bool isTrash)
    {
        _isTrashMode = isTrash;
        var icon = DeleteBtn.Content as System.Windows.Shapes.Path;
        if (icon != null)
        {
            // 切换图标：X ↔ 恢复箭头
            if (isTrash)
            {
                icon.Data = Geometry.Parse(
                    "M4,12 L10,6 M4,12 L10,18 M4,12 L20,12");
                icon.StrokeThickness = 1.8;
                DeleteBtn.ToolTip = "恢复";
            }
            else
            {
                icon.Data = Geometry.Parse("M6,6 L18,18 M18,6 L6,18");
                icon.StrokeThickness = 1.5;
                DeleteBtn.ToolTip = "删除";
            }
        }
    }

    private void OnActionBtnClicked(object sender, RoutedEventArgs e)
    {
        if (_item == null) return;

        if (_isTrashMode)
            RestoreRequested?.Invoke(this, _item);
        else
            DeleteRequested?.Invoke(this, _item);

        e.Handled = true;
    }

    private void OnCardRightClicked(object sender, MouseButtonEventArgs e)
    {
        if (_item == null) return;
        ContextMenuRequested?.Invoke(this, _item);
        e.Handled = true;
    }

    #region 鼠标悬停效果

    private void CardRoot_MouseEnter(object sender, MouseEventArgs e)
    {
        AnimateDeleteBtn(1);
    }

    private void CardRoot_MouseLeave(object sender, MouseEventArgs e)
    {
        AnimateDeleteBtn(0);
    }

    private void AnimateDeleteBtn(double toOpacity)
    {
        var da = new DoubleAnimation(toOpacity, TimeSpan.FromMilliseconds(150));
        DeleteBtn.BeginAnimation(OpacityProperty, da);
    }

    #endregion

    #region 事件

    public event EventHandler<SnipItem>? DeleteRequested;
    public event EventHandler<SnipItem>? RestoreRequested;
    public event EventHandler<SnipItem>? ContextMenuRequested;

    #endregion
}
