namespace MyToolBar.Plugin;

/// <summary>
/// 标记一个类为插件设置配置 POCO 类型。
/// 该类型需具备无参构造函数。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SettingsConfigAttribute : Attribute
{
    /// <summary>
    /// 设置组显示名称。以 "$" 开头标识资源键，通过 ResourceManagerType 解析；
    /// 否则为直接显示文本。
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 设置组描述文本。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 多语言 ResourceManager 所在类型。
    /// 该类型需暴露 <c>public static ResourceManager _rm</c> 字段。
    /// </summary>
    public Type? ResourceManagerType { get; set; }
}

/// <summary>
/// 标记属性为设置项，提供元数据供主程序生成类型感知的设置 UI。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SettingsFieldAttribute : Attribute
{
    /// <summary>
    /// 显示名称。以 "$" 开头标识资源键，通过 <see cref="SettingsConfigAttribute.ResourceManagerType"/> 解析；
    /// 否则为直接显示文本。
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 描述文本，在 UI 中作为 Tooltip 显示。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 帮助链接 URL，UI 中显示为可点击的问号图标按钮。
    /// </summary>
    public string? HelpUrl { get; set; }

    /// <summary>
    /// 占位符文本（输入框为空时显示）。
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// 是否为密码字段。为 true 时 UI 使用 PasswordBox 掩码显示。
    /// </summary>
    public bool IsPassword { get; set; }

    /// <summary>
    /// 是否必填。为 true 时保存前验证非空。
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 分类标签。UI 中按此字段分组显示。
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 排序权重，值越小越靠前（默认 0）。
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 数值类型的最小值（仅对整数和浮点数类型有效）。
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// 数值类型的最大值（仅对整数和浮点数类型有效）。
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// 正则表达式校验模式。
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// 校验失败时的提示文本。
    /// </summary>
    public string? ValidationMessage { get; set; }
}
