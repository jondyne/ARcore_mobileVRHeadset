Shader "Yaturu/Barrel Distortion"
{
	Properties{
		_MainTex("", 2D) = "white" {}

		[HideInInspector] _FOV("FOV", Range(1, 2)) = 1.0
		[HideInInspector] _Alpha("Alpha", Float) = 1.0
	}

	SubShader{
        ZWrite Off

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			// Default Vertex Shader
			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
				return o;
			}

			// Parameters
			sampler2D _MainTex;
			float _FOV;

			// Alpha is the ratio of pixel density: width to height
			float _Alpha;

			uniform float4x4 _UnityDisplayTransform;

			// Fragment Shader: Remap the texture coordinates to combine
			// barrel distortion and disparity video display
			fixed4 frag(v2f i) : COLOR {
				float2 uv1, uv2, uv3;
				float t1, t2;
				float offset;

				// uv1 is the remap of left and right screen to a full screen
				uv1 = i.uv - 0.5;
				uv1.x = uv1.x * 2 - 0.5 + step(i.uv.x, 0.5);

				t1 = sqrt(1.0 - uv1.x * uv1.x - uv1.y * uv1.y);
				t2 = 1.0 / (t1 * tan(_FOV * 0.5));

				// uv2 is the remap of side screen with barrel distortion
				uv2 = uv1 * t2 + 0.5;

				// black color for out-of-range pixels
				if (uv2.x >= 1.5 || uv2.y >= 1.0 || uv2.x <= -0.5 || uv2.y <= 0.0) {
					return fixed4(0, 0, 0, 1);
				} else {
					offset = 0.5 - _Alpha * 0.5;
					// uv3 is the remap of image texture
					uv3 = uv2;
					uv3.x = uv2.x * _Alpha + offset;

					return tex2D(_MainTex, uv3);
				}
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
