using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using MyToolBar.Common.WinAPI;

namespace MyToolBar.Services;

/// <summary>
/// 捕获 AppBar 画面，并按控件的实际横向位置分析适合的前景色。
/// </summary>
public sealed class AppBarForegroundService
{
    /// <summary>
    /// 捕获一次 AppBar 画面。调用方应在同一帧上完成所有元素分析并释放结果。
    /// </summary>
    public SamplingFrame? Capture(Window window, IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero || window.ActualWidth <= 0 || window.ActualHeight <= 0)
            return null;

        (double dpiX, double dpiY) = ScreenAPI.GetDPI(ScreenAPI.GetHmonitorForHwnd(windowHandle));
        int captureWidth = Math.Max(1, (int)Math.Ceiling(window.ActualWidth * dpiX));
        int captureHeight = Math.Max(1, (int)Math.Ceiling(window.ActualHeight * dpiY));
        Bitmap? bitmap = ScreenAPI.CaptureScreenArea(
            (int)(window.Left * dpiX),
            (int)(window.Top * dpiY),
            captureWidth,
            captureHeight);

        return bitmap == null ? null : new SamplingFrame(window, bitmap, dpiX, dpiY);
    }

    /// <summary>
    /// 一次截图对应的采样上下文。
    /// </summary>
    public sealed class SamplingFrame : IDisposable
    {
        private readonly Window _window;
        private readonly Bitmap _bitmap;
        private readonly double _dpiX;
        private readonly int _horizontalStep;
        private readonly int _edgeBandHeight;
        private bool _disposed;

        internal SamplingFrame(Window window, Bitmap bitmap, double dpiX, double dpiY)
        {
            _window = window;
            _bitmap = bitmap;
            _dpiX = dpiX;
            _horizontalStep = Math.Max(2, (int)Math.Round(6 * dpiX));
            _edgeBandHeight = Math.Min(
                Math.Max(2, (int)Math.Ceiling(6 * dpiY)),
                Math.Max(1, bitmap.Height / 2));
        }

        /// <summary>
        /// 分析单个元素。对于被父级拉伸的控件，可使用其期望宽度定位实际内容。
        /// </summary>
        public bool? Evaluate(
            FrameworkElement element,
            bool? currentUseBlackForeground,
            bool useDesiredWidth = false)
        {
            ThrowIfDisposed();
            return EvaluateRanges([GetHorizontalRange(element, useDesiredWidth)], currentUseBlackForeground);
        }

        /// <summary>
        /// 将多个元素的非连续横向范围作为一个整体进行分析。
        /// </summary>
        public bool? Evaluate(IEnumerable<FrameworkElement> elements, bool? currentUseBlackForeground)
        {
            ThrowIfDisposed();

            var ranges = new List<HorizontalRange?>();
            foreach (FrameworkElement element in elements)
            {
                ranges.Add(GetHorizontalRange(element));
            }

            return EvaluateRanges(ranges, currentUseBlackForeground);
        }

        private bool? EvaluateRanges(IEnumerable<HorizontalRange?> ranges, bool? currentUseBlackForeground)
        {
            var luminances = new List<double>();
            int firstSampleY = _bitmap.Height > 2 ? 1 : 0;
            int lastSampleY = _bitmap.Height > 2 ? _bitmap.Height - 1 : _bitmap.Height;

            foreach (HorizontalRange? range in ranges)
            {
                if (range is not { } value)
                    continue;

                for (int x = value.Begin; x < value.End; x += _horizontalStep)
                {
                    for (int y = firstSampleY; y < lastSampleY; y += 2)
                    {
                        bool isEdgeSample = y < _edgeBandHeight || y >= _bitmap.Height - _edgeBandHeight;
                        if (!isEdgeSample)
                            continue;

                        Color color = _bitmap.GetPixel(x, y);
                        luminances.Add(ForegroundContrastAnalyzer.GetRelativeLuminance(
                            color.R,
                            color.G,
                            color.B));
                    }
                }
            }

            return luminances.Count == 0
                ? null
                : ForegroundContrastAnalyzer.UseBlackForeground(luminances, currentUseBlackForeground);
        }

        private HorizontalRange? GetHorizontalRange(FrameworkElement element, bool useDesiredWidth = false)
        {
            if (!element.IsVisible || element.ActualWidth <= 0)
                return null;

            Rect bounds;
            try
            {
                bounds = element.TransformToVisual(_window).TransformBounds(new Rect(element.RenderSize));
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            if (useDesiredWidth && element.DesiredSize.Width > 0 && element.DesiredSize.Width < bounds.Width)
            {
                double desiredWidth = element.DesiredSize.Width;
                bounds = new Rect(
                    bounds.Left + (bounds.Width - desiredWidth) / 2.0,
                    bounds.Top,
                    desiredWidth,
                    bounds.Height);
            }

            int begin = Math.Clamp((int)Math.Floor(bounds.Left * _dpiX), 0, _bitmap.Width - 1);
            int end = Math.Clamp((int)Math.Ceiling(bounds.Right * _dpiX), begin + 1, _bitmap.Width);
            return new HorizontalRange(begin, end);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _bitmap.Dispose();
            _disposed = true;
        }

        private readonly record struct HorizontalRange(int Begin, int End);
    }
}
