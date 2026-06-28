using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MyToolBar.Shaders.Impl;

public sealed class HighlightEffect : ShaderEffect
{
    #region Constructor

    public HighlightEffect()
    {
        PixelShader = new PixelShader
        {
            UriSource = new Uri(
                "pack://application:,,,/MyToolBar;component/Shaders/Highlighter.ps",
                UriKind.Absolute)
        };
        PixelShader.Freeze();

        UpdateShaderValue(InputProperty);

        UpdateShaderValue(HighlightColorProperty);
        UpdateShaderValue(HighlightIntensityProperty);
    }

    #endregion

    #region Input (s0)

    /// <summary>
    /// 输入纹理
    /// </summary>
    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(
            "Input",
            typeof(HighlightEffect),
            0,
            SamplingMode.NearestNeighbor);

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    #endregion

    #region HighlightColor

    /// <summary>
    /// 高光颜色
    /// </summary>
    public static readonly DependencyProperty HighlightColorProperty =
        DependencyProperty.Register(
            nameof(HighlightColor),
            typeof(Color),
            typeof(HighlightEffect),
            new UIPropertyMetadata(
                Color.FromArgb(255, 230, 242, 255),
                PixelShaderConstantCallback(0)));

    public Color HighlightColor
    {
        get => (Color)GetValue(HighlightColorProperty);
        set => SetValue(HighlightColorProperty, value);
    }

    #endregion

    #region HighlightIntensity
    public double HighlightIntensity
    {
        get { return (double)GetValue(HighlightIntensityProperty); }
        set { SetValue(HighlightIntensityProperty, value); }
    }

    public static readonly DependencyProperty HighlightIntensityProperty =
        DependencyProperty.Register(
            nameof(HighlightIntensity),
            typeof(double),
            typeof(HighlightEffect),
            new UIPropertyMetadata(
                1.0,
                PixelShaderConstantCallback(1)));
    #endregion
}