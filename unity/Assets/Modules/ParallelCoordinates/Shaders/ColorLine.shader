Shader "ParallelCoordinates/ColorLine"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        ZWrite On
		Lighting Off
        Cull Off

        Pass {
            ZWrite On
            ColorMask 0
        }

        CGPROGRAM
        #pragma surface surf Unlit noforwardadd
        #pragma target 3.0
       
        struct Input
        {
            float4 color: Color;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = IN.color.rgb;
        }

        fixed4 LightingUnlit(SurfaceOutput so, fixed3 lightDir, fixed att)
        {
            fixed4 col;
            col.rgb = so.Albedo;
            return col;
        }
 
        ENDCG
    }
 }

