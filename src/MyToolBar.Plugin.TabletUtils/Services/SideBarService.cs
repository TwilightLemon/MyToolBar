using MyToolBar.Plugin.TabletUtils.PenPackages;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MyToolBar.Plugin.TabletUtils.Services
{
    public class SideBarService : ServiceBase
    {
        private bool _isRunning = false;
        private WeakReference<SideLauncherWindow>? _slw = null;
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

        public void Dispose()
        {
            if (IsRunning && _slw != null && _slw.TryGetTarget(out var window))
            {
                window.Close();
            }
        }

        public Task Start()
        {
            if (_slw == null)
            {
                var window = new SideLauncherWindow();
                window.Closing += Window_Closing;
                window.Show();
                _slw = new WeakReference<SideLauncherWindow>(window);
                IsRunning = true;
            }
            return Task.CompletedTask;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            IsRunning = false;
        }

        public Task Stop()
        {
            Dispose();
            return Task.CompletedTask;
        }
    }
}
