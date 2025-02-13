// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "Flora"
{
    Properties
    {
        m_mainTexture("Texture", 2D) = "white" {}

        //Shadow Properties
        m_shadowIntensity("Light", Range(0.0, 1.5)) = 0.33

        //Wind Properties
        m_windStrength("Wind Strength", Range(0.0, 1)) = 0.1
        m_windDirectionX("Wind Direction X", Range(0.0, 1)) = 1
        m_windDirectionY("Wind Direction Y", Range(0.0, 1)) = 1
        m_windDirectionZ("Wind Direction Z", Range(0.0, 1)) = 1
        m_windSpeed("Wind Speed", Range(0.0, 10)) = 1.0
        m_windTurbulence("Wind Turbulence", Range(0.0, 1)) = 0.5
    }

    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        LOD 100
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
    
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(MyProperties)
            UNITY_INSTANCING_BUFFER_END(MyProperties)


            //Texture Variables
            sampler2D m_mainTexture;
            float4 m_mainTexture_ST;

            //Shadow Variables
            half m_shadowIntensity;

            //Wind Variables
            half m_windStrength;
            half m_windSpeed;
            half m_windDirectionX;
            half m_windDirectionY;
            half m_windDirectionZ;
            half m_windTurbulence;


            v2f vert (appdata a_vertexData)
            {
                UNITY_SETUP_INSTANCE_ID(a_vertexData);
                
                half3 t_worldPosition = mul(unity_ObjectToWorld, a_vertexData.vertex).xyz;
                half t_yUVSqaured = pow(a_vertexData.uv.y, 2);
       
                //Wind Sway
                half t_localisedRandomX = t_worldPosition.x * ((_CosTime.w + _SinTime.z - _CosTime.x) * 0.33) * m_windDirectionX;
                half t_localisedRandomZ = t_worldPosition.z * ((_SinTime.w + _SinTime.y + _CosTime.y) * 0.33) * m_windDirectionZ;

                half t_windFrequency = (m_windSpeed * _Time.y) + ((t_localisedRandomX + t_localisedRandomZ) * 0.5) * m_windTurbulence;
                half t_wind = m_windStrength * ((sin(t_windFrequency) - cos(t_windFrequency * 0.5)) * 0.5);

                //Adding Wind
                t_worldPosition += t_wind * half3(m_windDirectionX, _SinTime.x * m_windDirectionY, m_windDirectionZ) * t_yUVSqaured;

                //Output
                v2f t_output;
                t_output.vertex = mul(UNITY_MATRIX_VP, half4(t_worldPosition, 1.0));
                t_output.uv = TRANSFORM_TEX(a_vertexData.uv, m_mainTexture);
    
                UNITY_TRANSFER_FOG(t_output, t_output.vertex);
                UNITY_TRANSFER_INSTANCE_ID(a_vertexData, t_output);
                return t_output;
            }

            fixed4 frag (v2f a_input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(a_input);

                fixed4 t_output = tex2D(m_mainTexture, a_input.uv);
                half t_darkenAmount = a_input.uv.y * m_shadowIntensity;
                t_output.xyz *= t_darkenAmount;
    
                //fog
                UNITY_APPLY_FOG(a_input.fogCoord, t_output);

                return t_output;
            }
            ENDCG
        }
    }
}
