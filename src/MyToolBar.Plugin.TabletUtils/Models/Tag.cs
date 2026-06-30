using System.ComponentModel;

namespace MyToolBar.Plugin.TabletUtils.Models;

/// <summary>
/// 标签实体
/// </summary>
public class Tag : INotifyPropertyChanged
{
    private int _id;
    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    private string _color = "#808080";
    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(nameof(Color)); }
    }

    private int _sortOrder;
    public int SortOrder
    {
        get => _sortOrder;
        set { _sortOrder = value; OnPropertyChanged(nameof(SortOrder)); }
    }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 该标签下的条目数量（查询时填充，非数据库字段）
    /// </summary>
    public int? ItemCount { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
