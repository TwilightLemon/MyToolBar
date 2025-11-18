using MyToolBar.Plugin.TabletUtils.PenPackages;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MyToolBar.Plugin.TabletUtils.Services
{
    public class PenMenuService : IUserService
    {
        private bool _isRunning = false;
        private WeakReference<PenControlWindow>? _pcw = null;
        public bool IsRunning
        {
            get => _isRunning; private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    IsRunningChanged?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<bool>? IsRunningChanged;
        public event EventHandler? OnForceStop;

        public Task Start()
        {
            if (_pcw == null)
            {
                if (Tablet.TabletDevices.Count > 0)
                {
                    var window = new PenControlWindow();
                    window.Owner = Application.Current.MainWindow;
                    window.Closing += Window_Closing;
                    window.Show();
                    _pcw = new WeakReference<PenControlWindow>(window);
                    IsRunning = true;
                }
                else
                {
                    IsRunning = false;
                    OnForceStop?.Invoke(this, EventArgs.Empty);
                }
            }
            return Task.CompletedTask;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            IsRunning = false;
        }

        public Task Stop()
        {
            if (IsRunning && _pcw != null && _pcw.TryGetTarget(out var window))
            {
                window.Close();
            }
            return Task.CompletedTask;
        }
    }
}
