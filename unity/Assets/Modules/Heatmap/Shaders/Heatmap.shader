Shader "Custom/Heatmap" {
	Properties{
		_Alpha("Alpha", Float) = 0.8

		_StartHeight("StartHeight", Float) = 0
		_EndHeight("EndHeight", Float) = 4

		_StartHue("StartHue", Float) = 0.6666 // 240/360 - blue
		_EndHue("EndHue", Float) = 0 // red
	}
		SubShader{
		Tags{ "Queue" = "Geometry" "RenderType" = "Transparent" }
		Cull Off

		CGPROGRAM
		
//#pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert
#pragma surface surf Standard fullforwardshadows vertex:vert

#pragma target 3.0

		sampler2D _MainTex;

	struct Input {
		float3 localPos;
	};

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.localPos = v.vertex;
	}

	half _Alpha;
	half _StartHeight;
	half _EndHeight;
	half _StartHue;
	half _EndHue;

	// adapted from: http://stackoverflow.com/a/17897228/4090817
	fixed3 hue2rgb(half hue) {
		fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		fixed3 h = fixed3(hue, hue, hue);
		fixed3 p = abs(frac(h + K.xyz) * 6.0 - K.www);
		return clamp(p - K.xxx, 0.0, 1.0);
	}

	void surf(Input IN, inout SurfaceOutputStandard o) {
		float height = 1 - (IN.localPos.y - _StartHeight) / (_EndHeight - _StartHeight);
		float hueRange = _StartHue - _EndHue;

		o.Albedo = hue2rgb(height * hueRange);
		o.Alpha = _Alpha;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
