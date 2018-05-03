// Simple terrain noise generator

#include "ClassicNoise2D.hlsl"

float terrainNoiseHeight(float2 uv, float scale)
{
    float o = 0.5;
    float s = scale;
    float w = 0.5;

    for (int i = 0; i < 6; i++)
    {
        float2 coord = uv * s;
        float2 period = s * 2.0;

        o += cnoise(coord) * w;

        s *= 2.0;
        w *= 0.5;
    }

    return o;
}

// Struct containing point height and normal.
struct TerrainPoint
{
    float height;
    float3 normal;
};

TerrainPoint terrainNoise(
    float2 uv, 
    float hilliness, 
    float heightMultiplier, 
    float normalSmoothing
)
{
    TerrainPoint o;

    o.height = terrainNoiseHeight(uv, hilliness).x * heightMultiplier;
    float4 v0 = float4(uv.x, o.height, uv.y, 1.0);

    // Create two fake neightbour vertices in order to calculate the normal
    float4 v1 = v0 + float4(normalSmoothing, 0.0, 0.0, 0.0); // +X
    v1.y = terrainNoiseHeight(uv + float2(normalSmoothing, 0), hilliness).x * heightMultiplier;
    float4 v2 = v0 + float4(0.0, 0.0, normalSmoothing, 0.0); // +Z
    v2.y = terrainNoiseHeight(uv + float2(0, normalSmoothing), hilliness).x * heightMultiplier;

    o.normal = normalize(cross(v2 - v0, v1 - v0).xyz);

    return o;
}