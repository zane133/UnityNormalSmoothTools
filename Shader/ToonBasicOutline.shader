Shader "Toon/ShSc_Basic_Outline" 
{
	Properties 
	{
		_Outline("Thick of Outline", range(0, 0.1)) = 0.03
        _Scale("Scale",range(0,1))=0.5        // how far of extrude
        _OutlineColor("OutlineColor", color) = (0, 0, 0, 0)
        _MaxOutLineZOffset("MaxOutLineZOffset",range(0,1))=0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		Cull Front
        ZWrite On
        ColorMask RGB
        Blend SrcAlpha OneMinusSrcAlpha
		
		Pass 
		{
			Name "OUTLINE"
			
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
            CBUFFER_START(UnityPerMaterial)
            float _Outline;
            float _Scale;
            float4 _OutlineColor;
            float _MaxOutLineZOffset;
            CBUFFER_END
			
            struct Attributes 
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 smoothNormalOS : TEXCOORD3;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
        
            struct Varyings 
            {
                float4 positionCS : SV_POSITION;
                half fogCoord : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            Varyings vert(Attributes input) 
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 viewPos = mul(UNITY_MATRIX_MV, input.positionOS);
                viewPos = viewPos / viewPos.w;

            	// 取法线
            	// float3 normalOS = input.smoothNormalOS * 2 - 1;
            	float3 normalOS = input.smoothNormalOS;
				float3 binormal = cross(input.normal, input.tangent) * input.tangent.w;
				float3x3 TtoO = float3x3(
				                input.tangent.xyz,
				                binormal.xyz,
				                input.normal.xyz);
				TtoO = transpose(TtoO);
				normalOS = mul(TtoO, normalOS);			
				// 这里最好单位化一下，不然绑定骨骼的时候会出现奇怪的法线偏移
				normalOS = normalize(normalOS);
            	// normalOS = input.normal;

                float3 N = mul((float3x3)UNITY_MATRIX_IT_MV, normalOS);
                N.z = 0.0099999998;
                N = normalize(N);

                float S = -viewPos.z / unity_CameraProjection[1].y;
                S = pow((S / _Scale), 0.5);
                float tmp12 = _Outline * _Scale * input.color.w * S;
                viewPos.xyz = viewPos.xyz + normalize(viewPos.xyz) * _MaxOutLineZOffset * _Scale * (input.color.z - 0.5);  
                viewPos.xy = viewPos.xy + N.xy * tmp12;

                
            	// viewPos.xyz += N * _Outline;
                // VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                // output.positionCS = vertexInput.positionCS;

                output.positionCS = mul(UNITY_MATRIX_P, viewPos);

                
                output.color = _OutlineColor;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
            	// output.color.xyz = abs(input.normal);
            	// output.color.xyz = abs(normalOS);
                return output;
            }
			
			half4 frag(Varyings i) : SV_Target
			{
				i.color.rgb = MixFog(i.color.rgb, i.fogCoord);
				return i.color;
			}
            ENDHLSL
		}
	}
}
