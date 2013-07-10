sampler s0;

// Get the threshold of what brightness level we want to glow
float Threshold = 0.3;

// get the blur distance of the filter
float BlurDistance = 0.002;

float4 blur2(float4 color, float4 color2)
{
	return (color + color2) / 2;
}

float4 blur4(float4 color, float4 color2, float4 color3, float4 color4)
{
	return (color + color2 + color3 + color4) / 4;
}

float4 blur5(float4 color, float4 color2, float4 color3, float4 color4, float4 color5)
{
	return (color + color2 + color3 + color4 + color5) / 5;
}

float4 cutoff (float4 color)
{
	return saturate( (color - Threshold) / (1 - Threshold));
}

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(s0, coords);
	float4 blurColor1 = tex2D(s0, coords + float2(BlurDistance, 0));
	float4 blurColor2 = tex2D(s0, coords + float2(-BlurDistance, 0));
	float4 blurColor3 = tex2D(s0, coords + float2(0, BlurDistance));
	float4 blurColor4 = tex2D(s0, coords + float2(0, -BlurDistance));

	float4 extraBlur = blur4(tex2D(s0, coords + float2(BlurDistance / 2, 0)), tex2D(s0, coords + float2(0, BlurDistance / 2)), tex2D(s0, coords + float2(-BlurDistance / 2, 0)), tex2D(s0, coords + float2(0, -BlurDistance / 2)));

	float4 blurResult = blur2( extraBlur, blur5(cutoff(color), cutoff(blurColor1), cutoff(blurColor2), cutoff(blurColor3), cutoff(blurColor4)));

	color *= (1 - saturate(blurResult)  / 2);

	return blurResult + color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

