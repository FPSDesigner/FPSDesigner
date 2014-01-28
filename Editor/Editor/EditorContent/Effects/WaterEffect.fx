float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float4x4 ReflectedView;

texture ReflectionMap;

float3 BaseColor = float3(0.2, 0.2, 0.0);
float BaseColorAmount = 0.1f;

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

float WaveLength = 0.1;
float WaveHeight = 0.1;
float Time = 0;
float WaveSpeed = 0.04f;
float Alpha = 0;

float FogStart = 1.5;
float FogEnd = 5.5;
float3 FogColor = float3(1, 1, 1);
float FogScale = 1.2;

float FogStartUw = 0.5;
float FogEndUw = 1;
float3 FogColorUw = float3(0.0588,0.156,0.1607);

bool IsUnderWater = false;
//float3 BaseColorUnderWater = float3(0.0588, 0.1568, 0.16);
float3 BaseColorUnderWater = float3(1,1,1);
float AlphaUnderWater = 0.2f;

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
	float Depth : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Depth = output.Position.z;
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

	

	if(IsUnderWater)
	{
		float fog = clamp((input.Depth*0.3 - FogStartUw) / (FogEndUw - FogStartUw), 0, 1);
		return float4(lerp((lerp(reflection, BaseColorUnderWater, AlphaUnderWater) + specular), FogColorUw, fog), 0.9+clamp(input.Depth*0.002, 0, 0.1));
	}
	else
	{
		float fog = clamp((input.Depth*0.01 - FogStart) / (FogEnd - FogStart), 0, 0.8);
		return float4(lerp((lerp(reflection, BaseColor, BaseColorAmount) + specular), FogColor, fog), Alpha);
	}
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
