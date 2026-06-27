using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MyToolBar.Common
{
    /// <summary>
    /// 防抖调度器：在连续触发时只保留最后一次，等待停歇后在 UI 线程执行。
    /// 线程安全，可从任意线程调用 Trigger。
    /// </summary>
    public class DebounceDispatcher
    {
        private readonly Action _action;
        private readonly int _delayMs;
        private readonly Dispatcher _dispatcher;
        private CancellationTokenSource? _cts;
        private readonly object _lock = new();

        /// <param name="action">要执行的操作</param>
        /// <param name="delayMs">防抖延迟（毫秒），默认 200ms</param>
        /// <param name="dispatcher">UI 调度器，默认使用 Application.Current.Dispatcher</param>
        public DebounceDispatcher(Action action, int delayMs = 200, Dispatcher? dispatcher = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _delayMs = delayMs;
            _dispatcher = dispatcher ?? System.Windows.Application.Current.Dispatcher;
        }

        /// <summary>
        /// 触发（线程安全）。每次调用重置计时，停歇超过 delayMs 后在 UI 线程执行 action
        /// </summary>
        public void Trigger()
        {
            CancellationTokenSource oldCts;
            lock (_lock)
            {
                oldCts = _cts;
                _cts = new CancellationTokenSource();
            }
            oldCts?.Cancel();

            var token = _cts.Token;
            Task.Delay(_delayMs, token).ContinueWith(_ =>
            {
                if (!token.IsCancellationRequested)
                {
                    _dispatcher.Invoke(_action);
                }
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// 取消当前等待中的防抖
        /// </summary>
        public void Cancel()
        {
            lock (_lock)
            {
                _cts?.Cancel();
                _cts = null;
            }
        }
    }
}
