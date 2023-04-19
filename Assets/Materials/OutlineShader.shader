Shader "Unlit/WireframeShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _WireframeFrontColour("Wireframe front colour", color) = (1.0, 1.0, 1.0, 1.0)
        _WireframeBackColour("Wireframe back colour", color) = (0.5, 0.5, 0.5, 1.0)
        _WireframeWidth("Wireframe width threshold", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

//        TODO: This is supposed to work for behind parts but somehow two passes doesnt work
//        SOLUTION: create second shader, second material and apply it also to the object :)
//        Pass
//        {
//            // Removes the front facing triangles, this enables us to create the wireframe for those behind.
//            Cull Front
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma geometry geom
//            // make fog work
//            #pragma multi_compile_fog
//
//            #include "UnityCG.cginc"
//
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//            };
//
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                UNITY_FOG_COORDS(1)
//                float4 vertex : SV_POSITION;
//            };
//
//            // We add our barycentric variables to the geometry struct.
//            struct g2f {
//                float4 pos : SV_POSITION;
//                float3 barycentric : TEXCOORD0;
//            };
//
//            sampler2D _MainTex;
//            float4 _MainTex_ST;
//
//            v2f vert(appdata v)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                UNITY_TRANSFER_FOG(o,o.vertex);
//                return o;
//            }
//
//            // This applies the barycentric coordinates to each vertex in a triangle.
//            [maxvertexcount(3)]
//            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream) {
//                g2f o;
//                o.pos = IN[0].vertex;
//                o.barycentric = float3(1.0, 0.0, 0.0);
//                triStream.Append(o);
//                o.pos = IN[1].vertex;
//                o.barycentric = float3(0.0, 1.0, 0.0);
//                triStream.Append(o);
//                o.pos = IN[2].vertex;
//                o.barycentric = float3(0.0, 0.0, 1.0);
//                triStream.Append(o);
//            }
//
//            fixed4 _WireframeBackColour;
//            float _WireframeWidth;
//
//            fixed4 frag(g2f i) : SV_Target
//            {
//                // Find the barycentric coordinate closest to the edge.
//                float closest = min(i.barycentric.x, min(i.barycentric.y, i.barycentric.z));
//                // Set alpha to 1 if within the threshold, else 0.
//                float alpha = step(closest, _WireframeWidth);
//                // Set to our backwards facing wireframe colour.
//                return fixed4(_WireframeBackColour.r, _WireframeBackColour.g, _WireframeBackColour.b, alpha);
//            }
//            ENDCG
//        }

        Pass
        {
            // Removes the back facing triangles.
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // We add our barycentric variables to the geometry struct.
            struct g2f {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // This applies the barycentric coordinates to each vertex in a triangle.
            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream) {
                g2f o;
                o.pos = IN[0].vertex;
                o.barycentric = float3(1.0, 0.0, 0.0);
                triStream.Append(o);
                o.pos = IN[1].vertex;
                o.barycentric = float3(0.0, 1.0, 0.0);
                triStream.Append(o);
                o.pos = IN[2].vertex;
                o.barycentric = float3(0.0, 0.0, 1.0);
                triStream.Append(o);
            }

            fixed4 _WireframeFrontColour;
            float _WireframeWidth;

            fixed4 frag(g2f i) : SV_Target
            {
                // Find the barycentric coordinate closest to the edge.
                float closest = min(i.barycentric.x, min(i.barycentric.y, i.barycentric.z));
                // Set alpha to 1 if within the threshold, else 0.
                float alpha = step(closest, _WireframeWidth);
                // Set to our forwards facing wireframe colour.
                return fixed4(_WireframeFrontColour.r, _WireframeFrontColour.g, _WireframeFrontColour.b, alpha);
            }
            ENDCG
        }
    }
}