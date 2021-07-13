Shader "AppsTools/FastShader/Effect/Effect_FresnelRef"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_fresnelBase("fresnelBase", Range(0, 1)) = 1
		_fresnelScale("fresnelScale", Range(0, 1)) = 1
		_fresnelIndensity("fresnelIndensity", Range(0, 5)) = 5
		_fresnelCol("_fresnelCol", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
		[Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 0
		//[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0  */
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 1
	}

		SubShader
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"  }
			LOD 100
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			ColorMask RGB
			Lighting Off ZWrite Off
			Cull[_Cull]

			Pass
			{
				tags{ "lightmode=" = "forward" }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "Lighting.cginc"

				struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 L : TEXCOORD1;
				float3 N : TEXCOORD2;
				float3 V : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _fresnelBase;

			float _fresnelScale;

			float _fresnelIndensity;

			float4 _fresnelCol;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.N = mul(v.normal, (float3x3)unity_WorldToObject);
				o.L = WorldSpaceLightDir(v.vertex);
				o.V = WorldSpaceViewDir(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 N = normalize(i.N);
				float3 L = normalize(i.L);
				float3 V = normalize(i.V);

				col.rgb *= _LightColor0.rgb;

				float fresnel = _fresnelBase + _fresnelScale * pow(1 - dot(N, V), _fresnelIndensity);

				col.rgb += lerp(col.rgb, _fresnelCol, fresnel) * _fresnelCol.a;

			return col;
			}

			ENDCG
		}
	}
}