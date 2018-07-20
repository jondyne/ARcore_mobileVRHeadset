Shader "Yaturu/ARCoreBackground"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		[HideInInspector] _FOV ("FOV", Range(1, 2)) = 1.6
		[HideInInspector] _Disparity ("Disparity", Range(0, 0.3)) = 0.0
		[HideInInspector] _Alpha ("Alpha", Range(0, 2.0)) = 0.5
	}

	// For GLES3
	SubShader
	{
		Pass
		{
			ZWrite Off

			GLSLPROGRAM

#pragma only_renderers gles3

#ifdef SHADER_API_GLES3
#extension GL_OES_EGL_image_external_essl3 : require
#endif

			uniform mat4 _UnityDisplayTransform;

#ifdef VERTEX
			varying vec2 textureCoord;

			void main() {
#ifdef SHADER_API_GLES3
				float flippedV = 1.0 - gl_MultiTexCoord0.y;
				textureCoord.x = _UnityDisplayTransform[0].x * gl_MultiTexCoord0.x + _UnityDisplayTransform[1].x * flippedV + _UnityDisplayTransform[2].x;
				textureCoord.y = _UnityDisplayTransform[0].y * gl_MultiTexCoord0.x + _UnityDisplayTransform[1].y * flippedV + _UnityDisplayTransform[2].y;
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
#endif
			}
#endif

#ifdef FRAGMENT
			varying mediump vec2 textureCoord;
			uniform samplerExternalOES _MainTex;

			uniform mediump float _FOV;
			uniform mediump float _Alpha;
			uniform mediump float _Disparity;

			void main() {
#ifdef SHADER_API_GLES3
				mediump vec2 uv1, uv2, uv3;
				mediump float t1, t2;
				mediump float offset;

				mediump vec2 inputUV = textureCoord;
				//inputUV.y = 1.0 - inputUV.y;

				// uv1 is the remap of left and right screen to a full screen
				uv1 = inputUV - 0.5;
				uv1.x = uv1.x * 2.0 - 0.5 + step(inputUV.x, 0.5);

				t1 = sqrt(1.0 - uv1.x * uv1.x - uv1.y * uv1.y);
				t2 = 1.0 / (t1 * tan(_FOV * 0.5));

				// uv2 is the remap of side screen with barrel distortion
				uv2 = uv1 * t2 + 0.5;

				// black color for out-of-range pixels
				if (uv2.x >= 1.0 || uv2.y >= 1.0 || uv2.x <= 0.0 || uv2.y <= 0.0) {
					gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
				} else {
					offset = 0.5 - _Alpha * 0.5 + _Disparity * 0.5 - _Disparity * step(inputUV.x, 0.5);
					// uv3 is the remap of image texture
					uv3 = uv2;
					uv3.x = uv2.x * _Alpha + offset;

					gl_FragColor = texture2D(_MainTex, uv3);
				}
#endif
			}

#endif
			ENDGLSL
		}
	}

	FallBack Off
}
