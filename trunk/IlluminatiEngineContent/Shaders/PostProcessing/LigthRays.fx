#include "PPVertexShader.fx"

#define NUM_SAMPLES 128

float3 lightPosition;
float3 cameraPosition;

float4x4 matVP;

float2 halfPixel;

float Density = .5f;
float Decay = .95f;
float Weight = 1.0f;
float Exposure = .15f;

sampler2D Scene: register(s0){
	AddressU = Clamp;
	AddressV = Clamp;
};


float4 lightRayPS( float2 texCoord : TEXCOORD0 ) : COLOR0
{
	// Find light pixel position
	float4 ScreenPosition = mul(lightPosition - cameraPosition, matVP);
	ScreenPosition.xyz /= ScreenPosition.w;
	ScreenPosition.x = ScreenPosition.x/2.0f+0.5f;
	ScreenPosition.y = (-ScreenPosition.y/2.0f+0.5f);

	float2 TexCoord = texCoord - halfPixel;

	float2 DeltaTexCoord = (TexCoord - ScreenPosition.xy);
	DeltaTexCoord *= (1.0f / NUM_SAMPLES * Density);

	DeltaTexCoord = DeltaTexCoord * clamp(ScreenPosition.w * ScreenPosition.z,0,.5f);

	float3 col = tex2D(Scene,TexCoord);
	float IlluminationDecay = 1.0;
	float3 Sample;
	
	for( int i = 0; i < NUM_SAMPLES; ++i )
	{
		TexCoord -= DeltaTexCoord;
		Sample = tex2D(Scene, TexCoord);
		Sample *= IlluminationDecay * Weight;
		col += Sample;
		IlluminationDecay *= Decay;			
	}

	return float4(col * Exposure,1);
	if(ScreenPosition.w > 0)
		return float4(col * Exposure,1) * (ScreenPosition.w * .0025);
	else
		return 0;
	
}

technique LightRayFX
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 lightRayPS();
	}
}

