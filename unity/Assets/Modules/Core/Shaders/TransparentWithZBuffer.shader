Shader "Core/TransparentWithZBuffer"
{
	Properties
	{
		_color("Color", Color) = (0, 0, 0, 0)
	}


	SubShader
	{
		Tags{ "Queue" = "Transparent-10" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Cull Back
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
		ColorMask RGB

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		uniform float4 _color;

		struct Input
		{
			float4 vertex : POSITION;
			float4 color: COLOR;
		};

		struct v2f {
			float4 position : SV_POSITION;
			float4 color : COLOR;
		};

		v2f vert(Input input) {
			v2f output;
			output.color = _color;
			output.position = UnityObjectToClipPos(input.vertex);
			return output;
		}

		fixed4 frag(v2f input) : SV_Target
		{
			return _color;
		}


		ENDCG
		}
	}
}

