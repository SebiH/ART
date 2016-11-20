Shader "Custom/Heatmap" {
	Properties{
		_Palette0("Palette0", Color) = (0, 0, 1, 1)
		_Palette1("Palette1", Color) = (0, 1, 0, 1)
		_Palette2("Palette2", Color) = (1, 1, 0, 1)
		_Palette3("Palette3", Color) = (1, 0, 0, 1)
		_MaxHeight("MaxHeight", Float) = 4
		_Alpha("Alpha", Float) = 0.8
	}
		SubShader{
		Tags{ "Queue" = "Geometry" "RenderType" = "Transparent" }
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
//#pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert
#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;

	struct Input {
		float3 localPos;
	};

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.localPos = v.vertex;
	}

	fixed4 _Palette0;
	fixed4 _Palette1;
	fixed4 _Palette2;
	fixed4 _Palette3;
	half _MaxHeight;
	half _Alpha;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		float height = IN.localPos.y;

		fixed x0 = 1 - step(0.01, height);
		fixed x1 = smoothstep(0, 0.01, height) - smoothstep(_MaxHeight / 3 , _MaxHeight / 3 + 0.01, height);
		fixed x2 = smoothstep(_MaxHeight / 3, _MaxHeight / 3 + 0.01, height) - smoothstep(2 * _MaxHeight / 3 , 2 * _MaxHeight / 3 + 0.01, height);
		fixed x3 = smoothstep(2 * _MaxHeight / 3, 2 *  _MaxHeight / 3 + 0.01, height);

		half4 pal0 = x0 * _Palette0;
		half4 pal1 = x1 * lerp(_Palette0, _Palette1, height / (_MaxHeight / 3));
		half4 pal2 = x2 * lerp(_Palette1, _Palette2, (height - _MaxHeight / 3) / (_MaxHeight / 3));
		half4 pal3 = x3 * lerp(_Palette2, _Palette3, (height - (2 * _MaxHeight / 3)) / (_MaxHeight / 3));

		o.Albedo = pal0 + pal1 + pal2 + pal3;
		o.Alpha = _Alpha;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
