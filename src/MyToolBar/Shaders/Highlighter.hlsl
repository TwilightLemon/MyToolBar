sampler2D input : register(s0);

float4 HighlightColor : register(c0);
float HighlightIntensity : register(c1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv);
    float intensity = color.a * HighlightIntensity;
    float3 additiveResult = color.rgb + HighlightColor.rgb * intensity;

    color.rgb = additiveResult;

    return color;
}