#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;
matrix LightViewProjection;
            
float3 LightDirection;
float3 LightColor;
float3 AmbientColor;
float3 LightPosition;

Texture2D ShadowMap;
sampler2D ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};
            
float ShadowMapSize = 2048.0;
float ShadowBias = 0.0005;

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 LightSpacePosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
                
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
                
    output.WorldPosition = worldPosition;
    output.Normal = normalize(mul(input.Normal, (float3x3) World));
    output.LightSpacePosition = mul(worldPosition, LightViewProjection);
                
    return output;
}

float CalculateShadow(float4 lightSpacePosition)
{
    float3 projCoords = lightSpacePosition.xyz / lightSpacePosition.w;
    float currentDepth = projCoords.z * 0.5 + 0.5;
    
    projCoords.x = projCoords.x * 0.5 + 0.5;
    projCoords.y = projCoords.y * -0.5 + 0.5;
    
    if (projCoords.x < 0 || projCoords.x > 1 ||
        projCoords.y < 0 || projCoords.y > 1 ||
        currentDepth > 1.0 || currentDepth < 0.0)
        return 1.0;
    
    float bias = ShadowBias;
    float shadow = 0.0;
    float texelSize = 1.0 / ShadowMapSize;
    
    int sampleRange = 2;
    int sampleCount = 0;
    
    for (int x = -sampleRange; x <= sampleRange; x++)
    {
        for (int y = -sampleRange; y <= sampleRange; y++)
        {
            float2 offset = float2(x, y) * texelSize;
            float shadowDepth = tex2D(ShadowMapSampler, projCoords.xy + offset).r;
            shadow += currentDepth - bias > shadowDepth ? 0.0 : 1.0;
            sampleCount++;
        }
    }
    
    return shadow / (float) sampleCount;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 normal = normalize(input.Normal);
    float3 lightDir = normalize(-LightDirection);
    
    float diffuse = max(dot(normal, lightDir), 0.0);
    float shadow = CalculateShadow(input.LightSpacePosition);
    
    float3 ambient = AmbientColor * 0.3;
    float3 lighting = ambient + (LightColor * diffuse * shadow);
                
    return float4(lighting, 1.0);
}

technique GeometryPassTechnique
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}