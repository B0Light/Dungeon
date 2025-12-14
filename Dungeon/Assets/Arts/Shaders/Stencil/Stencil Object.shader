Shader "Custom/URP/Stencil Object"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Toggle(_RECEIVE_SHADOWS_OFF)] _ReceiveShadows("Receive Shadows", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 200

        Pass
        {
            Name "Unlit"
            
            Stencil
            {
                Ref 1
                Comp Equal 
                Pass Keep  
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
            
            #if defined(_RECEIVE_SHADOWS_OFF)
                #define _RECEIVE_SHADOWS 0
            #else
                #define _RECEIVE_SHADOWS 1
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float4 _BaseColor;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                Light mainLight = GetMainLight();
                half3 normal = normalize(input.normalWS);
                half diffuse = saturate(dot(normal, mainLight.direction));
                
                #if _RECEIVE_SHADOWS
                float shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(input.positionWS));
                #else
                float shadow = 1.0;
                #endif

                half3 directLighting = mainLight.color * diffuse * shadow;

                half3 ambientLight = SampleSH(input.normalWS);

                half3 finalColor = color.rgb * (directLighting + ambientLight);

                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
    }
}