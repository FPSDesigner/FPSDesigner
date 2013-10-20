sampler2D tex[1];

float2 Offsets[15];
float Weights[15];

float4 PixelShaderFunction(float4 Position : POSITION0, 
	float2 UV : TEXCOORD0) : COLOR0
{
    float4 output = float4(0, 0, 0, 1);
    
    for (int i = 0; i < 15; i++)
	output += tex2D(tex[0], UV + Offsets[i]) * Weights[i];
		
	return output;
}

technique Technique1
{
    pass p0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}