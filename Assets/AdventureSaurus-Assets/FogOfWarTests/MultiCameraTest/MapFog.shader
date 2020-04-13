Shader "Hidden/MapFog"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_SecondaryTex("Secondary Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags
		{
			"Queue" = "Transparent+1"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _SecondaryTex;

			fixed4 frag(v2f i) : SV_Target
			{
				// Get the colors of the rooms layout
				fixed4 colRooms = tex2D(_SecondaryTex, i.uv);
				// Get the colros of the bleed lights layout
				fixed4 colBleed = tex2D(_MainTex, i.uv);

				// End color (assume transparent)
				fixed4 col = fixed4(0.0, 0.3, 0.0, 1.0);

				// Red represents what should be visible on the map
				float a = 1 - max(colRooms.r, colBleed.r);
				if (a > 0.0) {
					// Make it opaque
					col = fixed4(0.0, 0.3, 0.0, 1 - a);
				}

				return col;
			}
			ENDCG
		}
	}
}
