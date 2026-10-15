Shader "BreathIsland/Ground"
{
 Properties { _BaseColor("Ground",Color)=(.27,.43,.19,1) _Accent("Earth",Color)=(.38,.32,.19,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma editor_sync_compilation
 #pragma multi_compile_fog
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor,_Accent;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;};
 struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;};
 float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float Noise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(Hash(a),Hash(a+float2(1,0)),b.x),lerp(Hash(a+float2(0,1)),Hash(a+1),b.x),b.y);}
 V Vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 Frag(V i):SV_Target
 {
 float patch=Noise(i.w.xz*.42)*.65+Noise(i.w.xz*1.8)*.35;
 float grain=Noise(i.w.xz*32);
 float distanceFade=1-smoothstep(8,45,distance(i.w,_WorldSpaceCameraPos));
 half3 color=lerp(_BaseColor.rgb,_Accent.rgb,smoothstep(.36,.8,patch)*.65);
 color*=.87+.22*Noise(i.w.xz*3)+distanceFade*(grain-.5)*.13;
 Light sun=GetMainLight(TransformWorldToShadowCoord(i.w));
 half3 illumination=SampleSH(normalize(i.n))+.7*sun.color*saturate(dot(normalize(i.n),sun.direction))*sun.shadowAttenuation;
 return half4(MixFog(color*illumination,i.fog),1);
 }
 ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 }
}
