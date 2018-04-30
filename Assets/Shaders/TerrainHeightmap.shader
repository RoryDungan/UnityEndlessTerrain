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
            #include "TerrainNoise.hlsl"

            float _PosX;
            float _PosY;
            float _Scale;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.globalTexcoord + float2(_PosX, _PosY);

                float o = terrainNoise(uv, _Scale);

                return float4(o, o, o, 1);
            }

            ENDCG
        }
    }
}
