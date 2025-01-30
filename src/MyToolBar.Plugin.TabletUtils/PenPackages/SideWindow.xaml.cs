using DeepSeek;
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
        internal static string DeepSeekConfigKey = "MyToolBar.Plugin.TabletUtils.DeepSeek";
        SettingsMgr<DeepSeekConfig> mgr = new(DeepSeekConfigKey, SideBarPlugin._name);
        DeepSeekClient? client = null;
        const string model = "deepseek-reasoner";
        readonly List<Message> history = [
                    //Message.NewSystemMessage("You are a professional computer programmer.")
        ];
        readonly ChatRequest request = new()
        {
            Model = model
        };
        public SideWindow()
        {
            InitializeComponent();
            Height = SystemParameters.WorkArea.Height-12;
            Deactivated += SideWindow_Deactivated;
            Activated += SideWindow_Activated;
            mgr.OnDataChanged += Mgr_OnDataChanged;
            Init();         
        }

        private void Mgr_OnDataChanged()
        {
            Init();
        }

         async void Init()
        {
            await mgr.Load();
            if (!string.IsNullOrEmpty(mgr.Data?.APIKey))
            {
                client = new DeepSeekClient(mgr.Data.APIKey);
            }
        }

        private void SideWindow_Activated(object? sender, EventArgs e)
        {
            if (FixTb.IsChecked == true) return;
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
