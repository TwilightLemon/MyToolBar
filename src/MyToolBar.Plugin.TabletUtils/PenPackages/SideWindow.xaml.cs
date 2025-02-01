using DeepSeek.Core;
using DeepSeek.Core.Models;
using MyToolBar.Common;
using MyToolBar.Plugin.TabletUtils.Configs;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MyToolBar.Plugin.TabletUtils.PenPackages
{
    /// <summary>
    /// SideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SideWindow : Window
    {
        #region Deepseek config
        internal static string DeepSeekConfigKey = "MyToolBar.Plugin.TabletUtils.DeepSeek";
        private static SettingsMgr<DeepSeekConfig> config = new(DeepSeekConfigKey, SideBarPlugin._name);
        private static DeepSeekClient? client = null;
        static SideWindow()
        {
            Init();
            config.OnDataChanged += Mgr_OnDataChanged;
        }
        private static void Mgr_OnDataChanged()
        {
            Init();
        }
        private static async void Init()
        {
            await config.Load();
            if (!string.IsNullOrEmpty(config.Data?.APIKey))
            {
                client = new DeepSeekClient(config.Data.APIKey);
            }
        }

        private readonly List<Message> history = [];
        private readonly ChatRequest request = new();
        #endregion
        public SideWindow()
        {
            InitializeComponent();
            Height = SystemParameters.WorkArea.Height-12;
            Deactivated += SideWindow_Deactivated;
            Activated += SideWindow_Activated;
            GlobalService.OnIsDarkModeChanged += GlobalService_OnIsDarkModeChanged;
        }

        private void GlobalService_OnIsDarkModeChanged(bool isDarkMode)
        {
            ApplyThemeForMdViewer(isDarkMode);
        }

        private void ApplyThemeForMdViewer(bool isDarkMode)
        {
            viewer.Resources.MergedDictionaries.Clear();
            var theme = isDarkMode ? Wpf.Ui.Appearance.ApplicationTheme.Dark : Wpf.Ui.Appearance.ApplicationTheme.Light;
            viewer.Resources.MergedDictionaries.Add(new Wpf.Ui.Markup.ThemesDictionary() { Theme = theme });
            viewer.Resources.MergedDictionaries.Add(new Wpf.Ui.Markdown.Markup.ThemesDictionary() { Theme = theme });
            Wpf.Ui.Markdown.Appearance.ThemeManager.IsDarkMode = isDarkMode;
        }

        private void SideWindow_Activated(object? sender, EventArgs e)
        {
            if (FixTb.IsChecked == true) return;

            ApplyThemeForMdViewer(GlobalService.IsDarkMode);
            viewer.Markdown="""
                nihao
                ```bash
                sudo apt install zip
                ```
                """;
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
                GlobalService.OnIsDarkModeChanged -= GlobalService_OnIsDarkModeChanged;
            };
            this.BeginAnimation(LeftProperty, da);
        }

        private async void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (client == null) return;
            if(e.Key == Key.Enter)
            {
                var msg = tb.Text;
                viewer.Markdown += "\r\n***\r\nMe: "+msg+"\r\n";
                if (string.IsNullOrEmpty(msg)) return;
                history.Add(Message.NewUserMessage(msg));
                request.Messages = [.. history];
                var choices = await client.ChatStreamAsync(request, new CancellationToken());
                if (choices is null)
                {
                    viewer.Markdown+="## Error \r\n"+client.ErrorMsg;
                    return;
                }
                viewer.Markdown += "\r\n***\r\n> Reasoning...  \r\n> ";
                var answer = new StringBuilder();
                bool reasoningStopped = false;
                Stopwatch sw = new();
                sw.Start();
                await foreach (var choice in choices)
                {
                    var reasoning = choice.Delta?.ReasoningContent;
                    if(reasoning != null)
                    {
                        viewer.Markdown += reasoning;
                        if (reasoning.EndsWith('\n'))
                            viewer.Markdown += "> ";
                    }
                    
                    var str = choice.Delta?.Content;
                    if (str is not null)
                    {
                        if (!reasoningStopped)
                        {
                            sw.Stop();
                            viewer.Markdown += $"  \r\n> Time taken: {sw.Elapsed.TotalSeconds}s. \r\n\r\n";
                            reasoningStopped = true;
                        }
                        viewer.Markdown += str;
                        answer.Append(str);
                    }
                }
                history.Add(Message.NewAssistantMessage(answer.ToString(),false));
                tb.Text = "";
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            viewer.Markdown = "";
            history.Clear();
        }
    }
}
