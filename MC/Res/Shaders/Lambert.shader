Shader "Unlit/Lambert"
{
    Properties
    {
        _BaseMap    ("Base Texture", 2D)             = "white" {}
        _BaseColor  ("Base Color",   Color)          = (0, 0, 0, 0)
        _Cutoff     ("Cutoff",       Range(0, 1))    = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float  _Cutoff;
        CBUFFER_END

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv     : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv           : TEXCOORD0;
            float4 positionCS   : SV_POSITION;
            float3 positionWS   : TEXCOORD1;
            float3 positionVS   : TEXCOORD2;
            float4 positionNDC  : TEXCOORD3;
            float3 normalWS     : TEXCOORD4;
        };

        v2f Vert (appdata v)
        {
            v2f o;

            VertexPositionInputs vInputs = GetVertexPositionInputs(v.vertex);
            VertexNormalInputs   nInputs = GetVertexNormalInputs(v.normal);

            o.positionCS  = vInputs.positionCS;
            o.positionWS  = vInputs.positionWS;
            o.positionVS  = vInputs.positionVS;
            o.positionNDC = vInputs.positionNDC;
            o.normalWS    = nInputs.normalWS;

            o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

            return o;
        }

        float4 Frag (v2f i) : SV_Target
        {
            real2 screenSpaceCoord = i.positionNDC.xy / i.positionNDC.w;
            real  screenSpaceAO    = SampleAmbientOcclusion(screenSpaceCoord);
            real4 shadowCoord      = TransformWorldToShadowCoord(i.positionWS);

            //  base color
            real4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);

            //  diffuse color
            Light light = GetMainLight(shadowCoord);
            real3 diffuse = LightingLambert(light.color, light.direction, i.normalWS);
            diffuse *= light.shadowAttenuation * baseColor;

            //  ambient color
            real3 ambient = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
            ambient *= screenSpaceAO;

            return float4(diffuse + ambient, 1);
        }
        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            // Cull[_Cull]
            Cull Back


            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            ZWrite On
            // Cull[_Cull]
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
}
