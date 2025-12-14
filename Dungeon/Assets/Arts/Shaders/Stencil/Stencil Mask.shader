Shader "Custom/URP/Stencil Mask"
{
    Properties
    {
       _BaseColor ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
       Tags 
       { 
           "RenderType"="Opaque" 
           "RenderPipeline" = "UniversalPipeline" 
           "Queue" = "Geometry-100" 
       }

       Pass
       {
           Name "MaskPass"

           ColorMask 0 
           ZWrite Off  
          
           Stencil 
           {
              Ref 1           
              Comp Always     
              Pass Replace   
           }

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

           #pragma vertex vert
           #pragma fragment frag
           
           struct Attributes
           {
              float4 positionOS : POSITION;
           };

           struct Varyings
           {
              float4 positionCS : SV_POSITION;
           };

           float4 _BaseColor; 
           
           Varyings vert (Attributes input)
           {
              Varyings output;
              output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
              return output;
           }

           half4 frag (Varyings input) : SV_Target
           {
              return half4(0, 0, 0, 0); 
           }
           ENDHLSL
       }
    }
}