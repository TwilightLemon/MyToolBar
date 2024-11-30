using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MyToolBar.Common.UIBases;
using MyToolBar.Common.WinAPI;
using MyToolBar.Common;
using MyToolBar.Plugin.BasicPackage.API;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// ResourceMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class ResourceMonitor : PopupWindowBase
    {
        public ResourceMonitor()
        {
            InitializeComponent();
            DataContext = this;

            GlobalService.GlobalTimer.Elapsed += MonitorProcesses;
            LocalCulture.OnLanguageChanged += LocalCulture_OnLanguageChanged;
            LocalCulture_OnLanguageChanged(null, LocalCulture.Current);
            MonitorProcesses(null, null);
            this.Closed += delegate
            {
                GlobalService.GlobalTimer.Elapsed -= MonitorProcesses;
                LocalCulture.OnLanguageChanged -= LocalCulture_OnLanguageChanged;
            };
        }

        private void LocalCulture_OnLanguageChanged(object? sender, LocalCulture.Language e)
        {
            string uri = $"/MyToolBar.Plugin.BasicPackage;component/LanguageRes/ResouceMonitor/Lang{
                e switch { LocalCulture.Language.en_us=>"En_US",
                LocalCulture.Language.zh_cn=>"Zh_CN",
                _=> throw new Exception("Unsupported Language")
                }
                }.xaml";
            //删除旧资源
            var old=Resources.MergedDictionaries.FirstOrDefault(p => p.Source != null && p.Source.OriginalString.Contains("LanguageRes/ResouceMonitor/Lang"));
            if(old != null)
                Resources.MergedDictionaries.Remove(old);
            //添加新资源
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
        }

        #region MainPage
        /// <summary>
        /// [时钟任务] 监测进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitorProcesses(object? sender, System.Timers.ElapsedEventArgs e)
        {
            void InitProcessList(int count)
            {
                ProcessList.Children.Clear();
                for (int i = 0; i < count; i++)
                {
                    var item = new ProcessItem();
                    item.Click += ProcItem_Clicked;
                    ProcessList.Children.Add(item);
                }
            }
            this.Dispatcher.Invoke(() =>
            {
                if (ProcessList != null)
                {
                    //仅在第一次初始化时创建
                    int count = 30;
                    if (ProcessList.Children.Count == 0)
                        InitProcessList(count);
                    var processes = Process.GetProcesses().OrderByDescending(p => p.WorkingSet64)
                                                                               .Take(count- _frozenProc.Count).ToList();
                    processes.AddRange(_frozenProc);
                    for (int i = 0; i < count; i++)
                    {
                        (ProcessList.Children[i] as ProcessItem).UpdateData(i < processes.Count() ? processes.ElementAt(i) : null);
                    }
                }
            });
        }

        /// <summary>
        /// 选中的进程
        /// </summary>
        private Process? SelectedProcess = null;
        private bool SelectedProcess_Accessible = true;
        /// <summary>
        /// 用于执行动画的位置
        /// </summary>
        private Thickness? SeletedItemPos = null;
        private void ProcItem_Clicked(object sender, RoutedEventArgs e)
        {
            AlwaysShow = true;
            ProcessItem item = (ProcessItem)sender;

            SelectedProcess = item._pro;
            SelectedProcess_Accessible = true;
            //获取process对应图标 高完整性进程无法获取
            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(item._pro.MainModule.FileName);
                PInfo_Icon.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                PInfo_Icon.Source = null;
                SelectedProcess_Accessible = false;
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
                PInfo_file.Text = FindResource("Tip_AccessDenied").ToString();
                SelectedProcess_Accessible = false;
            }
            PInfo_PID.Text = item._pro.Id.ToString();
            // PInfo_CPU.Text = item._pro.TotalProcessorTime.ToString();
            PInfo_MEM.Text = NetworkInfo.FormatSize(item._pro.WorkingSet64);
            bool isResponding = item._pro.IsResponding(out _);

            PInfo_STA.Text = FindResource(isResponding ? "Tip_Responding" : "Tip_Frozen").ToString();

            Storyboard sb = (Storyboard)Resources["OpenDetalPage"];
            var trans = item.TranslatePoint(new Point(0, 0), this);
            SeletedItemPos = (sb.Children[1] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(10, trans.Y, 10, this.ActualHeight - trans.Y - item.ActualHeight);
            sb.Begin();

            if (SelectedProcess_Accessible)
            {
                GlobalService.GlobalTimer.Elapsed += RefreshPInfo;
            }
        }

        private void RefreshPInfo(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (SelectedProcess == null)
            {
                GlobalService.GlobalTimer.Elapsed -= RefreshPInfo;
                return;
            }
            var cur = SelectedProcess;
            bool isResponding = cur.IsResponding(out _);
            string mem= NetworkInfo.FormatSize(cur.WorkingSet64),
                name= cur.MainWindowTitle,
                proc= cur.ProcessName;
            Dispatcher.Invoke(() => { 
                PInfo_STA.Text = FindResource(isResponding ? "Tip_Responding" : "Tip_Frozen").ToString();
                PInfo_MEM.Text = mem;
                PInfo_Name.Text = name;
                if (string.IsNullOrWhiteSpace(PInfo_Name.Text))
                    PInfo_Name.Text = proc;
            });
        }
        private void ClosePInfoPage()
        {
            SelectedProcess = null;
            var sb = (Storyboard)Resources["CloseDetalPage"];
            AlwaysShow = false;
            (sb.Children[2] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = SeletedItemPos.Value;
            sb.Begin();
            GlobalService.GlobalTimer.Elapsed -= RefreshPInfo;
        }

        private void PInfo_EndBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedProcess_Accessible)
                {
                    SelectedProcess?.Kill();
                    ClosePInfoPage();
                }
            }
            catch { }
        }

        private void PInfo_OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedProcess_Accessible)
                {
                    var path = SelectedProcess?.MainModule.FileName;
                    //Process.Start("explorer", "/select,"+path);
                    if (path != null)
                    {
                        ExplorerAPI.OpenFolderAndSelectFile(path);
                    }
                }
            }
            catch { }
        }

        private void PInfo_BackBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedProcess = null;
            var sb = (Storyboard)Resources["CloseDetalPage"];
            AlwaysShow = false;
            (sb.Children[2] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = SeletedItemPos.Value;
            sb.Begin();
        }

        private void OpenTaskMonitorBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("taskmgr");
        }
        private static List<Process> _frozenProc = [];
        private void PInfo_FreezeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedProcess_Accessible) return;

            if (SelectedProcess.IsResponding(out _))
            {
                SelectedProcess.Freeze();
                _frozenProc.Add(SelectedProcess);
                //PInfo_STA.Text = FindResource("Tip_Frozen").ToString();
            }
            else
            {
                SelectedProcess.Unfreeze();
                if(_frozenProc.FirstOrDefault(p => p.Id == SelectedProcess.Id) is { } proc)
                {
                    _frozenProc.Remove(proc);
                }
                
               // PInfo_STA.Text = FindResource("Tip_Responding").ToString();
            }
        }
        #endregion

        #region FinalizerMode

        private bool _isFinalizerModeOpen = false,
            _isDetectingActiveWindow=false;
        private IntPtr _finalizerActiveHook;
        public List<Process>? NotRespondingProcData { get; set; }
        public Process? NotRespondingProcChoosen { get; set; }

        private void FinalizerModeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isFinalizerModeOpen) {
                AlwaysShow = false;
                _isFinalizerModeOpen = false;
                TitleSignTb.Visibility = Visibility.Hidden;
                this.Deactivated -= Finalizer_StartDetectActiveWindow;
                this.Activated -= Finalizer_StopDetectActiveWindow;
                ActiveWindow.UnregisterActiveWindowHook(_finalizerActiveHook);
                GlobalService.GlobalTimer.Elapsed -= FinalizerUpdateData;
                (Resources["CloseFinalizerModeAni"] as Storyboard).Begin();
            }
            else
            {
                AlwaysShow = true;
                _isFinalizerModeOpen = true;
                TitleSignTb.Visibility = Visibility.Visible;
                this.Deactivated += Finalizer_StartDetectActiveWindow;
                this.Activated += Finalizer_StopDetectActiveWindow;
                _finalizerActiveHook = ActiveWindow.RegisterActiveWindowHook(Finalizer_OnActiveWindow);
                GlobalService.GlobalTimer.Elapsed += FinalizerUpdateData;
                (Resources["OpenFinalizerModeAni"] as Storyboard).Begin();
            }
        }

        private void Finalizer_StopDetectActiveWindow(object? sender, EventArgs e)
        {
            _isDetectingActiveWindow = false;
        }

        private void Finalizer_StartDetectActiveWindow(object? sender, EventArgs e)
        {
            _isDetectingActiveWindow = true;
        }

        private void Finalizer_OnActiveWindow(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (_isDetectingActiveWindow)
            {
                NotRespondingList.SelectedItem = null;
                NotRespondingProcChoosen = ActiveWindow.GetForegroundWindow().GetWindowProcess();
                FinalizerChoosenTb.Text=NotRespondingProcChoosen?.ProcessName;
            }
        }

        private void FinalizerUpdateData(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var data = Process.GetProcesses().Where(p => !p.Responding).OrderBy(p=>p.Id).ToList();
            Dispatcher.Invoke(() =>
            {
                //判断NotRespondingProcData是否不同，有不同才更新：
                if (NotRespondingProcData == null || NotRespondingProcData.Count != data.Count
                        || !NotRespondingProcData.Select(p => p.Id).SequenceEqual(data.Select(p => p.Id)))
                    NotRespondingList.ItemsSource = NotRespondingProcData = data;
                FinalizerNoItemTip.Visibility = NotRespondingProcData.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void NotRespondingList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (NotRespondingList.SelectedItem == null) return;
            NotRespondingProcChoosen = NotRespondingList.SelectedItem as Process;
            FinalizerChoosenTb.Text = NotRespondingProcChoosen?.ProcessName;
        }

        private void FinalizerModeKillBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotRespondingProcChoosen?.Kill();
                NotRespondingProcChoosen = null;
                FinalizerChoosenTb.Text = "";
            }
            catch { }
        }
        #endregion
    }
}
