using System.ComponentModel;

namespace MyToolBar.Plugin.TabletUtils.Models;

/// <summary>
/// 速记内容类型
/// </summary>
public enum SnipType
{
    Text = 0,
    Image = 1
}

/// <summary>
/// 速记条目实体
/// </summary>
public class SnipItem : INotifyPropertyChanged
{
    private int _id;
    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    public SnipType Type { get; set; }

    private string? _content;
    public string? Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(nameof(Content)); OnPropertyChanged(nameof(PreviewText)); }
    }

    private string? _imagePath;
    public string? ImagePath
    {
        get => _imagePath;
        set { _imagePath = value; OnPropertyChanged(nameof(ImagePath)); }
    }

    private string? _thumbnailPath;
    public string? ThumbnailPath
    {
        get => _thumbnailPath;
        set { _thumbnailPath = value; OnPropertyChanged(nameof(ThumbnailPath)); }
    }

    private int? _tagId;
    public int? TagId
    {
        get => _tagId;
        set { _tagId = value; OnPropertyChanged(nameof(TagId)); OnPropertyChanged(nameof(HasTag)); }
    }

    /// <summary>
    /// 关联的标签对象（通过 join 查询填充）
    /// </summary>
    public Tag? Tag { get; set; }

    public DateTime CreatedAt { get; set; }

    private DateTime? _deletedAt;
    public DateTime? DeletedAt
    {
        get => _deletedAt;
        set { _deletedAt = value; OnPropertyChanged(nameof(DeletedAt)); OnPropertyChanged(nameof(IsDeleted)); }
    }

    public string? SourceApp { get; set; }

    // ===== 计算属性（非数据库字段） =====

    public bool IsDeleted => DeletedAt.HasValue;
    public bool HasTag => TagId.HasValue;

    /// <summary>
    /// 卡片时间显示 "HH:mm"
    /// </summary>
    public string TimeDisplay => CreatedAt.ToString("HH:mm");

    /// <summary>
    /// 预览文本：截断到 200 字符
    /// </summary>
    public string PreviewText
    {
        get
        {
            if (string.IsNullOrEmpty(Content)) return "";
            return Content.Length > 200 ? Content[..200] + "..." : Content;
        }
    }

    /// <summary>
    /// 日期分组键："今天"/"昨天"/"yyyy/MM/dd"
    /// </summary>
    public string DateGroupKey
    {
        get
        {
            var today = DateTime.Today;
            var date = CreatedAt.Date;
            if (date == today) return "今天";
            if (date == today.AddDays(-1)) return "昨天";
            return date.ToString("yyyy/MM/dd");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
