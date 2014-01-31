// Effect uses a scrolling displacement texture to offset the position of the main
// texture. Depending on the contents of the displacement texture, this can give a
// wide range of refraction, rippling, warping, and swirling type effects.

float2 DisplacementScroll;

sampler TextureSampler : register(s0);
sampler DisplacementSampler : register(s1);


float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the displacement amount.
    float2 displacement = tex2D(DisplacementSampler, DisplacementScroll + texCoord / 5);
    
    // Offset the main texture coordinates.
    texCoord += displacement * 0.2 - 0.15;
    
    // Look up into the main texture.
    return tex2D(TextureSampler, texCoord) * color;
}


technique Refraction
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}




/*sampler2D tex[1];

float2 Offsets[15];
float Weights[15];

float4 PixelShaderFunction(float4 Position : POSITION0, 
	float2 UV : TEXCOORD0) : COLOR0
{
    float4 output = float4(0, 0, 0, 1);
    
    for (int i = 0; i < 15; i++)
	output += tex2D(tex[0], UV + Offsets[i]) * Weights[i];
	
	return float4(output.r, output.g, 1.5f * output.b, output.a);
}

technique Technique1
{
    pass p0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}*/