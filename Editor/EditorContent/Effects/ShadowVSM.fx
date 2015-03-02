float4x4 World;
float4x4 View;
float4x4 Projection;

bool DoShadowMapping = true;
float4x4 ShadowView;
float4x4 ShadowProjection;
float3 ShadowLightPosition;
float ShadowFarPlane = 10000;
float ShadowMult = 0.3f;
float ShadowBias = 1.0f / 40.0f;

texture2D BasicTexture;
sampler2D basicTextureSampler = sampler_state { 
	texture = <BasicTexture>; 
	addressU = wrap; 
	addressV = wrap; 
	minfilter = anisotropic;
	magfilter = anisotropic;
	mipfilter = linear;
};
bool TextureEnabled = true;

texture2D LightTexture;
sampler2D lightSampler = sampler_state { 
	texture = <LightTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

texture2D ShadowMap;
sampler2D shadowSampler = sampler_state {
	texture = <ShadowMap>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

float3 AmbientColor = float3(0.15, 0.15, 0.15);
float3 DiffuseColor;

#include "PPShared.vsi"

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float4 PositionCopy : TEXCOORD1;
	float4 ShadowScreenPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 worldViewProjection = mul(World, mul(View, Projection));
	
	output.Position = mul(input.Position, worldViewProjection);
	output.PositionCopy = output.Position;
    
	output.UV = input.UV;

	output.ShadowScreenPosition = mul(mul(input.Position, World), 
		mul(ShadowView, ShadowProjection));

	return output;
}

float2 sampleShadowMap(float2 UV)
{
	if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1)
		return float2(1, 1);

	return tex2D(shadowSampler, UV).rg;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Sample model's texture
	float3 basicTexture = tex2D(basicTextureSampler, input.UV);
	
	if (!TextureEnabled)
		basicTexture = float4(1, 1, 1, 1);
		
	// Extract lighting value from light map
	float2 texCoord = postProjToScreen(input.PositionCopy) + halfPixel();
	float3 light = tex2D(lightSampler, texCoord);
	
	light += AmbientColor;

	float shadow = 1;

	if (DoShadowMapping)
	{
		float2 shadowTexCoord = postProjToScreen(input.ShadowScreenPosition)
			+ halfPixel();

		float realDepth = input.ShadowScreenPosition.z / ShadowFarPlane
			- ShadowBias;

		if (realDepth < 1)
		{
			// Variance shadow mapping code below from the variance shadow
			// mapping demo code @ http://www.punkuser.net/vsm/

			// Sample from depth texture
			float2 moments = sampleShadowMap(shadowTexCoord);

			// Check if we're in shadow
			float lit_factor = (realDepth <= moments.x);
    
			// Variance shadow mapping
			float E_x2 = moments.y;
			float Ex_2 = moments.x * moments.x;
			float variance = min(max(E_x2 - Ex_2, 0.0) + 1.0f / 10000.0f, 1.0);
			float m_d = (moments.x - realDepth);
			float p = variance / (variance + m_d * m_d);

			shadow = clamp(max(lit_factor, p), ShadowMult, 1.0f);
		}
	}

	return float4(basicTexture * DiffuseColor * light * shadow, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
