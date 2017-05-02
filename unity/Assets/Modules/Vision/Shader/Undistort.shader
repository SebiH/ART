// based on W. Steptoe from http://willsteptoe.com/post/67401705548/ar-rift-aligning-tracking-and-video-spaces-part
Shader "Vision/Undistort"
{
	Properties
	{
		_MainTex("Camera", 2D) = "white" {}
		_F1("Focallength 1", float) = 0
		_F2("Focallength 2", float) = 0
		_OX("Optical centre X", float) = 0
		_OY("Optical centre Y", float) = 0
		_K1("Distortion Coefficient 1", float) = 0
		_K2("Distortion Coefficient 2", float) = 0
		_K3("Distortion Coefficient 3", float) = 0
		_K4("Distortion Coefficient 4", float) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Background+1" "RenderType" = "Background" }

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 4.0
			#include "UnityCG.cginc"

			// texture from camera
			sampler2D _MainTex;

			 uniform float _F1;
			 uniform float _F2;
			 uniform float _OX;
			 uniform float _OY;

			 // distortion coefficients
			 uniform float _K1;
			 uniform float _K2;
			 uniform float _K3;
			 uniform float _K4;

			struct Input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(Input input)
			{
				v2f output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.vertex = input.vertex;
				output.uv = input.uv;
				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				float2 ff = {_F1,  _F2};
				float2 xy = (input.uv - float2(_OX, _OY) / _ScreenParams.xy) / ff;
				float r = sqrt(dot(xy, xy));
				float r2 = r * r;
				float r4 = r2 * r2;
				float coeff = (_K1 * r2 + _K2 * r4);

				float dx = _K3 * 2.0 * xy.x * xy.y + _K4 * (r2 + 2.0 * xy.x * xy.x);
				float dy = _K3 * (r2 + 2.0 * xy.y * xy.y) + _K4 * 2.0 * xy.x * xy.y;

				xy = (((xy + xy * coeff.xx + float2(dx, dy)) * ff) + float2(_OX, _OY)) / _ScreenParams.xy;
				return tex2D(_MainTex, xy);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
