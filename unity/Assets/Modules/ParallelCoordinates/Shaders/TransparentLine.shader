Shader "ParallelCoordinates/TransparentLine"
{
    Properties
    {
    }
    SubShader
    {
        // Tags { "RenderType"="Transparent" "Queue" = "Geometry+3000" "IgnoreProjector"="True"} 
        // Tags { "RenderType"="Transparent" "Queue" = "Geometry" "IgnoreProjector"="True"} 
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        ZTest Always
        ZWrite On
		// Lighting Off
        //Cull Off

        Pass {
            ZWrite On
            ColorMask 0
        }

        CGPROGRAM
        #pragma surface surf Unlit noforwardadd alpha
        #pragma target 3.0
       
        struct Input
        {
            float4 color: Color;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = IN.color.rgb; // * IN.color.a;
            o.Alpha = IN.color.a;
        }

        fixed4 LightingUnlit(SurfaceOutput so, fixed3 lightDir, fixed att)
        {
            fixed4 col;
            col.rgb = so.Albedo;
            col.a = so.Alpha;
            return col;
        }
 
        ENDCG
    }
 }

