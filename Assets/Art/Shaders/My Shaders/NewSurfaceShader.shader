Shader "Example/Diffuse Texture" {
Properties
	{
		_Roughness("Roughness", Range(0.0, 10.0)) = 0.0
	}

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			float _Roughness;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

            struct v2f 
			{
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 pos : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
        
            fixed4 frag (v2f i) : SV_Target
            {
               half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); //Direction of ray from the camera towards the object surface
                half3 reflection = reflect(-worldViewDir, i.worldNormal); // Direction of ray after hitting the surface of object
				/*If Roughness feature is not needed : UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflection) can be used instead.
				It chooses the correct LOD value based on camera distance*/
                half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflection, _Roughness);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR); // This is done becasue the cubemap is stored HDR
                return half4(skyColor, 1.0);
            }
            ENDCG
        }
    } 
  
    
    
    Fallback "Diffuse"
  }
 
  

