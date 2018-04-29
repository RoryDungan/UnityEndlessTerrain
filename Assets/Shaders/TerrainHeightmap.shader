Shader "Custom/TerrainHeightmap"
{
    Properties
    {
        _PosX ("Pos X", Float) = 0.0
        _PosY ("Pos Y", Float) = 0.0
        _Scale ("Scale", Float) = 1.0
    }
    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM

            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "ClassicNoise2D.hlsl"

            float _PosX;
            float _PosY;
            float _Scale;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.globalTexcoord + float2(_PosX, _PosY);

                float o = 0.5;
                float s = _Scale;
                float w = 0.5;

                for (int i = 0; i < 6; i++)
                {
                    float2 coord = uv * s;
                    float2 period = s * 2.0;

                    o += cnoise(coord) * w;

                    s *= 2.0;
                    w *= 0.5;
                }

                return float4(o, o, o, 1);
            }

            ENDCG
        }
    }
}
