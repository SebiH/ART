Shader "ParallelCoordinates/Line" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Spec Color", Color) = (1,1,1,0)
		_Emission ("Emmisive Color", Color) = (0,0,0,0)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.7
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader {
		ZWrite On
		Alphatest Greater 0
		Tags {Queue=Transparent}
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off
		ColorMask RGB
		Pass {
			Material {
				Shininess [_Shininess]
				Specular [_SpecColor]
				Emission [_Emission]
			}
			ColorMaterial AmbientAndDiffuse
			Lighting Off
			SeparateSpecular Off
			SetTexture [_MainTex] {
				Combine texture * primary, texture * primary
			}
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine previous * constant DOUBLE, previous * constant
			} 
		}
	}

}
