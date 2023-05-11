Shader "Ultraleap/GenericHandShader_ScaleEdit"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		[HDR]_MainColor("Main Color", Color) = (0,0,0,1)

		[MaterialToggle] _useOutline("Use Outline", Float) = 0
		[HDR]_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0,.2)) = .01
		_ScaleCorrection("Scale Correction", Range(-1,1)) = 1
		[MaterialToggle] _useLighting("Use Lighting", Float) = 0
		_LightIntensity("Light Intensity", Range(0,1)) = 1

		[MaterialToggle] _useFresnel("Use Fresnel", Float) = 0
		[HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,0)
		_FresnelPower("Fresnel Power", Range(0,1)) = 1
		[Space(20)]
		_ThumbColor("Thumb Finger Color", Color) = (1,1,1,1)
		_ThumbTipColor("Thumb Fingertip Color", Color) = (1,1,1,1)
		_ThumbFinger("Thumb Finger Heatmap", 2D) = "black" {}
		_ThumbVisibility("Thumb Finger Alpha", Range(0,1)) = 1
		_ThumbTipVisibility("Thumb Fingertip Alpha", Range(0,1)) = 1
		[Space(20)]
		_IndexColor("Index Finger Color", Color) = (1,1,1,1)
		_IndexTipColor("Index Fingertip Color", Color) = (1,1,1,1)
		_IndexFinger("Index Finger Heatmap", 2D) = "black" {}
		_IndexVisibility("Index Finger Alpha", Range(0,1)) = 1
		_IndexTipVisibility("Index Fingertip Alpha", Range(0,1)) = 1
		[Space(20)]
		_MiddleColor("Middle Finger Color", Color) = (1,1,1,1)
		_MiddleTipColor("Middle Fingertip Color", Color) = (1,1,1,1)
		_MiddleFinger("Middle Finger Heatmap", 2D) = "black" {}
		_MiddleVisibility("Middle Finger Alpha", Range(0,1)) = 1
		_MiddleTipVisibility("Middle Fingertip Alpha", Range(0,1)) = 1
		[Space(20)]
		_RingColor("Ring Finger Color", Color) = (1,1,1,1)
		_RingTipColor("Ring Fingertip Color", Color) = (1,1,1,1)
		_RingFinger("Ring Finger Heatmap", 2D) = "black" {}
		_RingVisibility("Ring Finger Alpha", Range(0,1)) = 1
		_RingTipVisibility("Ring Fingertip Alpha", Range(0,1)) = 1
		[Space(20)]
		_PinkyColor("Pinky Finger Color", Color) = (1,1,1,1)
		_PinkyTipColor("Pinky Fingertip Color", Color) = (1,1,1,1)
		_PinkyFinger("Pinky Finger Heatmap", 2D) = "black" {}
		_PinkyVisibility("Pinky Finger Alpha", Range(0,1)) = 1
		_PinkyTipVisibility("Pinky Fingertip Alpha", Range(0,1)) = 1
		[Space(20)]
		_AllHandColor("Whole Hand Color", Color) = (1,1,1,1)
		_AllHandAlpha("Whole Hand Highlight Alpha", Range(0,1)) = 0 

	}

		CGINCLUDE #include "UnityCG.cginc"

		float4 _MainColor;
		float _Outline;
		float4 _OutlineColor;
		sampler2D _MainTex;
		sampler2D _ThumbFinger, _IndexFinger, _MiddleFinger, _RingFinger, _PinkyFinger;
		fixed _ThumbVisibility, _IndexVisibility, _MiddleVisibility, _RingVisibility, _PinkyVisibility;
		fixed _ThumbTipVisibility, _IndexTipVisibility, _MiddleTipVisibility, _RingTipVisibility, _PinkyTipVisibility; 
		fixed4 _ThumbColor, _IndexColor, _MiddleColor, _RingColor, _PinkyColor;
		fixed4 _ThumbTipColor, _IndexTipColor, _MiddleTipColor, _RingTipColor, _PinkyTipColor;
		fixed4 _AllHandColor;
		fixed _AllHandAlpha;
		float4 _FresnelColor;
		float _FresnelPower;
		float _LightIntensity;
		float _useLighting;
		float _useFresnel;
		float _useOutline;
		float _ScaleCorrection;

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 pos : POSITION;
			float3 worldNormal : NORMAL;
			float3 viewDir : TEXCOORD1;
			fixed4 diff : COLOR0;

			UNITY_VERTEX_OUTPUT_STEREO
		};

		ENDCG

			SubShader
		{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "ForwardBase"}
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				Cull Back
				Blend Zero One
			}
			Pass
			{
				Cull Front

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata_base v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.pos = UnityObjectToClipPos(v.vertex);
					float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
					float2 offset = TransformViewToProjection(norm.xy);
					o.uv = v.texcoord;
					o.pos.xy = _useOutline ? o.pos.xy + offset * _Outline * _ScaleCorrection : o.pos.xy;
					return o;
				}

				half4 frag(v2f i) :COLOR
				{
					fixed4 col = tex2D(_MainTex, i.uv) * _OutlineColor;
					return col;
				}
				ENDCG
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
				#include "UnityCG.cginc" // for UnityObjectToWorldNormal
				#include "UnityLightingCommon.cginc" // for _LightColor0
				#include "AutoLight.cginc"

				v2f vert(appdata_base v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord;
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.diff = nl * _LightColor0;
					o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

					o.worldNormal = worldNormal;
					o.viewDir = WorldSpaceViewDir(v.vertex);
					return o;
				}

				//This is the fresnel function that shader graph uses
				float Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power)
				{
					return pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
				}

				half4 frag(v2f i) :COLOR
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					col *= _MainColor;

					if (_useLighting)
					col.rgb *= (i.diff * _LightIntensity);
					if (_useFresnel)
					col.rgb *= _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a;
					col.rgb += tex2D(_ThumbFinger, i.uv).r * _ThumbVisibility * _ThumbColor;
					col.rgb += tex2D(_IndexFinger, i.uv).r * _IndexVisibility * _IndexColor;
					col.rgb += tex2D(_MiddleFinger, i.uv).r * _MiddleVisibility * _MiddleColor;
					col.rgb += tex2D(_RingFinger, i.uv).r * _RingVisibility * _RingColor;
					col.rgb += tex2D(_PinkyFinger, i.uv).r * _PinkyVisibility * _PinkyColor;
					col.rgb = lerp(col.rgb, _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a + _ThumbTipColor, tex2D(_ThumbFinger, i.uv).g * _ThumbTipVisibility);
					col.rgb = lerp(col.rgb, _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a + _IndexTipColor, tex2D(_IndexFinger, i.uv).g * _IndexTipVisibility);
					col.rgb = lerp(col.rgb, _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a + _MiddleTipColor, tex2D(_MiddleFinger, i.uv).g * _MiddleTipVisibility);
					col.rgb = lerp(col.rgb, _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a + _RingTipColor, tex2D(_RingFinger, i.uv).g * _RingTipVisibility);
					col.rgb = lerp(col.rgb, _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a + _PinkyTipColor, tex2D(_PinkyFinger, i.uv).g * _PinkyTipVisibility);
					col.rgb = lerp(col.rgb, _FresnelColor * Unity_FresnelEffect_float(i.worldNormal, i.viewDir, _FresnelPower) * _FresnelColor.a + _AllHandColor, _AllHandAlpha);
					return col;
				}
				ENDCG
			}
					//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		}

			Fallback "Diffuse"
}