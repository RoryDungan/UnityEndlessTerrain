// Simple terrain noise generator

#include "ClassicNoise2D.hlsl"

float terrainNoise(float2 uv, float scale)
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