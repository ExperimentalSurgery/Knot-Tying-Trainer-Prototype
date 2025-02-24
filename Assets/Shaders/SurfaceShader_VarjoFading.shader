Shader "KnotbAR/SurfaceShader_VarjoFading"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _MetallicGlossMap ("Metallic(R) Smoothness (A)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Alpha ("Alpha", Range(0,1)) = 1
    }
    SubShader
    {

        Pass
        {
            ZWrite On
            ColorMask 0
        }

        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
			#include "UnityStandardUtils.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed3 normalDir : TEXCOORD2;
                float4 worldPos : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _MetallicGlossMap;
            float4 _MainTex_ST;
            fixed4 _Color; 
            fixed _Metallic, _Smoothness;
            half _Alpha;
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 normal = normalize(i.normalDir);
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

				float3 lightColor = _LightColor0.rgb;
				float3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;

                half smap = tex2D(_MetallicGlossMap, i.uv).a * _Smoothness;
                fixed metal = tex2D(_MetallicGlossMap,i.uv).r * _Metallic;

				float3 specularTint;
				float oneMinusReflectivity;
				albedo = DiffuseAndSpecularFromMetallic(
					albedo, metal, specularTint, oneMinusReflectivity
				);
				UnityLight dirlight;
				dirlight.color = lightColor;
				dirlight.dir = lightDir;
				dirlight.ndotl = max(0.0, dot(normal, lightDir));

				UnityIndirect indLight;
                indLight.diffuse = 0.5;
                indLight.specular = 0;
                indLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));

                float3 reflectionDir = reflect(-viewDir, normal);
                float4 skybox = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDir);

                Unity_GlossyEnvironmentData envData;
                envData.roughness = 1 - smap;
                envData.reflUVW = reflectionDir;
                indLight.specular = Unity_GlossyEnvironment(
                    UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);





                float4 final = UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, smap,
					normal, viewDir,
					dirlight, indLight);

                final.a = _Alpha;
                UNITY_APPLY_FOG(i.fogCoord, final);
                return final;
            }
            ENDCG
        }      
    }
}
