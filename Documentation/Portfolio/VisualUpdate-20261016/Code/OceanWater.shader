Shader "BreathIsland/OceanWater"
{
    Properties
    {
        _ShallowColor("Shallow water",Color)=(0.12,0.64,0.58,1)
        _DeepColor("Deep water",Color)=(0.015,0.20,0.30,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma editor_sync_compilation
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _ShallowColor, _DeepColor;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float fog:TEXCOORD1; };
            float3 Waves(float2 p)
            {
                float3 phase=float3(dot(p,float2(.52,.31))+_Time.y*1.1,
                    dot(p,float2(-.73,.57))+_Time.y*1.65,dot(p,float2(1.7,1.1))-_Time.y*2.4);
                float3 c=cos(phase)*float3(.11,.055,.025);
                return float3(dot(sin(phase),float3(.11,.055,.025)),
                    dot(c,float3(.52,-.73,1.7)),dot(c,float3(.31,.57,1.1)));
            }
            Varyings Vert(Attributes input)
            {
                Varyings o;o.world=TransformObjectToWorld(input.positionOS.xyz);
                o.world.y=-.7+Waves(o.world.xz).x;
                o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 uv=GetNormalizedScreenSpaceUV(input.positionCS);
                float sceneDepth=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);
                float surfaceDepth=-TransformWorldToView(input.world).z;
                float depth=max(0,sceneDepth-surfaceDepth);
                float3 wave=Waves(input.world.xz);
                float2 fine=float2(sin(input.world.x*6.1+input.world.z*3.8+_Time.y*2+sin(input.world.z*1.7)),
                    cos(input.world.z*7.3-input.world.x*2.9-_Time.y*1.8+sin(input.world.x*1.4)))*.045;
                float detailFade=1-smoothstep(15,70,distance(input.world,_WorldSpaceCameraPos));
                fine*=detailFade;
                wave.yz*=.25+.75*detailFade;
                half3 normal=normalize(float3(-wave.y+fine.x,1,-wave.z+fine.y));
                half3 view=GetWorldSpaceNormalizeViewDir(input.world);
                float2 distorted=uv+normal.xz*.008*saturate(depth);
                // Never refract foreground silhouettes into the water.
                if(LinearEyeDepth(SampleSceneDepth(distorted),_ZBufferParams)<surfaceDepth)distorted=uv;
                half3 transmitted=SampleSceneColor(distorted);
                half3 tint=lerp(_ShallowColor.rgb,_DeepColor.rgb,1-exp(-depth*.32));
                half absorption=1-exp(-depth*.20);
                half3 color=lerp(transmitted,tint,.18+absorption*.72);
                half fresnel=.035+.7*pow(1-saturate(dot(normal,view)),4);
                half3 reflection=GlossyEnvironmentReflection(reflect(-view,normal),.12h,1.h);
                color=lerp(color,reflection,fresnel);
                Light sun=GetMainLight();
                half spec=pow(saturate(dot(normal,normalize(view+sun.direction))),96);
                color+=sun.color*spec*.9;
                float foamNoise=.6+.4*sin(input.world.x*8+sin(input.world.z*5)+_Time.y*2);
                float foam=(1-smoothstep(.02,.27,depth))*foamNoise;
                color=lerp(color,half3(.78,.94,.88),foam*.85);
                return half4(MixFog(color,input.fog),1);
            }
            ENDHLSL
        }
    }
}
