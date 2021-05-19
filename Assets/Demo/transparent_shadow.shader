// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/trasparent_recieve_shadow"
 {
     Properties
     {
     }
     
     CGINCLUDE
     #include "UnityCG.cginc"
     #include "AutoLight.cginc"
     #include "Lighting.cginc"
     ENDCG
 
  SubShader
  {
      LOD 200
      Tags { "RenderType"="transparent" }
	  Blend SrcAlpha OneMinusSrcAlpha
      Pass { 
             Lighting On
             Tags {"LightMode" = "ForwardBase"}
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_fwdbase
 
             struct vertexInput
             {
                 half4 vertex : POSITION;
                 half3 normal : NORMAL;
             };
             
             struct vertexOutput
             {
                 half4 pos                :    SV_POSITION;
                 fixed4 lightDirection    :    TEXCOORD1;
                 fixed3 viewDirection    :    TEXCOORD2;
                 fixed3 normalWorld        :    TEXCOORD3;
                 LIGHTING_COORDS(4,6)
             };
 
              vertexOutput vert (vertexInput v)
             {
                 vertexOutput o;
                 
                 half4 posWorld = mul( unity_ObjectToWorld, v.vertex );
                 o.normalWorld = normalize( mul(half4(v.normal, 0.0), unity_WorldToObject).xyz );
                 o.pos = UnityObjectToClipPos(v.vertex);
                 o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);
             
                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             
             half4 frag (vertexOutput i) : COLOR
             {
                 fixed NdotL = dot(i.normalWorld, i.lightDirection);
                 half atten = LIGHT_ATTENUATION(i);
                 fixed3 diffuseReflection = atten;
                 fixed3 finalColor = diffuseReflection;
                 float a = max(0.8 - finalColor.x, 0);
                 return float4(0,0,0, a);
             }
             
              ENDCG
          }
      }
      FallBack "Diffuse"
  }