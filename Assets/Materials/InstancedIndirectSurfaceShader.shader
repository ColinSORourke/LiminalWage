Shader "Instanced/InstancedIndirectSurfaceShader"
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1.0, 1.0, 0.8, 1.0)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_BumpMap ("Bumpmap", 2D) = "bump" {}
	}

	SubShader {
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model
#pragma surface surf Standard addshadow
#pragma multi_compile_instancing
#pragma instancing_options procedural:setup


      sampler2D _MainTex;
      sampler2D _BumpMap;

		float4x4 _mylocalToWorldMatrix;
		float4x4 _myworldToLocalMatrix;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float4> positionBuffer;
		#endif


		//  void vert(inout appdata_full v, out Input data)
        // {
        //     UNITY_INITIALIZE_OUTPUT(Input, data);

        //     #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

        //     float4 pos = positionBuffer[unity_InstanceID];
		//   v.vertex.xyz += pos.xyz;

        //     #endif
        // }


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

		half _Glossiness;
		half _Metallic;
		float4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{

// #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
// 				//col.gb = (float)(unity_InstanceID % 256) / 255.0f;
// 				// col = colorBuffer[unity_InstanceID];
// #else
// 				//col.gb = float4(0, 0, 1, 1);

// #endif

			// clip (frac((IN.worldPos.y+IN.worldPos.z*0.5) * 5) - 0.5);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
		}

		ENDCG
	}
	FallBack "Diffuse"
}
