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
float4 DiffuseColor;
float SpecularPower;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 WorldPos : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float4 ProjPos : TEXCOORD3;
};

struct PixelShaderOutput
{
    float4 Position : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Albedo : SV_Target2;
    float4 Depth : SV_Target3;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    float4 worldPos = mul(input.Position, World);
    output.WorldPos = worldPos;
    
    float4 projPos = mul(worldPos, mul(View, Projection));
    output.Position = projPos;
    output.ProjPos = projPos;
    
    output.Normal = normalize(mul(input.Normal, (float3x3) World));
    output.TexCoord = input.TexCoord;
    
    return output;
}

PixelShaderOutput MainPS(VertexShaderOutput input)
{
    PixelShaderOutput output;
    
    output.Position = input.WorldPos;
    output.Normal = float4(normalize(input.Normal), 1.0);
    output.Albedo = float4(DiffuseColor.rgb, SpecularPower / 255.0);
    
    float depth = input.ProjPos.z / input.ProjPos.w;
    output.Depth = float4(depth, depth, depth, 1.0);
    
    return output;
}

technique GBufferPass
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}