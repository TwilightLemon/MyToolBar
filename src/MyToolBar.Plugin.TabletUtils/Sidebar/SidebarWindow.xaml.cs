using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MyToolBar.Common;
using MyToolBar.Common.WinAPI;
using MyToolBar.Plugin.TabletUtils.Models;
using MyToolBar.Plugin.TabletUtils.Services;
using Button = EleCho.WpfSuite.Controls.Button;

namespace MyToolBar.Plugin.TabletUtils.Sidebar;

/// <summary>
/// 速记抽屉主窗口
/// </summary>
public partial class SidebarWindow : Window
{
    private readonly SnipRepository _repo;
    private readonly SnipboardService _snipService;
    private readonly TagService _tagService;

    private DateTime _selectedDate = DateTime.Today;
    private bool _isTrashMode;
    private int? _newItemTagId;
    private List<Tag> _allTags = [];
    private List<int> _activeTagFilterIds = [];
    private IntPtr _hwnd;

    public SidebarWindow()
    {
        InitializeComponent();

        _repo = new SnipRepository();
        _snipService = new SnipboardService(_repo);
        _tagService = new TagService(_repo);

        Height = SystemParameters.WorkArea.Height - 12;
        Deactivated += SideWindow_Deactivated;
        Activated += SideWindow_Activated;
        Loaded += SidebarWindow_Loaded;

        // 搜索框防抖
        SearchBox.TextChanged += async (s, e) =>
        {
            await Task.Delay(300);
            if (SearchBox.IsFocused)
                await RefreshCards();
        };

        // 输入框：回车发送（Shift+Enter 换行）
        InputBox.PreviewKeyDown += InputBox_PreviewKeyDown;

        // 拖放支持
        AllowDrop = true;
        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        Drop += OnDrop;
    }

    #region 窗口生命周期

    private async void SidebarWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        await LoadTags();
        await RefreshCards();
    }

    private void SideWindow_Activated(object? sender, EventArgs e)
    {
        if (FixTb.IsChecked == true) return;
        Height = ScreenAPI.GetScreenArea(_hwnd).Height - 64;
        var da = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(300));
        da.EasingFunction = new CircleEase();
        BeginAnimation(LeftProperty, da);
    }

    private void SideWindow_Deactivated(object? sender, EventArgs e)
    {
        if (FixTb.IsChecked == true) return;
        var da = new DoubleAnimation(-ActualWidth, TimeSpan.FromMilliseconds(300));
        da.EasingFunction = new CircleEase();
        da.Completed += delegate { Hide(); };
        BeginAnimation(LeftProperty, da);
    }

    #endregion

    #region 数据加载

    private async Task LoadTags()
    {
        _allTags = await _tagService.GetAllTags();
        BuildTagFilterPanel();
    }

    /// <summary>
    /// 刷新卡片列表
    /// </summary>
    private async Task RefreshCards()
    {
        CardContainer.Children.Clear();

        List<SnipItem> items;
        if (_isTrashMode)
        {
            items = await _snipService.GetTrashItems();
        }
        else if (!string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            items = await _snipService.Search(SearchBox.Text.Trim());
            // 如果还有标签筛选，二次过滤
            if (_activeTagFilterIds.Count > 0)
                items = items.Where(i => i.TagId != null && _activeTagFilterIds.Contains(i.TagId.Value)).ToList();
        }
        else if (_activeTagFilterIds.Count > 0)
        {
            // 多标签筛选：取并集
            items = new List<SnipItem>();
            foreach (var tagId in _activeTagFilterIds)
            {
                items.AddRange(await _snipService.GetByTag(tagId));
            }
            items = items.DistinctBy(i => i.Id).OrderByDescending(i => i.CreatedAt).ToList();
        }
        else
        {
            items = _selectedDate == DateTime.Today
                ? await _snipService.GetTodayItems()
                : await _snipService.GetByDate(_selectedDate);
        }

        // 渲染卡片
        foreach (var item in items)
        {
            var card = CreateCard(item);
            CardContainer.Children.Add(card);
        }

        // 空状态
        bool isEmpty = CardContainer.Children.Count == 0;
        EmptyHint.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        ContentScroller.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;

        // 回收站模式
        TrashToolbar.Visibility = _isTrashMode ? Visibility.Visible : Visibility.Collapsed;
        BackToTodayBtn.Visibility = (!_isTrashMode && _selectedDate != DateTime.Today)
            ? Visibility.Visible : Visibility.Collapsed;

        // 更新空状态文字
        if (_isTrashMode)
            EmptyHint.Text = "🗑️ 回收站是空的";
        else if (_selectedDate != DateTime.Today)
            EmptyHint.Text = $"📝 {_selectedDate:yyyy/MM/dd} 没有记录";
        else
            EmptyHint.Text = "📝 今天还没有记录，输入内容开始吧~";
    }

    /// <summary>
    /// 创建单张卡片
    /// </summary>
    private SnipCard CreateCard(SnipItem item)
    {
        var card = new SnipCard();
        card.SetItem(item);
        card.SetTrashMode(_isTrashMode);
        card.DeleteRequested += OnCardDeleteRequested;
        card.RestoreRequested += OnCardRestoreRequested;
        card.ContextMenuRequested += OnCardContextMenuRequested;
        return card;
    }

    #endregion

    #region 事件处理 — 卡片操作

    private async void OnCardDeleteRequested(object? sender, SnipItem item)
    {
        if (_isTrashMode)
        {
            // 回收站中：硬删除
            var result = MessageBox.Show("确定要永久删除这条记录吗？此操作不可撤销。",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _snipService.HardDelete(item.Id);
                await RefreshCards();
            }
        }
        else
        {
            // 正常模式：软删除
            await _snipService.SoftDelete(item.Id);
            await RefreshCards();
        }
    }

    private async void OnCardRestoreRequested(object? sender, SnipItem item)
    {
        await _snipService.Restore(item.Id);
        await RefreshCards();
    }

    private void OnCardContextMenuRequested(object? sender, SnipItem item)
    {
        if (_isTrashMode) return; // 回收站不显示标签菜单

        var menu = new ContextMenu();

        // 标签子菜单
        var tagMenu = new MenuItem { Header = "设置标签" };
        foreach (var tag in _allTags)
        {
            var tagItem = new MenuItem { Header = $"● {tag.Name}", Tag = tag };
            try
            {
                var hex = tag.Color.TrimStart('#');
                byte r = System.Convert.ToByte(hex[..2], 16);
                byte g = System.Convert.ToByte(hex[2..4], 16);
                byte b = System.Convert.ToByte(hex[4..6], 16);
                tagItem.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch { }
            tagItem.Click += async (s, args) =>
            {
                await _snipService.SetTag(item.Id, tag.Id);
                await RefreshCards();
            };
            tagMenu.Items.Add(tagItem);
        }
        tagMenu.Items.Add(new Separator());
        var clearTagItem = new MenuItem { Header = "清除标签" };
        clearTagItem.Click += async (s, args) =>
        {
            await _snipService.SetTag(item.Id, null);
            await RefreshCards();
        };
        tagMenu.Items.Add(clearTagItem);
        menu.Items.Add(tagMenu);

        // 复制文本
        if (item.Type == SnipType.Text && !string.IsNullOrEmpty(item.Content))
        {
            var copyItem = new MenuItem { Header = "复制文本" };
            copyItem.Click += (s, args) => Clipboard.SetText(item.Content);
            menu.Items.Add(copyItem);
        }

        // 复制图片
        if (item.Type == SnipType.Image && !string.IsNullOrEmpty(item.ImagePath))
        {
            var copyImgItem = new MenuItem { Header = "复制图片" };
            copyImgItem.Click += (s, args) =>
            {
                var fullPath = Path.Combine(SnipRepository.DataDir, item.ImagePath);
                if (File.Exists(fullPath))
                {
                    var bmp = new BitmapImage(new Uri(fullPath));
                    Clipboard.SetImage(bmp);
                }
            };
            menu.Items.Add(copyImgItem);
        }

        menu.IsOpen = true;
    }

    #endregion

    #region 事件处理 — 发送/输入

    private async void SendBtn_Click(object sender, RoutedEventArgs e)
    {
        await SendText();
    }

    private async void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
        {
            e.Handled = true;
            await SendText();
        }
        // Ctrl+V 粘贴图片
        else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (Clipboard.ContainsImage())
            {
                e.Handled = true;
                await PasteImage();
            }
        }
    }

    private async Task SendText()
    {
        var text = InputBox.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        await _snipService.SaveText(text, _newItemTagId);
        InputBox.Text = "";
        await RefreshCards();
    }

    private async Task PasteImage()
    {
        try
        {
            var item = await _snipService.SaveImageFromClipboard(_newItemTagId);
            await RefreshCards();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"粘贴图片失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region 事件处理 — 拖放

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
            e.Data.GetDataPresent(DataFormats.Text) ||
            e.Data.GetDataPresent(DataFormats.UnicodeText))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        // 持续声明 Copy 语义，防止源应用在 DragOver 期间切换为 Move
        if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
            e.Data.GetDataPresent(DataFormats.Text) ||
            e.Data.GetDataPresent(DataFormats.UnicodeText))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        // 1) 拖入文本内容（直接从其他应用拖选文字）
        if (e.Data.GetDataPresent(DataFormats.UnicodeText))
        {
            var text = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _snipService.SaveText(text.Trim(), _newItemTagId);
                await RefreshCards();
                return;
            }
        }
        if (e.Data.GetDataPresent(DataFormats.Text))
        {
            var text = e.Data.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _snipService.SaveText(text.Trim(), _newItemTagId);
                await RefreshCards();
                return;
            }
        }

        // 2) 拖入文件
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var imageExts = new HashSet<string> { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".tiff", ".ico" };
            var textExts = new HashSet<string> { ".txt", ".md", ".csv", ".log", ".json", ".xml", ".cs", ".html", ".css", ".js", ".py", ".ini", ".cfg", ".yaml", ".yml" };

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (imageExts.Contains(ext))
                {
                    try
                    {
                        await _snipService.SaveImageFile(file, _newItemTagId);
                    }
                    catch { }
                }
                else if (textExts.Contains(ext))
                {
                    try
                    {
                        var text = await File.ReadAllTextAsync(file);
                        if (!string.IsNullOrWhiteSpace(text))
                            await _snipService.SaveText(text.Trim(), _newItemTagId);
                    }
                    catch { }
                }
            }
            await RefreshCards();
        }
    }

    #endregion

    #region 事件处理 — 新条目标签

    private void NewItemTagBadge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 显示标签选择上下文菜单
        var menu = new ContextMenu();
        foreach (var tag in _allTags)
        {
            var item = new MenuItem { Header = $"● {tag.Name}", Tag = tag };
            try
            {
                var hex = tag.Color.TrimStart('#');
                byte r = System.Convert.ToByte(hex[..2], 16);
                byte g = System.Convert.ToByte(hex[2..4], 16);
                byte b = System.Convert.ToByte(hex[4..6], 16);
                item.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch { }
            item.Click += (s, args) => SetNewItemTag(tag);
            menu.Items.Add(item);
        }

        // 清除标签选项
        menu.Items.Add(new Separator());
        var clearItem = new MenuItem { Header = "清除标签" };
        clearItem.Click += (s, args) => ClearNewItemTag();
        menu.Items.Add(clearItem);

        menu.IsOpen = true;
        e.Handled = true;
    }

    private void ClearNewItemTag()
    {
        _newItemTagId = null;
    }

    private void SetNewItemTag(Tag tag)
    {
        _newItemTagId = tag.Id;
        NewItemTagLabel.Text = tag.Name;

        try
        {
            var hex = tag.Color.TrimStart('#');
            byte r = System.Convert.ToByte(hex[..2], 16);
            byte g = System.Convert.ToByte(hex[2..4], 16);
            byte b = System.Convert.ToByte(hex[4..6], 16);
            NewItemTagDot.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
            NewItemTagBadge.Background = new SolidColorBrush(Color.FromArgb(40, r, g, b));
        }
        catch { }
    }

    #endregion

    #region 事件处理 — 工具栏

    private async void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_isTrashMode) return;

        var result = MessageBox.Show("确定要将今天的所有记录移入回收站吗？（不受标签筛选影响）",
            "确认清除", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            await _snipService.ClearToday();
            await RefreshCards();
        }
    }

    private async void DatePickerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_isTrashMode) return;

        // 使用简单的日期选择对话框
        var dialog = new DatePickerDialog(_selectedDate);
        dialog.Owner = this;
        if (dialog.ShowDialog() == true)
        {
            _selectedDate = dialog.SelectedDate.Date;
            await RefreshCards();
        }
    }

    private async void BackToTodayBtn_Click(object sender, RoutedEventArgs e)
    {
        _selectedDate = DateTime.Today;
        await RefreshCards();
    }

    #endregion

    #region 事件处理 — 回收站

    private async void TrashBtn_Click(object sender, RoutedEventArgs e)
    {
        _isTrashMode = !_isTrashMode;
        if (!_isTrashMode)
        {
            _selectedDate = DateTime.Today;
            SearchBox.Text = "";
            _activeTagFilterIds.Clear();
            UpdateTagFilterVisual();
        }
        await RefreshCards();
    }

    private async void EmptyTrashBtn_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("确定要永久删除回收站中的所有记录吗？此操作不可撤销。",
            "清空回收站", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            await _snipService.EmptyTrash();
            await RefreshCards();
        }
    }

    private async void ExitTrashBtn_Click(object sender, RoutedEventArgs e)
    {
        _isTrashMode = false;
        _selectedDate = DateTime.Today;
        await RefreshCards();
    }

    #endregion

    #region 标签筛选浮窗

    private void BuildTagFilterPanel()
    {
        TagFilterContainer.Children.Clear();
        var buttonStyle = FindResource("SimpleButtonStyleForWs") as Style;
        // "全部" 按钮 — 重置标签筛选
        var allBtn = new Button
        {
            Content = "全部",
            Background = Brushes.Transparent,
            CornerRadius=new CornerRadius(12),
            BorderThickness = new Thickness(0, 0, 0, 0),
            Foreground = (SolidColorBrush)FindResource("ForeColor"),
            Cursor = Cursors.Hand,
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FontWeight = FontWeights.SemiBold,
            Style=buttonStyle
        };
        allBtn.Click += (s, e) =>
        {
            _activeTagFilterIds.Clear();
            UpdateTagFilterVisual();
            _ = RefreshCards();
        };
        TagFilterContainer.Children.Add(allBtn);

        foreach (var tag in _allTags)
        {
            var btn = new Button
            {
                Content = $"● {tag.Name}",
                Tag = tag,
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(0, 0, 0, 0),
                Foreground = (SolidColorBrush)FindResource("ForeColor"),
                Cursor = Cursors.Hand,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Style = buttonStyle
            };

            // 设置标签颜色圆点
            try
            {
                var hex = tag.Color.TrimStart('#');
                byte r = System.Convert.ToByte(hex[..2], 16);
                byte g = System.Convert.ToByte(hex[2..4], 16);
                byte b = System.Convert.ToByte(hex[4..6], 16);
                btn.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch { }

            btn.Click += (s, e) =>
            {
                ToggleTagFilter(tag.Id);
            };

            TagFilterContainer.Children.Add(btn);
        }
    }

    private void ToggleTagFilter(int tagId)
    {
        if (_activeTagFilterIds.Contains(tagId))
            _activeTagFilterIds.Remove(tagId);
        else
            _activeTagFilterIds.Add(tagId);

        UpdateTagFilterVisual();
        _ = RefreshCards();
    }

    private void UpdateTagFilterVisual()
    {
        bool anyActive = _activeTagFilterIds.Count > 0;

        foreach (Button btn in TagFilterContainer.Children)
        {
            var tag = btn.Tag as Tag;
            if (tag == null)
            {
                // "全部" 按钮：无筛选时高亮
                btn.Opacity = anyActive ? 0.5 : 1;
                continue;
            }

            btn.Opacity = _activeTagFilterIds.Contains(tag.Id) ? 1 : 0.5;
        }
    }

    #endregion
}
