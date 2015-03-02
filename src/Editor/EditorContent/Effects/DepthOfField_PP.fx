sampler2D tex[3];

float MaxDepth = 20000;

// Distance at which blur starts
float BlurStart = 600;

// Distance at which scene is fully blurred
float BlurEnd = 1000;

float4 PixelShaderFunction(float4 Position : POSITION0, 
	float2 UV : TEXCOORD0) : COLOR0
{
	// Determine depth
	float depth = tex2D(tex[2], UV).r * MaxDepth;

	// Get blurred and unblurred render of scene
	float4 unblurred = tex2D(tex[1], UV);
	float4 blurred = tex2D(tex[0], UV);

	// Determine blur amount (similar to fog calculation)
	float blurAmt = clamp((depth - BlurStart) / (BlurEnd - BlurStart), 0, 1);

	// Blend between unblurred and blurred images
	float4 mix = lerp(unblurred, blurred, blurAmt);
	
	return mix;
}

technique Technique1
{
    pass p0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
