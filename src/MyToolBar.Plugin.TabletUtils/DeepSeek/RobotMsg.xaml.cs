using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace MyToolBar.Plugin.TabletUtils.DeepSeek;

/// <summary>
/// RobotMsg.xaml 的交互逻辑
/// </summary>
public partial class RobotMsg : UserControl
{
    public RobotMsg()
    {
        InitializeComponent();
    }
    public string Markdown
    {
        get => viewer.Content;
        set => viewer.Content = value;
    }

    public string DeepThought
    {
        get => ThoughtTb.Text;
        set => ThoughtTb.Text = value;
    }

    public bool HasDeepThinkingContent
    {
        set => DeepThinking.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }
    public void SetDeepThinkingTime(TimeSpan time)
    {
        OpenThoughtBtn.Content = $"Thought for {Math.Round(time.TotalSeconds,2)}s";
    }

    private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        cc.Condition = !cc.Condition;
        tb.Text = Markdown;
    }

    private void OpenThoughtBtn_Click(object sender, RoutedEventArgs e)
    {
        ThoughtBd.Visibility = OpenThoughtBtn.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }
}
