Shader "Custom/ObsidianShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.05, 0.0, 0.08, 1.0)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _BumpStrength("Bump Strength", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 normalOS : NORMAL;
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _BumpStrength;
            CBUFFER_END

            // the displacement map so i can get a height value for a given uv
            float displacementHeight(float u, float v)
            {
                float bump1 = sin(u * 10.0) * sin(v * 10.0);
                float bump2 = sin(u * 23.0 + 1.7) * sin(v * 17.0 + 0.9);
                float bump3 = sin(u * 5.0) * cos(v * 8.0 + 2.1);
                return (bump1 + bump2 + bump3);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                float e = 0.01;
                float h  = displacementHeight(IN.uv.x, IN.uv.y);
                float hX = displacementHeight(IN.uv.x + e, IN.uv.y);
                float hY = displacementHeight(IN.uv.x, IN.uv.y + e);

                float dX = (hX - h) / e;
                float dY = (hY - h) / e;

                float3 bumpNormal = normalize(float3(-dX * _BumpStrength, -dY * _BumpStrength, 1.0));

                normal = normalize(normal + float3(bumpNormal.x, 0, bumpNormal.y) * _BumpStrength);

                float3 ambient = float3(0.1, 0.1, 0.1);

                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                float diffuse = saturate(dot(normal, lightDir));

                // specular with halfway vector
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 halfDir = normalize(lightDir + viewDir);
                float specular = pow(saturate(dot(normal, halfDir)), 64.0);

                float3 diffuseColor = _BaseColor.rgb * (ambient + diffuse);
                float3 specularColor = float3(1.0, 1.0, 1.0) * specular * 0.5;

                return half4(diffuseColor + specularColor, 1.0);
            }
            ENDHLSL
        }
    }
}