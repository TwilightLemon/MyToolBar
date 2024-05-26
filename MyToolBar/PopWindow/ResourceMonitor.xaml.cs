using MyToolBar.Func;
using MyToolBar.PopWindow.Items;
using MyToolBar.WinApi;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// ResourceMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class ResourceMonitor : PopWindowBase
    {
        public ResourceMonitor()
        {
            InitializeComponent();
            GlobalService.GlobalTimer.Elapsed += MonitorProcesses;
            MonitorProcesses(null, null);
            this.Closed += delegate
            {
                GlobalService.GlobalTimer.Elapsed -= MonitorProcesses;
            };
        }

        private void MonitorProcesses(object? sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (ProcessList != null)
                {
                    ProcessList.Children.Clear();
                    var processes = Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).Take(20);
                    foreach (var p in processes)
                    {
                        var item = new ProcessItem(p);
                        item.MouseLeftButtonUp += ProcItem_Clicked;
                        ProcessList.Children.Add(item);
                    }
                }
            });
        }
        private Process? SelectedProcess = null;
        private Thickness? SeletedItemPos = null;
        private void ProcItem_Clicked(object sender, MouseButtonEventArgs e)
        {
            ProcessItem item = (ProcessItem)sender;

            SelectedProcess= item._pro;
            //获取process对应图标
            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(item._pro.MainModule.FileName);
                PInfo_Icon.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                PInfo_Icon.Source = null;
            }
            PInfo_Name.Text = item._pro.MainWindowTitle;
            if (string.IsNullOrWhiteSpace(PInfo_Name.Text))
                PInfo_Name.Text = item._pro.ProcessName;
            try
            {
                PInfo_file.Text = Path.GetFileName(item._pro.MainModule.FileName);
            }
            catch
            {
                PInfo_file.Text = "Access Denied !";
            }
            PInfo_PID.Text ="PID: "+ item._pro.Id.ToString();
           // PInfo_CPU.Text = item._pro.TotalProcessorTime.ToString();
            PInfo_MEM.Text ="Memory: "+ NetworkInfo.FormatSize(item._pro.WorkingSet64);
            Storyboard sb = (Storyboard)Resources["OpenDetalPage"];
            var trans = item.TranslatePoint(new Point(0, 0), this);
            SeletedItemPos=(sb.Children[1] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value=new Thickness(10,trans.Y,10,this.ActualHeight-trans.Y-item.ActualHeight);
            sb.Begin();
        }

        private void OpenTaskMonitorBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("taskmgr");
        }

        private void PInfo_EndBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedProcess?.Kill();
            SelectedProcess = null;
            ((Storyboard)Resources["CloseDetalPage"]).Begin();
        }

        private void PInfo_OpenBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var path = SelectedProcess?.MainModule.FileName;
                //Process.Start("explorer", "/select,"+path);
                if (path != null)
                {
                    ExplorerAPI.OpenFolderAndSelectFile(path);
                }
            }
            catch { }
        }

        private void PInfo_BackBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedProcess = null;
            var sb = (Storyboard)Resources["CloseDetalPage"];
            (sb.Children[2] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = SeletedItemPos.Value;
            sb.Begin();
        }
    }
}
