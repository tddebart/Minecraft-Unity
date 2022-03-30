
Shader "Voxel"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue" = "AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		LOD 100
		Lighting Off
		
		Pass {
			CGPROGRAM
			#pragma vertex vertFunction
			#pragma fragment fragFunction
			#pragma target 2.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float2 light : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				// This is the light value on this vertex
				// With x being the sky light and y being the block light
				float2 light : TEXCOORD1;
			};

			sampler2D _MainTex;
			float GlobalLightLevel;
			float minGlobalLightLevel;
			float maxGlobalLightLevel;
			float4 lightColors[256];
			

			v2f vertFunction (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				o.light = v.light;

				return o;
			}
			// float inverseLerp (int a,int b, float t) {
			// 	return (t - a) / (b - a);
			// }

			fixed4 fragFunction (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				clip(col.a -1);

				
				float4 lightColor = lightColors[(round(i.light.x) * 16 + round(i.light.y))];
				col *= lightColor;
				// col *= 0.8f;
				
				return col;
			}


			ENDCG
		}
	}
}