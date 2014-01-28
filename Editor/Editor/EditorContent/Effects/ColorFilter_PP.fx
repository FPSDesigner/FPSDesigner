sampler2D tex[1]; // Input Texture

float redPercent;
float greenPercent;
float bluePercent;

float4 PixelShaderFunction(float4 Position : POSITION0,
	float2 UV : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(tex[0], UV);
	
	return float4(redPercent * color.r, greenPercent * color.g, bluePercent * color.b, color.a);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}