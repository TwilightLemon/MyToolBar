sampler2D input : register(s0);

float4 HighlightColor : register(c0);
float HighlightIntensity : register(c1);
//float Contrast : register(c2);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv);

    // 根据 Alpha 计算高光强度
    float intensity = color.a * HighlightIntensity;

    // 提高对比度（以 0.5 为中心）
    color.rgb = ((color.rgb - 0.5) * 0.2 + 0.5)*color.a;

    // 加高光
    color.rgb += HighlightColor.rgb * intensity;

    // 防止溢出
    color.rgb = saturate(color.rgb);

    return color;
}