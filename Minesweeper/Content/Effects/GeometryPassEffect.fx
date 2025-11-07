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

Texture2D ShadowMap;
sampler2D ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};
            
float ShadowMapSize = 2048.0;

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

static const float2 poissonDisk[64] =
{
    float2(-0.613392, 0.617481),
    float2(0.170019, -0.040254),
    float2(-0.299417, 0.791925),
    float2(0.645680, 0.493210),
    float2(-0.651784, 0.717887),
    float2(0.421003, 0.027070),
    float2(-0.817194, -0.271096),
    float2(-0.705374, -0.668203),
    float2(0.977050, -0.108615),
    float2(0.063326, 0.142369),
    float2(0.203528, 0.214331),
    float2(-0.667531, 0.326090),
    float2(-0.098422, -0.295755),
    float2(-0.885922, 0.215369),
    float2(0.566637, 0.605213),
    float2(0.039766, -0.396100),
    float2(0.751946, 0.453352),
    float2(0.078707, -0.715323),
    float2(-0.075838, -0.529344),
    float2(0.724479, -0.580798),
    float2(0.222999, -0.215125),
    float2(-0.467574, -0.405438),
    float2(-0.248268, -0.814753),
    float2(0.354411, -0.887570),
    float2(0.175817, 0.382366),
    float2(0.487472, -0.063082),
    float2(-0.084078, 0.898312),
    float2(0.488876, -0.783441),
    float2(0.470016, 0.217933),
    float2(-0.696890, -0.549791),
    float2(-0.149693, 0.605762),
    float2(0.034211, 0.979980),
    float2(0.503098, -0.308878),
    float2(-0.016205, -0.872921),
    float2(0.385784, -0.393902),
    float2(-0.146886, -0.859249),
    float2(0.643361, 0.164098),
    float2(0.634388, -0.049471),
    float2(-0.688894, 0.007843),
    float2(0.464034, -0.188818),
    float2(-0.440840, 0.137486),
    float2(0.364483, 0.511704),
    float2(0.034028, 0.325968),
    float2(0.099094, -0.308023),
    float2(0.693960, -0.366253),
    float2(0.678884, -0.204688),
    float2(0.001801, 0.780328),
    float2(0.145177, -0.898984),
    float2(0.062655, -0.611866),
    float2(0.315226, -0.604297),
    float2(-0.780145, 0.486251),
    float2(-0.371868, 0.882138),
    float2(0.200476, 0.494430),
    float2(-0.494552, -0.711051),
    float2(0.612476, 0.705252),
    float2(-0.578845, -0.768792),
    float2(-0.772454, -0.090976),
    float2(0.504440, 0.372295),
    float2(0.155736, 0.065157),
    float2(0.391522, 0.849605),
    float2(-0.620106, -0.328104),
    float2(0.789239, -0.419965),
    float2(-0.545396, 0.538133),
    float2(-0.178564, -0.596057)
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
    
    int numSamples = 16;
    
    for (int i = 0; i < numSamples; i++)
    {
        int index = int(64.0 * random(floor(worldPos * 1000.0), i)) % 64;
        
        float2 offset = poissonDisk[index] * texelSize * 0.75;
        float shadowDepth = tex2D(ShadowMapSampler, projCoords.xy + offset).r;
        
        shadow += (currentDepth > shadowDepth ? 0.2 : 1.0);
    }
    
    return shadow / float(numSamples);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 normal = normalize(input.Normal);
    float3 lightDir = normalize(-LightDirection);
    
    float diffuse = max(dot(normal, lightDir), 0.0);
    float shadow = CalculateShadow(input.LightSpacePosition, normal, input.WorldPosition.xyz);
    
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