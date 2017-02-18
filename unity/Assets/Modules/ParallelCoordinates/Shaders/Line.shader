Shader "ParallelCoordinates/Line" {
    Properties {
    }
    SubShader {
        // Tags { "RenderType"="Transparent" "Queue" = "Geometry+3000" "IgnoreProjector"="True"} 
        // Tags { "RenderType"="Transparent" "Queue" = "Geometry" "IgnoreProjector"="True"} 

        // ZTest Always
		// Lighting Off
        //Cull Off

        // Pass {
        //     ColorMask 0
        // }

        CGPROGRAM
        #pragma surface surf Lambert // alpha
        #pragma target 3.0
       
        struct Input {
            float4 color: Color;
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color.rgb; // * IN.color.a;
            // o.Alpha = IN.color.a;
        }
        ENDCG
    }
 }

