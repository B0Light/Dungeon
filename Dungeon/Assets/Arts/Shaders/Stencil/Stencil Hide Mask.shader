Shader"Custom/URP/Stencil Hide Mask"
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

           ColorMask 0 // 색상 렌더링을 끔 (마스크 오브젝트를 안 보이게 함) 
           ZWrite Off  // 깊이 버퍼 쓰기를 끔 
          
           Stencil // Stencil Buffer에 값을 기록 
           {
              Ref 1   // 참조 값은 1 
              Comp Always     // 항상 테스트 통과 
              Pass Replace   // 테스트 통과 시 Stencil Buffer 값을 Ref(1)로 교체 
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
              // ColorMask 0 때문에 최종 색상은 기록되지 않지만,
              // Stencil 연산을 위해 픽셀을 처리해야 합니다.
              return half4(0, 0, 0, 0); 
           }
           ENDHLSL
       }
    }
}