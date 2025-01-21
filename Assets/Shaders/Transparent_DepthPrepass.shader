Shader "Transparent/Standard Lit Depth Prepass"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _MetallicSmoothnessMap("Metallic (R) Smoothness (A)", 2D) = "grey" {}
        _Alpha("Alpha", Float) = 1.0
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            // Depth prepass
            Pass
            {
                ZWrite On
                ColorMask 0
            }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            //BlendOp Add
            
            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows 

            #pragma target 3.0

            struct Input
            {
                float2 uv_MainTex;
                float3 worldNormal;
            };
                    
            fixed4 _Color;
            float _Alpha;

            sampler2D _MainTex;
            sampler2D _NormalMap;
            sampler2D _MetallicSmoothnessMap;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)


            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = _Alpha;
                o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
                o.Metallic = tex2D(_MetallicSmoothnessMap, IN.uv_MainTex).r;
                o.Smoothness = tex2D(_MetallicSmoothnessMap, IN.uv_MainTex).a;
            }
            ENDCG
        }
            Fallback "Diffuse"
}
