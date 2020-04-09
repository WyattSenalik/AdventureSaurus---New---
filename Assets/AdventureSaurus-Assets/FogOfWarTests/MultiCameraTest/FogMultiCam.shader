Shader "Hidden/FogMultiCam"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _SecondaryTex;

            fixed4 frag (v2f i) : SV_Target
            {
				// Get the colors of the rooms layout
                fixed4 colRooms = tex2D(_SecondaryTex, i.uv);
				// Get the colros of the bleed lights layout
				fixed4 colBleed = tex2D(_MainTex, i.uv);
				
				// Choose the brighter red of the layouts, and that will determine the "light"
				// Where more light is an alpha value (0 - lit, 1 - dark)
				float a = 1 - max(colRooms.r, colBleed.r);

				// Apply the alpha
				fixed4 col = fixed4(0.0, 0.0, 0.0, a);
			
                return col;
            }
            ENDCG
        }
    }
}
