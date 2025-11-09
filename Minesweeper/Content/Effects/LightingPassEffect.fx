#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D PositionTexture;
Texture2D NormalTexture;
Texture2D AlbedoTexture;
Texture2D ShadowMap;

sampler2D PositionSampler = sampler_state
{
    Texture = <PositionTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

sampler2D NormalSampler = sampler_state
{
    Texture = <NormalTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

sampler2D AlbedoSampler = sampler_state
{
    Texture = <AlbedoTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

sampler2D ShadowSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float ShadowMapSize = 2048.0;
float ShadowIntensity = 1.0;

float3 LightDirection;
float3 LightColor;
float3 AmbientColor;
float3 CameraPosition;
matrix LightViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

static const float2 poissonDisk[16] =
{
    float2(-0.94201624, -0.39906216),
    float2(0.94558609, -0.76890725),
    float2(-0.094184101, -0.92938870),
    float2(0.34495938, 0.29387760),
    float2(-0.91588581, 0.45771432),
    float2(-0.81544232, -0.87912464),
    float2(-0.38277543, 0.27676845),
    float2(0.97484398, 0.75648379),
    float2(0.44323325, -0.97511554),
    float2(0.53742981, -0.47373420),
    float2(-0.26496911, -0.41893023),
    float2(0.79197514, 0.19090188),
    float2(-0.24188840, 0.99706507),
    float2(-0.81409955, 0.91437590),
    float2(0.19984126, 0.78641367),
    float2(0.14383161, -0.14100790)
};

float random(float3 seed, int i)
{
    float4 seed4 = float4(seed, i);
    float dotProduct = dot(seed4, float4(12.9898, 78.233, 45.164, 94.673));
    return frac(sin(dotProduct) * 43758.5453);
}

float CalculateShadow(float4 lightSpacePosition, float3 normal, float3 worldPos)
{
    float3 projCoords = lightSpacePosition.xyz;
    float currentDepth = projCoords.z * 0.5 + 0.5;
    
    projCoords.x = projCoords.x * 0.5 + 0.5;
    projCoords.y = projCoords.y * -0.5 + 0.5;
    
    if (projCoords.x < 0 || projCoords.x > 1 ||
        projCoords.y < 0 || projCoords.y > 1 ||
        currentDepth > 1.0 || currentDepth < 0.0)
        return 1.0;
    
    float shadow = 0.0;
    float texelSize = 1.0 / ShadowMapSize;
    
    float angle = random(floor(worldPos * 1000.0), 0) * 6.28318;
    float2x2 rotation = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
    
    int numSamples = 16;
    
    for (int i = 0; i < numSamples; i++)
    {
        float2 offset = mul(poissonDisk[i % 16], rotation) * texelSize * 0.75;
        float shadowDepth = tex2D(ShadowSampler, projCoords.xy + offset).r;
        
        shadow += (currentDepth > shadowDepth ? 0.2 : 1.0);
    }
    
    return shadow / float(numSamples);
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 position = tex2D(PositionSampler, input.TexCoord);
    float4 normal = tex2D(NormalSampler, input.TexCoord);
    float4 albedo = tex2D(AlbedoSampler, input.TexCoord);
    
    if (length(position.xyz) < 0.001)
        return float4(0, 0, 0, 1);
    
    float3 worldPos = position.xyz;
    float3 N = normalize(normal.xyz);
    float3 L = normalize(-LightDirection);
    float3 V = normalize(CameraPosition - worldPos);
    float3 H = normalize(L + V);
    
    float NdotL = max(0.0, dot(N, L));
    float3 diffuse = albedo.rgb * LightColor * NdotL;
    
    float specularPower = albedo.a * 255.0;
    float NdotH = max(0.0, dot(N, H));
    float3 specular = LightColor * pow(NdotH, specularPower);
    
    float3 ambient = albedo.rgb * AmbientColor;
    
    float4 lightSpacePos = mul(float4(worldPos, 1.0), LightViewProjection);
    lightSpacePos.xyz /= lightSpacePos.w;
    float shadow = CalculateShadow(lightSpacePos, N, worldPos);
    
    float3 finalColor = ambient + (diffuse + specular) * (shadow * ShadowIntensity + (1.0 - ShadowIntensity));
    
    return float4(finalColor, 1.0);
}

technique LightingPass
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}