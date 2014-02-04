float4x4 View;
float4x4 Projection;

float3 LightDirection = float3(1, -1, 0);
float TextureTiling = 1;
float LightIntensity = 1;

float4 ClipPlane;
bool ClipPlaneEnabled = false;

texture RTexture;
sampler RTextureSampler = sampler_state {
	texture = <RTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture GTexture;
sampler GTextureSampler = sampler_state {
	texture = <GTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture BTexture;
sampler BTextureSampler = sampler_state {
	texture = <BTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture BaseTexture;
sampler BaseTextureSampler = sampler_state {
	texture = <BaseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture WeightMap;
sampler WeightMapSampler = sampler_state {
	texture = <WeightMap>;
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float Depth : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
};

float DetailTextureTiling;
float DetailDistance = 2500;

float FogStart = 1.5;
float FogEnd = 5.5;
bool FogWaterActivated = true;
bool IsUnderWater = false;
float FogWaterHeight = 44.5;
float3 FogColor = float3(1,1,1);
float3 FogColorWater = float3(0.0588,0.156,0.1607);
float3 ShoreColor = float3(1,1,1);

texture DetailTexture;
sampler DetailSampler = sampler_state {
	texture = <DetailTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Linear;
	MagFilter = Linear;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position, mul(View, Projection));
	output.Normal = input.Normal;
	output.UV = input.UV;
	output.Depth = output.Position.z;
	output.WorldPosition = input.Position;

    return output;
}


// !fog && !underwater
float4 PixelShaderFunctionTechnique1(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	float light = dot(normalize(input.Normal), normalize(LightDirection)) * LightIntensity;
	light = clamp(light + 0.4f, 0, 1);

	float3 rTex = tex2D(RTextureSampler, input.UV * TextureTiling);
	float3 gTex = tex2D(GTextureSampler, input.UV * TextureTiling);
	float3 bTex = tex2D(BTextureSampler, input.UV * TextureTiling);
	float3 base = tex2D(BaseTextureSampler, input.UV * TextureTiling);

	float3 weightMap = tex2D(WeightMapSampler, input.UV);

	float3 output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1)
					* base
					+ weightMap.r * rTex + weightMap.g * gTex + weightMap.b * bTex;

	float3 detail = tex2D(DetailSampler, input.UV * DetailTextureTiling);
	float detailAmt = input.Depth / DetailDistance;

	detail = lerp(detail, 1, clamp(detailAmt, 0, 1));

	float shore = 0;
	if(input.WorldPosition.y > FogWaterHeight)
		shore = clamp(0.4*(1/(input.WorldPosition.y - FogWaterHeight)), 0, 0.5);

	float fog = clamp((input.Depth*0.01 - FogStart) / (FogEnd - FogStart), 0, 0.8);
	return float4(lerp(lerp( output * light, FogColor, fog), ShoreColor, shore), 1);
}


// fog && !underwater
float4 PixelShaderFunctionTechnique2(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	float light = dot(normalize(input.Normal), normalize(LightDirection)) * LightIntensity;
	light = clamp(light + 0.4f, 0, 1);

	float3 rTex = tex2D(RTextureSampler, input.UV * TextureTiling);
	float3 gTex = tex2D(GTextureSampler, input.UV * TextureTiling);
	float3 bTex = tex2D(BTextureSampler, input.UV * TextureTiling);
	float3 base = tex2D(BaseTextureSampler, input.UV * TextureTiling);

	float3 weightMap = tex2D(WeightMapSampler, input.UV);

	float3 output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1)
					* base
					+ weightMap.r * rTex + weightMap.g * gTex + weightMap.b * bTex;

	float3 detail = tex2D(DetailSampler, input.UV * DetailTextureTiling);
	float detailAmt = input.Depth / DetailDistance;

	detail = lerp(detail, 1, clamp(detailAmt, 0, 1));

	float shore = 0;
	if(input.WorldPosition.y > FogWaterHeight)
		shore = clamp(0.4*(1/(input.WorldPosition.y - FogWaterHeight)), 0, 0.5);

	if(input.WorldPosition.y-0.1 < FogWaterHeight)
	{
		float fog = clamp(input.Depth*0.005*(FogWaterHeight - input.WorldPosition.y), 0, 1);
		return float4(lerp(lerp( output * light, FogColorWater, fog), ShoreColor, shore), 1);
	}
	else
	{
		float fog = clamp((input.Depth*0.01 - FogStart) / (FogEnd - FogStart), 0, 0.8);
		return float4(lerp(lerp( output * light, FogColor, fog), ShoreColor, shore), 1);
	}
	
}

// !fog && underwater
float4 PixelShaderFunctionTechnique3(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	float light = dot(normalize(input.Normal), normalize(LightDirection)) * LightIntensity;
	light = clamp(light + 0.4f, 0, 1);

	float3 rTex = tex2D(RTextureSampler, input.UV * TextureTiling);
	float3 gTex = tex2D(GTextureSampler, input.UV * TextureTiling);
	float3 bTex = tex2D(BTextureSampler, input.UV * TextureTiling);
	float3 base = tex2D(BaseTextureSampler, input.UV * TextureTiling);

	float3 weightMap = tex2D(WeightMapSampler, input.UV);

	float3 output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1)
					* base
					+ weightMap.r * rTex + weightMap.g * gTex + weightMap.b * bTex;

	float3 detail = tex2D(DetailSampler, input.UV * DetailTextureTiling);
	float detailAmt = input.Depth / DetailDistance;

	detail = lerp(detail, 1, clamp(detailAmt, 0, 1));

	float fog = clamp((input.Depth*0.01 - FogStart) / (FogEnd - FogStart), 0, 0.8);
	
	return float4(lerp( output * light, FogColor, fog), 1);
	
}


// fog && underwater
float4 PixelShaderFunctionTechnique4(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	float light = dot(normalize(input.Normal), normalize(LightDirection)) * LightIntensity;
	light = clamp(light + 0.4f, 0, 1);

	float3 rTex = tex2D(RTextureSampler, input.UV * TextureTiling);
	float3 gTex = tex2D(GTextureSampler, input.UV * TextureTiling);
	float3 bTex = tex2D(BTextureSampler, input.UV * TextureTiling);
	float3 base = tex2D(BaseTextureSampler, input.UV * TextureTiling);

	float3 weightMap = tex2D(WeightMapSampler, input.UV);

	float3 output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1)
					* base
					+ weightMap.r * rTex + weightMap.g * gTex + weightMap.b * bTex;

	float3 detail = tex2D(DetailSampler, input.UV * DetailTextureTiling);
	float detailAmt = input.Depth / DetailDistance;

	detail = lerp(detail, 1, clamp(detailAmt, 0, 1));

	if(input.WorldPosition.y-0.1 < FogWaterHeight)
	{
		float fog = clamp(input.Depth*0.02, 0, 1);
		return float4(lerp( output * light, FogColorWater, fog), 1);
	}
	else
	{
		float fog = clamp((input.Depth*0.01 - FogStart) / (FogEnd - FogStart), 0, 0.8);
		return float4(lerp( output * light, FogColor, fog), 1);
	}
}

/********************** We create 4 techniques: ***********************/
// fog && underwater, fog && !underwater, !fog && underwater, !fog && !underwater

// !fog && !underwater
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionTechnique1();
    }
}

// fog && !underwater
technique Technique2
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionTechnique2();
    }
}

// !fog && underwater
technique Technique3
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionTechnique3();
    }
}

// fog && underwater
technique Technique4
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionTechnique4();
    }
}
