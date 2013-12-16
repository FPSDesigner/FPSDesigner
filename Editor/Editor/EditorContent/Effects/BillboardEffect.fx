float4x4 View;
float4x4 Projection;

texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};

float2 Size;
float3 Up; // Camera's 'up' vector
float3 Side; // Camera's 'side' vector

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.95f;
float DrawingDistance = 0.2f;

// Lighting parameters.
float3 LightDirection;
float3 LightColor = 0.8;
float3 AmbientColor = 0.4;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	//float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	//float Random : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
	float4 Color : COLOR0;
};


float3 WindDirection = float3(1, 0, 0);
float WindWaveSize = 0.1;
float WindRandomness = 1;
float WindSpeed = 4;
float WindAmount = 0;
float WindTime = 0;

// Parameters describing the billboard itself.
float BillboardWidth = 10;
float BillboardHeight = 10;

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	/*// Apply a scaling factor to make some of the billboards
    // shorter and fatter while others are taller and thinner.

    //float squishFactor = 0.75 + abs(input.Random) / 2;
	float squishFactor = 0.75 + abs(0.99) / 2;

	float width = BillboardWidth * squishFactor;
    float height = BillboardHeight / squishFactor;

    // Flip half of the billboards from left to right. This gives visual variety
    // even though we are actually just repeating the same texture over and over.

    ////if (input.Random < 0)
    ////   width = -width;

	// Work out what direction we are viewing the billboard from.
    float3 viewDirection = View._m02_m12_m22;

	//float3 rightVector = normalize(cross(viewDirection, input.Normal));
	float3 rightVector = 1;

	// Calculate the position of this billboard vertex.
    float3 position = input.Position;

    // Offset to the left or right.
    position += rightVector * (input.TexCoord.x - 0.5) * width;
    
    // Offset upward if we are one of the top two vertices.
    //position += input.Normal * (1 - input.TexCoord.y) * height;
	position += (1 - input.TexCoord.y) * height;

    // Work out how this vertex should be affected by the wind effect.
    float waveOffset = dot(position, WindDirection) * WindWaveSize;
    
    //waveOffset += input.Random * WindRandomness;
	waveOffset += 0.99 * WindRandomness;
    
    // Wind makes things wave back and forth in a sine wave pattern.
    float wind = sin(WindTime * WindSpeed + waveOffset) * WindAmount;
    
    // But it should only affect the top two vertices of the billboard!
    wind *= (1 - input.TexCoord.y);
    
    position += WindDirection * wind;

    // Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), View);

    output.Position = mul(viewPosition, Projection);
	output.Depth = output.Position.z;

    output.TexCoord = input.TexCoord;
    
    // Compute lighting.
    //float diffuseLight = max(-dot(input.Normal, LightDirection), 0);
	float diffuseLight = 1;
    
    output.Color.rgb = diffuseLight * LightColor + AmbientColor;
    output.Color.a = 1;
    
    return output;

	*/

	
	// Determine which corner of the rectangle this vertex
	// represents
	float2 offset = float2(
		(input.TexCoord.x - 0.5f) * 2.0f, 
		-(input.TexCoord.y - 0.5f) * 2.0f
	);

	float3 position = input.Position;

	// Move the vertex along the camera's 'plane' to its corner
	position += offset.x * Size.x * Side + offset.y * Size.y * Up;

	// Transform the position by view and projection
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.Depth = output.Position.z;

	output.TexCoord = input.TexCoord;

	output.Color.rgb = 1;
	output.Color.a = 1;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(texSampler, input.TexCoord);

	if (AlphaTest)
		clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));

	return clamp(1/(DrawingDistance * input.Depth), 0, 1) * color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
