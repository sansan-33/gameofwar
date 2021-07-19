Shader "AppsTools/FastShader/Effect/Blend_Normals"
{
	 Properties{

        _MainTex("MainTex", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_Emission("Emission", Float) = 2
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		[MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
		_Depthpower("Depth power", Float) = 1
		[MaterialToggle] _Usecenterglow("Use center glow?", Float) = 0
		_Mask("Mask", 2D) = "white" {}
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_NormalMap("Normal Map", 2D) = "white" {}
		_NormalScale("Normal Scale", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
    
    }
    SubShader{
        Pass {
            
            // 只有定义了正确的LightMode才能得到一些Unity的内置光照变量
            Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			ZTest LEqual
            CGPROGRAM
	        #pragma fragmentoption ARB_precision_hint_fastest
            // 包含unity的内置的文件，才可以使用Unity内置的一些变量
            #include "Lighting.cginc" // 取得第一个直射光的颜色_LightColor0 第一个直射光的位置_WorldSpaceLightPos0（即方向）
            #pragma vertex vert
            #pragma fragment frag



            uniform float _NormalScale;
		    uniform sampler2D _NormalMap;
		    uniform float4 _SpeedMainTexUVNoiseZW;
		    uniform float4 _NormalMap_ST;
		    uniform sampler2D _MainTex;
		    uniform float4 _MainTex_ST;
		    uniform sampler2D _Noise;
		    uniform float4 _Noise_ST;
		    uniform float4 _Color;
		    uniform sampler2D _Mask;
		    uniform float4 _Mask_ST;
		    uniform float _Emission;
		    uniform sampler2D _CameraDepthTexture;
		    uniform float _Depthpower;
		    uniform fixed _Usedepth;
		    uniform float _Opacity;
		    uniform fixed _Usecenterglow;

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
                float3 worldNomalDir : COLOR;  // 用于存储世界空间下的法线方向
               
                float4 uv_texcoord : TEXCOORD0;
                float4 vertexColor : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            // 计算顶点坐标从模型坐标系转换到裁剪面坐标系
            v2f vert(a2v v)
            {
                v2f f;
                f.position = UnityObjectToClipPos(v.vertex); // UNITY_MATRIX_MVP是内置矩阵。该步骤用来把一个坐标从模型空间转换到剪裁空间
                
                // 法线方向。把法线方向从模型空间转换到世界空间
                f.worldNomalDir = (mul(v.normal, (float3x3)unity_WorldToObject)); // 反过来相乘就是从模型到世界，否则是从世界到模型

                f.screenPos = ComputeScreenPos(v.vertex);
                f.vertexColor = v.color;
                f.uv_texcoord = v.texcoord;

                return f;
            }

            // 计算每个像素点的颜色值
            fixed4 frag(v2f i) : SV_Target 
            {

                float2 appendResult21 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			    float2 temp_output_24_0 = ( appendResult21 * _Time.y );
			    float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
		
			    float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			    float4 tex2DNode13 = tex2D( _MainTex, ( temp_output_24_0 + uv_MainTex ) );
			    float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			    float2 appendResult22 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
			    float4 tex2DNode14 = tex2D( _Noise, ( uv_Noise + ( _Time.y * appendResult22 ) ) );
			    float3 temp_output_30_0 = ( (tex2DNode13).rgb * (tex2DNode14).rgb * (_Color).rgb * (i.vertexColor).rgb );
			    float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			    float3 temp_output_58_0 = (tex2D( _Mask, uv_Mask )).rgb;
			    float3 temp_cast_0 = ((1.0 + (0.0 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0))).xxx;
			    float3 clampResult38 = clamp( ( temp_output_58_0 - temp_cast_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			    float3 clampResult40 = clamp( ( temp_output_58_0 * clampResult38 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			    float3 staticSwitch46 = lerp(temp_output_30_0, ( temp_output_30_0 * clampResult40 ), _Usecenterglow);
		
			    float temp_output_60_0 = ( tex2DNode13.a * tex2DNode14.a * _Color.a * i.vertexColor.a );
			    float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			    float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			    ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			    float screenDepth49 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			    float distanceDepth49 = abs( ( screenDepth49 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depthpower ) );
			    float clampResult53 = clamp( distanceDepth49 , 0.0 , 1.0 );		
			    float4 staticSwitch47 = temp_output_60_0;
                
                float3 Normal = UnpackScaleNormal( tex2D( _NormalMap, ( temp_output_24_0 + uv_NormalMap ) ), _NormalScale );
			    float3 Albedo = staticSwitch46;
			    float3 Emission = ( staticSwitch46 * _Emission );
			    float4 Alpha = ( staticSwitch47 * _Opacity );

 

                return fixed4(Albedo + Emission, Alpha.w); // tempColor是float3已经包含了三个数值
            }

            ENDCG
        }
        
    }
    FallBack "Diffuse"
}