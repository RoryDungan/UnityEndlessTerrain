Shader "Custom/Terrain"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _HeightMultiplier("Height multiplier", Float) = 20.0
        _VertexSpacing("Vertex spacing", Float) = 0.05
        _Hilliness("Hilliness", Float) = 0.01
    }
    SubShader
    {

        Pass
        {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            // compile shader into multiple variants with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc" // shadow helper functions and macros

            #include "TerrainNoise.hlsl"

            // make fog work
            #pragma multi_compile_fog

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                fixed3 diff : COLOR0; // diffuse lighting color
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
                UNITY_FOG_COORDS(2) // Used to pass fog amount around
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _HeightMultiplier;
            float _VertexSpacing;
            float _Hilliness;

            v2f vert (appdata_base v)
            {
                v2f o;

                float4 v0 = float4(v.vertex.x, v.vertex.y, v.vertex.z, 1);
                float2 v0WorldXZ = mul(unity_ObjectToWorld, v0).xz;
                v0.y = terrainNoise(v0WorldXZ, _Hilliness).x * _HeightMultiplier;

                // Create two fake neightbour vertices in order to calculate the normal
                float4 v1 = v0 + float4(_VertexSpacing, 0.0, 0.0, 0.0); // +X
                v1.y = terrainNoise(v0WorldXZ + float2(_VertexSpacing, 0), _Hilliness).x * _HeightMultiplier;
                float4 v2 = v0 + float4(0.0, 0.0, _VertexSpacing, 0.0); // +Z
                v2.y = terrainNoise(v0WorldXZ + float2(0, _VertexSpacing), _Hilliness).x * _HeightMultiplier;
                
                float3 vertexNormal = normalize(cross(v2 - v0, v1 - v0).xyz);

                o.pos = UnityObjectToClipPos(v0.xyz);

                o.uv = v.texcoord;

                // get the vertex normal in world space
                half3 worldNormal = (half3) vertexNormal;

                // dot product between the normal and light direction for
                // standard diffuse (Lambert) lighting

                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal, 1));
                //o.diff = worldNormal;

                // compute shadows data
                TRANSFER_SHADOW(o)

                // Compute fog amount from clip space position.
                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //return fixed4(i.diff.x, i.diff.y, i.diff.z, 1);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;

                // multiply by lighting
                col.rgb *= lighting;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Shadow caster pass
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }

            ENDCG
        }
    }
}
