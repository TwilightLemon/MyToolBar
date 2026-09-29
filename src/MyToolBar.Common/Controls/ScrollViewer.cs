using FluentWpfCore.Controls;
using FluentWpfCore.ScrollPhysics;

namespace MyToolBar.Common.Controls;

public sealed class ScrollViewer : SmoothScrollViewer
{
    public ScrollViewer()
    {
        Physics = new ExponentialScrollPhysics
        {
            DecayRate = 12,
            StopThreshold = 0.5
        };
    }
}
