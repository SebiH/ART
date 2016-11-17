Shader "Custom/DiscardSeethrough" {
	SubShader{
		// See: http://answers.unity3d.com/questions/316064/can-i-obscure-an-object-using-an-invisible-object.html
		Tags { "Queue" = "Geometry-1" }
		Pass {
			Blend Zero One // keep the image behind it
		}
	}
}
