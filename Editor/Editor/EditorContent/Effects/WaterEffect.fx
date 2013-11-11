float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float4x4 ReflectedView;

texture ReflectionMap;

float3 BaseColor = float3(0.2, 0.2, 0.0);
float BaseColorAmount = 0.3f;

float3 LightDirection = float3(1, 1, 1);

sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>; 
	MinFilter = Anisotropic; 
	MagFilter = Anisotropic;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture WaterNormalMap;

sampler2D waterNormalSampler = sampler_state {
	texture = <WaterNormalMap>; 
	MinFilter = Anisotropic; 
	MagFilter = Anisotropic;
};

float WaveLength = 0.2;
float WaveHeight = 0.15;
float Time = 0;
float WaveSpeed = 0.04f;
float Alpha = 0.9f;

#include "PPShared.vsi"

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ReflectionPosition : TEXCOORD1;
    float2 NormalMapPosition : TEXCOORD2;
    float4 WorldPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    
    float4x4 rwvp = mul(World, mul(ReflectedView, Projection));
    output.ReflectionPosition = mul(input.Position, rwvp);
    
	output.NormalMapPosition = input.UV / WaveLength;
	output.NormalMapPosition.y -= Time * WaveSpeed;
	
	output.WorldPosition = mul(input.Position, World);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 reflectionUV = postProjToScreen(input.ReflectionPosition)
		+ halfPixel();
		
	float4 normal = tex2D(waterNormalSampler, input.NormalMapPosition) * 2 - 1;
	float2 UVOffset = WaveHeight * normal.rg;

	float3 reflection = tex2D(reflectionSampler, reflectionUV + UVOffset);
	 
	float3 viewDirection = normalize(CameraPosition - input.WorldPosition);

	float3 reflectionVector = -reflect(LightDirection, normal.rgb);
	float specular = dot(normalize(reflectionVector), viewDirection);
	specular = pow(specular, 256);

	return float4(lerp(reflection, BaseColor, BaseColorAmount)
	 + specular, Alpha);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
