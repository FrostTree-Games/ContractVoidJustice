sampler s0;

Texture halfResMap;
Texture quarterResMap;

sampler halfSampler = sampler_state
{
	Texture = <halfResMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler quarterSampler = sampler_state
{
	Texture = <quarterResMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};


// Get the threshold of what brightness level we want to glow
float Threshold = 0.7;

// get the blur distance of the filter
float BlurDistanceX = 0.0005;
float BlurDistanceY = 0.0005;

float4 cutoff (float4 color)
{
	return saturate( (color - Threshold) / (1 - Threshold));
}

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

float4 blur5Cutoff(float4 color, float4 color2, float4 color3, float4 color4, float4 color5)
{
	return (cutoff(color) + cutoff(color2) + cutoff(color3) + cutoff(color4) + cutoff(color5)) / 5;
}

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(s0, coords);
	float4 halfColor = blur5Cutoff(tex2D(halfSampler, coords), tex2D(halfSampler, coords + float2(BlurDistanceX, 0)), tex2D(halfSampler, coords + float2(-BlurDistanceX, 0)), tex2D(halfSampler, coords + float2(0, BlurDistanceY)), tex2D(halfSampler, coords + float2(0, -BlurDistanceY)));
	float4 quarterColor = blur5Cutoff(tex2D(quarterSampler, coords), tex2D(quarterSampler, coords + float2(BlurDistanceX, 0)), tex2D(quarterSampler, coords + float2(-BlurDistanceX, 0)), tex2D(quarterSampler, coords + float2(0, BlurDistanceY)), tex2D(quarterSampler, coords + float2(0, -BlurDistanceY)));

	if (coords.x < 0.5)
	{
	return color + halfColor + quarterColor;
	}
	else
	{
	return color;
	}

}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

