Shader "Unlit/AltCamScreenShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Lighting Off
		Cull Back
		ZWrite Off
		ZTest Less
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 worldPosition : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                // float4 vertex : SV_Depth;/
            };

            sampler2D _MainTex;

            // Here we've defined a new variable we can put
            // our second camera's projection info into.
            float4x4 _CameraMatrix;

            // Here we've defined a new variable we can put
            // our second camera's projection info into.
            float3 _CamCorrectionOffset = float3(0,0,0);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, out float outDepth : SV_Depth) : SV_Target
            {
                // Map world position into camera's projected coordinates.
                float4 projected = mul(_CameraMatrix, float4(i.worldPosition.xyz + _CamCorrectionOffset, 1.0f));

                // Perform perspective divide and adjust range
                // from NDC's -1...1 to UV space's 0...1
                float2 uv = (projected.xy / projected.w) * 0.5f + 0.5f;


                // Sample the render texture with this UV to prove it works.
                fixed4 col = tex2D(_MainTex, uv);
                // col = float4(uv.xy,0,1);
                if(uv.x > 1.0f || uv.y > 1.0f) {
                    // col = float4(0.9,0,0.9,0.0);
                    clip(-1);
                }else if(uv.x < 0.0f || uv.y < 0.0f) {
                    // col = float4(0,0.9,0.9,0.0);
                    clip(-1);
                }
                // outDepth = 0.5;
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // // Z pass
        // Pass {
        // ZWrite On
        // ColorMask 0
        // vert() {
        // o.pos = UnityObjectToClipPos(v.vertex);
        // .....
        // }

        // frag(o, out float outDepth : SV_Depth) {
        // // I test several depth value
        // // n is near plane, f is far plane, retrived from unity variable _ProjectionParams
        // outDepth = o.pos.z; // (1) this works out fine, like the one without modifying depth
        // outDepth = depth read from camera depth texture // (2) this one produce incorrect result,
        // outDepth = o.pos.z / o.pos.w // (3) incorrect result
        // outDepth = (o.pos.z / o.pos.w) * (f - n) / 2 + (f + n) / 2 // (4) incorrect result
        // }
        // }
        // // base forward shading pass
        // Pass {
        // Zwrite Off
        // ZTest LEqual
        // // do the normal shading task
        // }
    }
}


