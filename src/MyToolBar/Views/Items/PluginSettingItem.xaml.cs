using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using EleCho.WpfSuite.Controls;
using System.Windows.Input;
using MyToolBar.Common;
using MyToolBar.Plugin;
using Border = System.Windows.Controls.Border;
using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
using ComboBox = EleCho.WpfSuite.Controls.ComboBox;
using ComboBoxItem = EleCho.WpfSuite.Controls.ComboBoxItem;
using TextBox = EleCho.WpfSuite.Controls.TextBox;
using PasswordBox = EleCho.WpfSuite.Controls.PasswordBox;
using Button = EleCho.WpfSuite.Controls.Button;

namespace MyToolBar.Views.Items
{
    /// <summary>
    /// PluginSettingItem.xaml 的交互逻辑。
    /// 支持两种模式：
    /// 1. 新方式 (SettingsTypes) — 通过反射和 Attribute 生成类型感知的设置 UI
    /// 2. 旧方式 (SettingsSignKeys) — 通过解析 JSON 结构生成简单 TextBox UI（兼容）
    /// </summary>
    public partial class PluginSettingItem : UserControl
    {
        private readonly IPlugin _plugin;
        private readonly string? _packageName;
        private readonly List<SettingsCard> _cards = [];

        public PluginSettingItem(IPlugin plugin, string? packageName = null)
        {
            InitializeComponent();
            _plugin = plugin;
            _packageName = packageName;
            Loaded += PluginSettingItem_Loaded;
            Unloaded += PluginSettingItem_Unloaded;
        }

        // ==================== Unload / Save ====================

        /// <summary>
        /// 显式保存所有已修改的设置卡片（供 Apply 按钮调用）。
        /// </summary>
        public async System.Threading.Tasks.Task SaveAsync()
        {
            // 新方式：保存所有脏卡片
            foreach (var card in _cards.Where(c => c.Dirty))
            {
                await SaveCardToJson(card);
            }
            // 旧方式兼容：保存旧版 JSON 变更
            SaveLegacyChanges();
        }

        private async void PluginSettingItem_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unload 时作为安全兜底保存
            await SaveAsync();
        }

        // ==================== Main Entry ====================

        private void PluginSettingItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (_plugin.SettingsTypes is { Length: > 0 } types)
            {
                foreach (var settingsType in types)
                    GenerateTypeBasedUI(settingsType);
            }
#pragma warning disable CS0618 // 兼容旧方式
            else if (_plugin.SettingsSignKeys is { Count: > 0 } signKeys)
            {
                GenerateLegacyUI(signKeys);
            }
#pragma warning restore CS0618
        }

        // ======================================================
        //  新方式：Type 反射 + Attribute 驱动的 UI 生成
        // ======================================================

        private void GenerateTypeBasedUI(Type settingsType)
        {
            // 读取类级别 Attribute
            var configAttr = settingsType.GetCustomAttribute<SettingsConfigAttribute>();
            string asmName = settingsType.Assembly.GetName().Name!;
            string sign = $"{asmName}.{settingsType.Name}";
            string pkgName = _packageName ?? configAttr?.DisplayName ?? settingsType.Name;
            string filePath = Settings.GetPathBySign(sign, Settings.sType.Settings);

            // 解析类级别显示名称
            string title = ResolveString(configAttr?.DisplayName ?? settingsType.Name,
                                         configAttr?.ResourceManagerType);

            // 通过反射遍历 [SettingsField] 属性
            var props = settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<SettingsFieldAttribute>() != null)
                .Select(p => (Property: p, Attr: p.GetCustomAttribute<SettingsFieldAttribute>()!))
                .OrderBy(p => p.Attr.Category ?? "")
                .ThenBy(p => p.Attr.Order)
                .ToList();

            if (props.Count == 0) return;

            // 创建默认实例
            object instance;
            try { instance = Activator.CreateInstance(settingsType)!; }
            catch { return; } // 无参构造函数不可用则跳过

            // 从 JSON 加载已有值（若存在）
            var originalValues = TryLoadJsonValues(filePath, settingsType, instance);

            // 构建 SettingsCard
            var card = new SettingsCard
            {
                SettingsType = settingsType,
                Sign = sign,
                PackageName = pkgName,
                FilePath = filePath,
                Instance = instance,
                ConfigAttr = configAttr,
                Fields = new List<FieldEntry>(props.Count)
            };

            // === 生成 UI ===

            // 卡片边框
            var border = new Border
            {
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(20)
            };
            border.SetResourceReference(BackgroundProperty, "MaskColor");

            var grid = new Grid();
            border.Child = grid;

            // 列定义：标签 : 控件 ≈ 1 : 4
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });

            // 标题行
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            var titleTb = new TextBlock
            {
                Text = title,
                FontSize = 20,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            grid.Children.Add(titleTb);
            Grid.SetColumnSpan(titleTb, 2);

            // TODO: 按 Category 分组显示 GroupBox
            // 当前简化实现：直接按 Order 排序，内联显示 Category 分隔
            string? currentCategory = null;
            int row = 1;

            foreach (var (prop, attr) in props)
            {
                // 分类分隔
                if (attr.Category != currentCategory)
                {
                    currentCategory = attr.Category;
                    if (currentCategory != null)
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                        var catTb = new TextBlock
                        {
                            Text = ResolveString(currentCategory, configAttr?.ResourceManagerType),
                            FontSize = 14,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(20, 12, 10, 4),
                            Opacity = 0.7
                        };
                        grid.Children.Add(catTb);
                        Grid.SetRow(catTb, row);
                        Grid.SetColumnSpan(catTb, 2);
                        row++;
                    }
                }

                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                // 标签
                string labelText = ResolveString(attr.DisplayName ?? prop.Name, configAttr?.ResourceManagerType);
                var labelPanel = CreateLabelPanel(labelText, attr, configAttr?.ResourceManagerType);
                grid.Children.Add(labelPanel);
                Grid.SetRow(labelPanel, row);
                Grid.SetColumn(labelPanel, 0);

                // 值控件
                object propValue = prop.GetValue(instance)!;
                var control = CreateControlForProperty(prop, attr, propValue);

                // 监听值变化
                AttachChangeHandler(control, prop, card);

                grid.Children.Add(control);
                Grid.SetRow(control, row);
                Grid.SetColumn(control, 1);

                card.Fields.Add(new FieldEntry
                {
                    Property = prop,
                    Attribute = attr,
                    Control = control
                });

                row++;
            }

            SettingItemList.Children.Add(border);
            _cards.Add(card);
        }

        // ---- 控件工厂 ----

        private static FrameworkElement CreateControlForProperty(
            PropertyInfo prop, SettingsFieldAttribute attr, object? currentValue)
        {
            var propType = prop.PropertyType;
            // 处理 Nullable<T>
            var underlyingType = Nullable.GetUnderlyingType(propType);
            var actualType = underlyingType ?? propType;

            // bool → ToggleButton
            if (actualType == typeof(bool))
            {
                var toggle = new ToggleButton
                {
                    IsChecked = currentValue is true,
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                toggle.SetResourceReference(StyleProperty, "RoundToggleButtonStyle");
                return toggle;
            }

            // Enum → ComboBox
            if (actualType.IsEnum)
            {
                var values = Enum.GetValues(actualType);
                var combo = new ComboBox
                {
                    Margin = new Thickness(10),
                    Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    SelectedValue = currentValue
                };
                foreach (var v in values)
                {
                    // 尝试用 DescriptionAttribute 作为显示文本
                    var desc = actualType.GetField(v.ToString()!)
                        ?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                    combo.Items.Add(new ComboBoxItem
                    {
                        Content = desc ?? v.ToString(),
                        Tag = v
                    });
                }
                // 选中当前值
                foreach (ComboBoxItem item in combo.Items)
                {
                    if (Equals(item.Tag, currentValue))
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
                combo.SetResourceReference(StyleProperty, "SimpleComboBoxStyleForWs");
                combo.SetResourceReference(BackgroundProperty, "MaskColor");
                combo.SetResourceReference(ForegroundProperty, "ForeColor");
                return combo;
            }

            // 整数类型
            if (actualType == typeof(int) || actualType == typeof(long) ||
                actualType == typeof(short) || actualType == typeof(byte))
            {
                var tb = new TextBox
                {
                    Text = currentValue?.ToString() ?? "",
                    Height = 30,
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                };
                tb.SetResourceReference(StyleProperty, "SimpleTextBoxStyle");
                tb.SetResourceReference(BackgroundProperty, "MaskColor");
                tb.SetResourceReference(ForegroundProperty, "ForeColor");
                tb.PreviewTextInput += (s, e) =>
                {
                    e.Handled = !int.TryParse(e.Text, out _) &&
                                e.Text != "-"; // 允许负号
                };
                tb.LostFocus += (s, e) =>
                {
                    if (attr.MinValue.HasValue || attr.MaxValue.HasValue)
                    {
                        if (double.TryParse(tb.Text, out var val))
                        {
                            if (attr.MinValue.HasValue && val < attr.MinValue.Value)
                                tb.Text = attr.MinValue.Value.ToString();
                            if (attr.MaxValue.HasValue && val > attr.MaxValue.Value)
                                tb.Text = attr.MaxValue.Value.ToString();
                        }
                    }
                };
                return tb;
            }

            // 浮点类型
            if (actualType == typeof(double) || actualType == typeof(float) ||
                actualType == typeof(decimal))
            {
                var tb = new TextBox
                {
                    Text = currentValue?.ToString() ?? "",
                    Height = 30,
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                };
                tb.SetResourceReference(StyleProperty, "SimpleTextBoxStyle");
                tb.SetResourceReference(BackgroundProperty, "MaskColor");
                tb.SetResourceReference(ForegroundProperty, "ForeColor");
                tb.PreviewTextInput += (s, e) =>
                {
                    e.Handled = !char.IsDigit(e.Text[0]) &&
                                e.Text != "." && e.Text != "-";
                };
                tb.LostFocus += (s, e) =>
                {
                    if (attr.MinValue.HasValue || attr.MaxValue.HasValue)
                    {
                        if (double.TryParse(tb.Text, out var val))
                        {
                            if (attr.MinValue.HasValue && val < attr.MinValue.Value)
                                tb.Text = attr.MinValue.Value.ToString();
                            if (attr.MaxValue.HasValue && val > attr.MaxValue.Value)
                                tb.Text = attr.MaxValue.Value.ToString();
                        }
                    }
                };
                return tb;
            }

            // string (IsPassword) → PasswordBox
            if (attr.IsPassword)
            {
                var pwdBox = new PasswordBox
                {
                    Password = currentValue?.ToString() ?? "",
                    Padding=new Thickness(6,6,0,0),
                    Height = 30,
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                };
                if (!string.IsNullOrEmpty(attr.Placeholder))
                {
                    // 使用 Tag 存储 placeholder（PasswordBox 不支持 Placeholder）
                    pwdBox.Tag = attr.Placeholder;
                }
                pwdBox.SetResourceReference(StyleProperty, "SimplePasswordBoxStyleForWs");
                pwdBox.SetResourceReference(BackgroundProperty, "MaskColor");
                pwdBox.SetResourceReference(ForegroundProperty, "ForeColor");
                return pwdBox;
            }

            // string (default) → TextBox
            var textBox = new TextBox
            {
                Text = currentValue?.ToString() ?? "",
                Height = 30,
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center
            };
            if (!string.IsNullOrEmpty(attr.Placeholder))
            {
                textBox.Tag = attr.Placeholder;
                // 简单的 placeholder 实现（当文本框为空时显示灰色提示）
                AttachPlaceholder(textBox, attr.Placeholder);
            }
            textBox.SetResourceReference(StyleProperty, "SimpleTextBoxStyle");
            textBox.SetResourceReference(BackgroundProperty, "MaskColor");
            textBox.SetResourceReference(ForegroundProperty, "ForeColor");
            return textBox;
        }

        // ---- 标签面板 ----

        private static FrameworkElement CreateLabelPanel(
            string text, SettingsFieldAttribute attr, Type? rmType)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 10, 10, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 标签文本
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 15,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            // Tooltip
            string? desc = ResolveString(attr.Description, rmType);
            if (!string.IsNullOrEmpty(desc))
                tb.ToolTip = desc;

            panel.Children.Add(tb);

            // HelpUrl 按钮
            if (!string.IsNullOrEmpty(attr.HelpUrl))
            {
                var helpBtn = new Button
                {
                    Content = "?",
                    Width = 20,
                    Height = 20,
                    FontSize = 12,
                    Margin = new Thickness(6, 0, 0, 0),
                    Padding = new Thickness(0),
                    Tag = attr.HelpUrl
                };
                helpBtn.SetResourceReference(StyleProperty, "IconButtonStyleForWs");
                helpBtn.Click += (s, e) =>
                {
                    if (s is Button btn && btn.Tag is string url)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                };
                panel.Children.Add(helpBtn);
            }

            // 必填标记
            if (attr.IsRequired)
            {
                var reqTb = new TextBlock
                {
                    Text = "*",
                    Foreground = System.Windows.Media.Brushes.Red,
                    FontSize = 15,
                    Margin = new Thickness(4, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                panel.Children.Add(reqTb);
            }

            return panel;
        }

        // ---- 值变化监听 ----

        private void AttachChangeHandler(FrameworkElement control, PropertyInfo prop, SettingsCard card)
        {
            switch (control)
            {
                case ToggleButton toggle:
                    toggle.Checked += (_, _) => { card.Dirty = true; };
                    toggle.Unchecked += (_, _) => { card.Dirty = true; };
                    break;
                case ComboBox combo:
                    combo.SelectionChanged += (_, _) => { card.Dirty = true; };
                    break;
                case PasswordBox pwd:
                    pwd.TextChanged += (_, _) => { card.Dirty = true; };
                    break;
                case TextBox tb:
                    tb.TextChanged += (_, _) => { card.Dirty = true; };
                    break;
            }
        }

        // ---- 从控件收集值并写回实例 ----

        private static void CollectValueFromControl(FieldEntry field)
        {
            var prop = field.Property;
            var actualType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            switch (field.Control)
            {
                case ToggleButton toggle:
                    prop.SetValue(field.Card.Instance, toggle.IsChecked == true);
                    break;
                case ComboBox combo:
                    if (combo.SelectedItem is ComboBoxItem item && item.Tag != null)
                        prop.SetValue(field.Card.Instance, item.Tag);
                    break;
                case PasswordBox pwd:
                    prop.SetValue(field.Card.Instance, pwd.Password);
                    break;
                case TextBox tb:
                    var text = tb.Text;
                    try
                    {
                        var converted = Convert.ChangeType(text, actualType, CultureInfo.InvariantCulture);
                        prop.SetValue(field.Card.Instance, converted);
                    }
                    catch
                    {
                        // 转换失败时尝试直接赋值字符串（对 string 类型有效）
                        if (actualType == typeof(string))
                            prop.SetValue(field.Card.Instance, text);
                    }
                    break;
            }
        }

        // ---- JSON 读写 ----

        private static Dictionary<string, object?>? TryLoadJsonValues(
            string filePath, Type settingsType, object instance)
        {
            try
            {
                if (!File.Exists(filePath)) return null;

                string json = File.ReadAllText(filePath);
                using var doc = JsonDocument.Parse(json);
                var dataElement = doc.RootElement.GetProperty("Data");
                var saved = JsonSerializer.Deserialize(dataElement.GetRawText(), settingsType);
                if (saved == null) return null;

                // 将 JSON 中的值覆盖到实例上
                var result = new Dictionary<string, object?>();
                foreach (var prop in settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetCustomAttribute<SettingsFieldAttribute>() == null) continue;
                    try
                    {
                        var savedValue = prop.GetValue(saved);
                        result[prop.Name] = savedValue;
                        prop.SetValue(instance, savedValue);
                    }
                    catch { /* JSON 中缺少该字段则跳过 */ }
                }
                return result;
            }
            catch { return null; }
        }

        private async System.Threading.Tasks.Task SaveCardToJson(SettingsCard card)
        {
            try
            {
                // 从控件收集值
                foreach (var field in card.Fields)
                {
                    field.Card = card;
                    CollectValueFromControl(field);
                }

                var json = JsonSerializer.Serialize(
                    new
                    {
                        Sign = card.Sign,
                        PackageName = card.PackageName,
                        Data = card.Instance
                    },
                    new JsonSerializerOptions { WriteIndented = true });

                await File.WriteAllTextAsync(card.FilePath, json);
                card.Dirty = false;
            }
            catch { /* 保存失败静默处理 */ }
        }

        // ---- 资源解析 ----

        private static string ResolveString(string? text, Type? rmType)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            if (text.StartsWith('$') && rmType != null)
            {
                try
                {
                    var rmField = rmType.GetField("_rm",
                        BindingFlags.Public | BindingFlags.Static);
                    if (rmField?.GetValue(null) is ResourceManager rm)
                    {
                        var culture = CultureInfo.DefaultThreadCurrentCulture
                                      ?? CultureInfo.CurrentCulture;
                        return rm.GetString(text[1..], culture) ?? text;
                    }
                }
                catch { }
            }

            return text.StartsWith('$') ? text[1..] : text;
        }

        // ---- Placeholder 辅助 ----

        private static void AttachPlaceholder(TextBox tb, string placeholder)
        {
            bool isPlaceholder = string.IsNullOrEmpty(tb.Text);
            if (isPlaceholder)
            {
                tb.Text = placeholder;
                tb.Opacity = 0.5;
            }

            tb.GotFocus += (s, e) =>
            {
                if (tb.Opacity < 1.0)
                {
                    tb.Text = "";
                    tb.Opacity = 1.0;
                }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.Opacity = 0.5;
                }
            };
        }

        // ======================================================
        //  旧方式：JSON 解析驱动的 UI 生成（兼容）
        // ======================================================

        private record struct SettingJsonItem(string filePath, JsonNode jsonObj, bool changed);
        private record struct SettingItem(string sign, string dataKey, string dataValue);
        private readonly Dictionary<string, SettingJsonItem> _legacyDic = [];

#pragma warning disable CS0618
        private async void GenerateLegacyUI(List<string> signKeys)
        {
            foreach (var sign in signKeys)
            {
                string filePath = Settings.GetPathBySign(sign, Settings.sType.Settings);
                if (!File.Exists(filePath))
                    continue;

                string data = await File.ReadAllTextAsync(filePath);
                JsonNode node = JsonNode.Parse(data)!;
                if (node == null) continue;

                _legacyDic.Add(sign, new SettingJsonItem(filePath, node, false));

                string packageName = node["PackageName"]?.ToString() ?? sign;
                var items = node["Data"]?.AsObject();
                if (items == null) continue;

                var b = new Border
                {
                    CornerRadius = new CornerRadius(15),
                    Margin = new Thickness(20)
                };
                b.SetResourceReference(BackgroundProperty, "MaskColor");

                var g = new Grid();
                b.Child = g;
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });

                var tb = new TextBlock
                {
                    Text = packageName,
                    FontSize = 20,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };
                g.Children.Add(tb);
                Grid.SetColumnSpan(tb, 2);

                int i = 1;
                foreach (var item in items)
                {
                    var key = item.Key;
                    var value = item.Value?.ToString() ?? "";

                    var keyTb = new TextBlock
                    {
                        Text = key,
                        FontSize = 15,
                        Margin = new Thickness(20, 10, 10, 10),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var valueTb = new TextBox
                    {
                        Text = value,
                        FontSize = 15,
                        Height = 30,
                        Margin = new Thickness(10),
                        Style = FindResource("SimpleTextBoxStyle") as Style,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = new SettingItem(sign, key, value)
                    };
                    valueTb.TextChanged += LegacyValueTb_TextChanged;
                    valueTb.SetResourceReference(BackgroundProperty, "MaskColor");
                    valueTb.SetResourceReference(ForegroundProperty, "ForeColor");

                    g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    g.Children.Add(keyTb);
                    g.Children.Add(valueTb);
                    Grid.SetRow(keyTb, i);
                    Grid.SetColumn(keyTb, 0);
                    Grid.SetRow(valueTb, i);
                    Grid.SetColumn(valueTb, 1);
                    i++;
                }
                SettingItemList.Children.Add(b);
            }
        }
#pragma warning restore CS0618

        private void LegacyValueTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is SettingItem item)
            {
                var refObj = _legacyDic[item.sign];
                refObj.jsonObj["Data"]![item.dataKey] = tb.Text;
                refObj.changed = item.dataValue != tb.Text;
                _legacyDic[item.sign] = refObj;
            }
        }

        private void SaveLegacyChanges()
        {
            var changedItems = _legacyDic.Values.Where(x => x.changed).ToList();
            foreach (var item in changedItems)
            {
                System.IO.File.WriteAllText(item.filePath, item.jsonObj.ToString());
            }
        }

        // ======================================================
        //  内部类型
        // ======================================================

        private sealed class SettingsCard
        {
            public Type SettingsType { get; init; } = null!;
            public string Sign { get; init; } = null!;
            public string PackageName { get; set; } = null!;
            public string FilePath { get; init; } = null!;
            public object Instance { get; set; } = null!;
            public SettingsConfigAttribute? ConfigAttr { get; set; }
            public List<FieldEntry> Fields { get; set; } = [];
            public bool Dirty { get; set; }
        }

        private sealed class FieldEntry
        {
            public PropertyInfo Property { get; init; } = null!;
            public SettingsFieldAttribute Attribute { get; init; } = null!;
            public FrameworkElement Control { get; init; } = null!;
            /// <summary>反向引用，用于 CollectValueFromControl 中 set 值</summary>
            public SettingsCard Card { get; set; } = null!;
        }
    }
}
