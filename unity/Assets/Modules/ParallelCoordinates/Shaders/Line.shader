Shader "ParallelCoordinates/Line" {
    Properties {
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent" }

        Cull Off
        ZWrite On

        CGPROGRAM
        #pragma surface surf Lambert alpha
        #pragma target 3.0
       
        struct Input {
            float4 color: Color;
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color.rgb;
            o.Alpha = IN.color.a;
        }
        ENDCG
    }
 }

