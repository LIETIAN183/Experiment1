Shader "InitialPrefabs/SDF" {
    Properties {
        _MainTex ("Font Texture", 2D) = "white" { }
        _LinearCorrection ("Requires Linear Correction", Float) = 0
    }

    SubShader {

        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Lighting Off Cull Off ZTest Always ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _Color;
            float4 _MainTex_TexelSize;
            int _LinearCorrection;

            v2f vert(appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = _LinearCorrection == 1 ? pow(v.color, 2.2) : v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.texcoord1 = v.texcoord1;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float cutoff = i.texcoord1.x;
                float4 color = i.color;

                // sdf distance from edge (scalar)
                float dist = (cutoff - tex2D(_MainTex, (i.texcoord)).a);
                // sdf distance per pixel (gradient vector)
                float2 ddist = float2(ddx(dist), ddy(dist));
                // distance to edge in pixels (scalar)
                float pixelDist = dist / length(ddist);

                color.a = saturate(0.5 - pixelDist);
                if (i.color.a == 0.0) {
                    color.a *= 0;
                }
                return color;
            }
            ENDCG

        }
    }
}
