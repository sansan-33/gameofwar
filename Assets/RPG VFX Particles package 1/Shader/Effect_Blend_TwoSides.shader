Shader "AppsTools/FastShader/Effect/Blend_TwoSides"
{
	Properties{

      	_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("Main Tex", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_FrontFacesColor("Front Faces Color", Color) = (0,0.2313726,1,1)
		_BackFacesColor("Back Faces Color", Color) = (0.1098039,0.4235294,1,1)
		_Emission("Emission", Float) = 2
		[Toggle]_UseFresnel("Use Fresnel?", Float) = 1
		[Toggle]_SeparateFresnel("SeparateFresnel", Float) = 0
		_SeparateEmission("Separate Emission", Float) = 2
		_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		_Fresnel("Fresnel", Float) = 1 
		_FresnelEmission("Fresnel Emission", Float) = 1
		[Toggle]_UseCustomData("Use Custom Data?", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
    
    }
    SubShader{
        Pass {
            
            // 只有定义了正确的LightMode才能得到一些Unity的内置光照变量
            Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
           Cull Off
		    Blend SrcAlpha OneMinusSrcAlpha	
           
            CGPROGRAM
	        #pragma fragmentoption ARB_precision_hint_fastest
            // 包含unity的内置的文件，才可以使用Unity内置的一些变量
            #include "Lighting.cginc" // 取得第一个直射光的颜色_LightColor0 第一个直射光的位置_WorldSpaceLightPos0（即方向）
            #pragma vertex vert
            #pragma fragment frag



			uniform float _SeparateFresnel;
			uniform float _UseFresnel;
			uniform float4 _FrontFacesColor;
			uniform float _Fresnel;
			uniform float _FresnelEmission;
			uniform float4 _FresnelColor;
			uniform float4 _BackFacesColor;
			uniform float _Emission;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _SpeedMainTexUVNoiseZW;
			uniform float _SeparateEmission;
			uniform sampler2D _Mask;
			uniform float4 _Mask_ST;
			uniform sampler2D _Noise;
			uniform float4 _Noise_ST;
			uniform float _UseCustomData;
			uniform float _Cutoff = 0.5;

            struct a2v
            {
                float4 vertex : POSITION;    // 告诉Unity把模型空间下的顶点坐标填充给vertex属性
                float3 normal : NORMAL;        // 告诉Unity把模型空间下的法线方向填充给normal属性
                float4 texcoord : TEXCOORD0;// 告诉Unity把第一套纹理坐标填充给texcoord属性
                fixed4 color : COLOR;//顶点颜色
            };

            struct v2f
            {
                float4 position : SV_POSITION; // 声明用来存储顶点在裁剪空间下的坐标
                float3 worldPos : COLOR;  // 用于存储世界空间下的法线方向
               
                float2 uv_texcoord : TEXCOORD0;
                float4 vertexColor : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float4 uv_tex4coord: TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                float3 worldNormal : TEXCOORD5;
            };

            // 计算顶点坐标从模型坐标系转换到裁剪面坐标系
            v2f vert(a2v v)
            {
                v2f f;
                f.position = UnityObjectToClipPos(v.vertex); // UNITY_MATRIX_MVP是内置矩阵。该步骤用来把一个坐标从模型空间转换到剪裁空间
                
                // 法线方向。把法线方向从模型空间转换到世界空间
                f.worldPos = (mul(v.vertex, (float3x3)unity_WorldToObject)); // 反过来相乘就是从模型到世界，否则是从世界到模型

                f.screenPos = ComputeScreenPos(v.vertex);
                f.vertexColor = v.color;
                f.uv_texcoord = v.texcoord.xy;
                f.uv_tex4coord = v.texcoord;
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));//计算出顶点到相机的向量
                f.viewDir = viewDir;
                f.worldNormal = (mul(v.normal, (float3x3)unity_WorldToObject)); 
                return f;
            }

            // 计算每个像素点的颜色值
            fixed4 frag(v2f i) : SV_Target 
            {

                float3 ase_worldPos = i.worldPos;
			    float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			    float3 ase_worldNormal = i.worldNormal;
			    float fresnelNdotV95 = dot( ase_worldNormal, ase_worldViewDir );
			    float fresnelNode95 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV95, _Fresnel ) );
			    float dotResult87 = dot( ase_worldNormal , i.viewDir );
			    float4 lerpResult91 = lerp( lerp(_FrontFacesColor,( ( _FrontFacesColor * ( 1.0 - fresnelNode95 ) ) + ( _FresnelEmission * _FresnelColor * fresnelNode95 ) ),_UseFresnel) , _BackFacesColor , (1.0 + (sign( dotResult87 ) - -1.0) * (0.0 - 1.0) / (1.0 - -1.0)));
			    float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			    float2 appendResult21 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			    float4 tex2DNode105 = tex2D( _MainTex, ( uv0_MainTex + ( appendResult21 * _Time.y ) ) );
			
			    float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			    float4 uv0_Noise = i.uv_tex4coord;
			    uv0_Noise.xy = i.uv_tex4coord.xy * _Noise_ST.xy + _Noise_ST.zw;
			    float2 appendResult22 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
			    clip( ( tex2D( _Mask, uv_Mask ) * tex2D( _Noise, ( (uv0_Noise).xy + ( _Time.y * appendResult22 ) + uv0_Noise.w ) ) * lerp(1.0,uv0_Noise.z,_UseCustomData) ).r - _Cutoff );

                float3 col = lerp(( lerpResult91 * _Emission * i.vertexColor * i.vertexColor.a * tex2DNode105 ),( ( lerpResult91 + ( _FresnelColor * tex2DNode105 * _SeparateEmission ) ) * _Emission * i.vertexColor * i.vertexColor.a ),_SeparateFresnel).rgb;

                return fixed4(col, 1); // tempColor是float3已经包含了三个数值
            }

            ENDCG
        }
        
    }
    FallBack "Diffuse"
}