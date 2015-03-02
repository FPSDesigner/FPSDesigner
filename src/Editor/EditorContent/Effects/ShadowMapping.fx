float4x4 World;
float4x4 View;
float4x4 Projection;

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

bool DoShadowMapping = true;
float4x4 ShadowView;
float4x4 ShadowProjection;
float3 ShadowLightPosition;
float ShadowFarPlane;
float ShadowMult = 0.3f;
float ShadowBias = 1.0f / 50.0f;

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

float sampleShadowMap(float2 UV)
{
	if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1)
		return 1;

	return tex2D(shadowSampler, UV).r;
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

	float2 shadowTexCoord = postProjToScreen(input.ShadowScreenPosition)
		+ halfPixel();

	float mapDepth = sampleShadowMap(shadowTexCoord);
	float realDepth = input.ShadowScreenPosition.z / ShadowFarPlane;
	float shadow = 1;

	if (realDepth < 1 && realDepth - ShadowBias > mapDepth)
		shadow = ShadowMult;

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
