sampler2D tex[1]; // Input Texture

float4 PixelShaderFunction(float4 Position : POSITION0,
	float2 UV : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(tex[0], UV);
	
	float intensity = 0.3f * color.r
		+ 0.59f * color.g
		+ 0.11f * color.b;

	return float4(intensity, intensity, intensity, color.a);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}