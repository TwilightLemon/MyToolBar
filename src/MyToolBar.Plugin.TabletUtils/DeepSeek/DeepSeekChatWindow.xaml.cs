using DeepSeek.Core;
using DeepSeek.Core.Models;
using EleCho.MdViewer.ColorModes;
using EleCho.MdViewer.Markup;
using MyToolBar.Common;
using MyToolBar.Common.WinAPI;
using MyToolBar.Plugin.TabletUtils.Configs;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MyToolBar.Plugin.TabletUtils.DeepSeek;

/// <summary>
/// SideWindow.xaml 的交互逻辑
/// </summary>
public partial class DeepSeekChatWindow : Window
{
    #region Deepseek config
    internal static string DeepSeekConfigKey = "MyToolBar.Plugin.TabletUtils.DeepSeek";
    private SettingsMgr<DeepSeekConfig> config = new(DeepSeekConfigKey, SideBarPlugin._name);
    private DeepSeekClient? client = null;
    private void Mgr_OnDataChanged()
    {
        _=Init();
    }
    private async Task Init()
    {
        await config.LoadAsync();
        if (!string.IsNullOrEmpty(config.Data?.APIKey))
        {
            client = new DeepSeekClient(config.Data.APIKey);
            client.SetTimeout(5);
            request.Model = config.Data!.Model;
        }
    }

    private readonly List<Message> history = [];
    private static readonly ChatRequest request = new() { 
    MaxTokens=4000
    };
    #endregion
    public DeepSeekChatWindow()
    {
        InitializeComponent();
        Height = SystemParameters.WorkArea.Height-12;
        config.OnDataChanged += Mgr_OnDataChanged;
        Deactivated += SideWindow_Deactivated;
        Activated += SideWindow_Activated;
        Loaded += DeepSeekChatWindow_Loaded;
        Closed += SideWindow_Closed;
        GlobalService.OnIsDarkModeChanged += GlobalService_OnIsDarkModeChanged;
    }

    private IntPtr hwnd;
    private async void DeepSeekChatWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Init();
        hwnd = new WindowInteropHelper(this).Handle;
        SwitchModelTb.IsChecked = config.Data.Model == DeepSeekModels.ReasonerModel;
    }

    private void SideWindow_Closed(object? sender, EventArgs e)
    {
        GlobalService.OnIsDarkModeChanged -= GlobalService_OnIsDarkModeChanged;
        _ = config.SaveAsync();
    }

    private void GlobalService_OnIsDarkModeChanged(bool isDarkMode)
    {
        ApplyThemeForMdViewer(isDarkMode);
    }

    private void ApplyThemeForMdViewer(bool isDarkMode)
    {
        foreach(var robot in MsgContainer.Children)
        {
            if(robot is RobotMsg viewer && viewer.Resources.MergedDictionaries[1] is ThemeDictionary theme)
            {
                theme.ColorMode = isDarkMode ? ColorMode.Dark : ColorMode.Light;
            }
        }
    }

    private void SideWindow_Activated(object? sender, EventArgs e)
    {
        if (FixTb.IsChecked == true) return;

        ApplyThemeForMdViewer(GlobalService.IsDarkMode);
        Height = ScreenAPI.GetScreenArea(hwnd).Height - 64; //?? 

        var da = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(300));
        da.EasingFunction = new CircleEase();
        this.BeginAnimation(LeftProperty, da);
    }

    private void SideWindow_Deactivated(object? sender, EventArgs e)
    {
        if (FixTb.IsChecked == true) return;


        var da = new DoubleAnimation(-ActualWidth, TimeSpan.FromMilliseconds(300));
        da.EasingFunction = new CircleEase();
        da.Completed += delegate {
            Hide();
        };
        this.BeginAnimation(LeftProperty, da);
    }

    private async void tb_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
        {
            await NewMessage();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter &&( Keyboard.Modifiers == ModifierKeys.Control ||
             Keyboard.Modifiers == ModifierKeys.Shift))
        {
            // 处理Ctrl(or Shift) + Enter换行的逻辑
            tb.AppendText(Environment.NewLine);
            tb.CaretIndex = tb.Text.Length;
            e.Handled = true;
        }
    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        MsgContainer.Children.Clear();
        history.Clear();
    }

    private async void SendBtn_Click(object sender, RoutedEventArgs e)
    {
        if (cancelToken == null)
            await NewMessage();
        else
        {
            //Stop
            cancelToken.Cancel();
            SendingStatus.Data = (Geometry)FindResource("SendIcon");
        }
    }

    private static Border UserMsg(string msg,string user="Me")
    {
        var bd = new Border()
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8, 4, 8, 4),
            Child = new TextBlock()
            {
                Text = user+": "+msg,
                TextWrapping = TextWrapping.Wrap
            }
        };
        bd.SetResourceReference(BackgroundProperty, "MaskColor");
        return bd;
    }

    private CancellationTokenSource? cancelToken;
    private async Task NewMessage()
    {
        if (client == null||cancelToken!=null) return;
        var msg = tb.Text;
        if (string.IsNullOrEmpty(msg)) return;
        var usr = UserMsg(msg);
        MsgContainer.Children.Add(usr);
        history.Add(Message.NewUserMessage(msg));
        request.Messages = [.. history];

        SendingStatus.Data = (Geometry)FindResource("StopIcon");
        cancelToken = new();

        try
        {
            var choices = await client.ChatStreamAsync(request, cancelToken.Token);
            if (choices is null)
            {
                MsgContainer.Children.Add(UserMsg(client.ErrorMsg, "Error"));
                return;
            }
            var viewer = new RobotMsg();
            MsgContainer.Children.Add(viewer);
            viewer.HasDeepThinkingContent = request.Model == DeepSeekModels.ReasonerModel;
            var answer = new StringBuilder();
            var cache = new StringBuilder();
            bool reasoningStopped = false;
            Stopwatch sw = new();
            sw.Start();
            await foreach (var choice in choices)
            {
                var reasoning = choice.Delta?.ReasoningContent;
                if (reasoning != null)
                {
                    viewer.DeepThought += reasoning;
                }

                var str = choice.Delta?.Content;
                if (str is not null)
                {
                    if (!reasoningStopped)
                    {
                        sw.Stop();
                        viewer.SetDeepThinkingTime(sw.Elapsed);
                        reasoningStopped = true;
                    }
                    cache.Append(str);
                    if (cache.Length > 10)
                    {
                        answer.Append(cache);
                        viewer.Markdown += cache.ToString();
                        cache.Clear();
                    }
                }
            }
            if (cache.Length > 0)
            {
                answer.Append(cache);
                viewer.Markdown += cache.ToString();
            }
            history.Add(Message.NewAssistantMessage(answer.ToString(), false));
            tb.Text = "";
        }
        catch(Exception e){
            MsgContainer.Children.Add(UserMsg(e.Message, "Error"));
            history.RemoveAt(history.Count - 1);
        }
        SendingStatus.Data = (Geometry)FindResource("SendIcon");
        cancelToken = null;
    }

    private void SwitchModelTb_Click(object sender, RoutedEventArgs e)
    {
        config.Data!.Model=request.Model = SwitchModelTb.IsChecked == true ? DeepSeekModels.ReasonerModel : DeepSeekModels.ChatModel;
        _ = config.SaveAsync();
    }
}
