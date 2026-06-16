Shader "Custom/WaterShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.1, 0.3, 0.5, 0.5)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        
        //sky that looks toooo high res for this
        _SkyboxCube("Skybox Cubemap", Cube) = "" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        // had to add queue so that blocks behind it can load first
        Pass
        {
            // most painful to look at formula ive seen in my life
            Blend SrcAlpha OneMinusSrcAlpha
            
            // so that it doesnt block stuff behind apparently
            ZWrite Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                //at the end it'll either be me or the normals standing
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                //here also cuz ofc
                float3 normalWS : TEXCOORD1;
                
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END
            
            //let there be sky
            TEXTURECUBE(_SkyboxCube);
            SAMPLER(sampler_SkyboxCube);
            
            //i was promised something unique, this is just worse java :c
            float waveHeight(float x, float z)
            {
                //to make it wiggle wiggle, but not like uniformely cuz thats boring
                float wave1 = sin(x * 0.5 + _Time.y);
                float wave2 = sin(z * 0.5 + _Time.y * 0.8);
                
                return (wave1 + wave2)  * 0.3;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // worldpos helped to make it between chunks, so less tearing
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                
                // wiggle wiggle apply
                IN.positionOS.y += waveHeight(worldPos.x, worldPos.z);
                
                float e = 0.01; //arbitrary low number because we love magic numbers
                float3 A = float3(worldPos.x, waveHeight(worldPos.x, worldPos.z), worldPos.z);
                float3 B = float3(worldPos.x + e, waveHeight(worldPos.x + e, worldPos.z), worldPos.z);
                float3 C = float3(worldPos.x, waveHeight(worldPos.x, worldPos.z + e), worldPos.z + e);
                
                // these were wrong order and cost me 45 minutes of debugging unity
                float3 normal = normalize(cross(C - A, B - A));
                OUT.normalWS = TransformObjectToWorldNormal(normal);
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                float3 displacedWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionWS = displacedWorldPos;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                // direction from the camera to this pixel on the water
                float3 viewDir = normalize(IN.positionWS - _WorldSpaceCameraPos);

                // bunce the ray off the surface, samples sky
                float3 reflectDir = reflect(viewDir, normal);
                half4 reflectionColor = SAMPLE_TEXTURECUBE(_SkyboxCube, sampler_SkyboxCube, reflectDir);

                // for refraction just tinted it, looks good enough
                half4 refractionColor = _BaseColor;

                // view angle or headon-ness-ness
                float fresnel = 1.0 - saturate(dot(-viewDir, normal));

                // grazing angle > reflective
                // looking down > refractive
                // shaders > head hurts
                half3 finalColor = lerp(refractionColor.rgb, reflectionColor.rgb, fresnel);
                float alpha = lerp(0.5, 1.0, fresnel);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
