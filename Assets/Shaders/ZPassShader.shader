Shader "ZTest Shaders/Cube"
{
    Properties
    {
        ObjectColor("Object Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent+10"
        }

        Pass
        {
            ZWrite On
            ZTest Greater

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            uniform fixed4 ObjectColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = ObjectColor;
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}