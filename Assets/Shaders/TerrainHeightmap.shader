Shader "Custom/TerrainHeightmap"
{
	Properties
	{
		_PosX ("Pos X", Int) = 0
		_PosY ("Pos Y", Int) = 0
		_L0Weight ("Weight level 0", Float) = 400
		_L0Scale ("Scale level 0", Int) = 1
		_L1Weight ("Weight level 1", Float) = 200
		_L1Scale ("Scale level 1", Int) = 4
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

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				return IN.localTexcoord.xyxy;
			}

			ENDCG
		}
	}
}
