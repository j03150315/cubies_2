Shader "Holo" 
{
	Properties
	{
		_Intensity ("Overall Intensity", Range(0,2)) = 1
		_Fader("Dynamic Fading ", Range(0,1)) = 1

		_CenterColor ("Center Tint", Color) = (0.5,0.5,0.5,1)
		_CenterIntensity("Center Intensity", Range(0,2)) = 1
		_CenterFade ("Center Fade", Range(0.5,3.5)) = 0

		_EdgeColor ("Edge Tint", Color) = (0.5,0.5,0.5,1)	
		_EdgeIntensity("Edge Intensity", Range(0,1)) = 0
		_EdgeFalloff ("Edge falloff", Range(0,1)) = 1	

		// Recharge
		_BarHeight("Bar Height", Range(0.0, 1.0)) = 0.8
		_Tightness("Tightness", Range(0.01, 100.0)) = 2.0
		_Recharge("Recharge", Range(0.0, 1.0)) = 0.8
		_EmptyColor("Empty Tint", Color) = (0.5,0.5,0.5,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 100
		
		// Z-Prepass
		Pass
		{
			zwrite on
			ColorMask 0
			cull back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc"
			struct appdata 
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				return 0;
			}
			ENDCG
		}

		Pass
		{
			zwrite off
			blend one one
			cull back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc"
			float4 _CenterColor, _EdgeColor, _EmptyColor;
			float _Intensity, _Fader;
			float _CenterIntensity, _CenterFade;
			float _EdgeIntensity, _EdgeFalloff;

			float _BarHeight;
			float _Recharge;
			float _Tightness;

			struct appdata 
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
				float2 uv_main : TEXCOORD0;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float gradient : TEXCOORD0;
			};
			v2f vert (appdata v)
			{
				v2f o;
				float4 objpos = v.vertex;
				
				o.gradient = clamp(objpos.y / _BarHeight, 0, 1);
				o.vertex = UnityObjectToClipPos(objpos);

				float3 normal = normalize(v.normal);
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				float3 NoV = dot(viewDir, normal);
                float fresnel = 1 - abs(NoV);
				float center = max(0, lerp(1, NoV, _CenterFade));
				float edge = smoothstep(1 - _EdgeFalloff, 1.0, fresnel);

				o.color.rgb = center * _CenterIntensity * _CenterColor + edge * _EdgeIntensity * _EdgeColor;
				o.color.a = 1;

				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				float frac = clamp( (i.gradient - _Recharge) * _Tightness, 0.0, 1.0);
				float4 col = i.color + _EmptyColor * frac;
				return col * _Fader;
			}
			ENDCG
		}
	}
}
