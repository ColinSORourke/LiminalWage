Shader "Instanced/InstancedIndirectSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        // _BumpMap ("Bumpmap", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        // Cull Off

        CGPROGRAM
        //addshadow
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        // Instancing support
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;
        // sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // For transforming each instance (Rotation, Scale, Translation)
        float4x4 _mylocalToWorldMatrix;
		float4x4 _myworldToLocalMatrix;

        // For positioning the instance in the grid pattern
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float4> positionBuffer;
		#endif

        /// Note we are using our own instancing, so this doesn't apply.
        // // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // // #pragma instancing_options assumeuniformscaling
        // UNITY_INSTANCING_BUFFER_START(Props)
        //     // put more per-instance properties here
        // UNITY_INSTANCING_BUFFER_END(Props)

        void setup()
		{
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float4 data = positionBuffer[unity_InstanceID];

				float4x4 ObjToWrld = _mylocalToWorldMatrix;
				float4x4 WrldToObj = _myworldToLocalMatrix;
				ObjToWrld._14_24_34_44 += float4(data.xyz, 0);
				WrldToObj._14_24_34 -= float4(data.xyz, 0);

				unity_WorldToObject = WrldToObj;
				unity_ObjectToWorld = ObjToWrld;
            #endif
		}


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
// clip (frac((IN.worldPos.y+IN.worldPos.z*0.5) * 5) - 0.5);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            // o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
