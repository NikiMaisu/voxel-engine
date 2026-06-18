Shader "Custom/PortalShader"
{
    Properties
    {
        _StarDensity("Star Density", Range(10, 200)) = 100
        _StarBrightness("Star Brightness", Range(0, 2)) = 1.0
        _NebulaScale("Nebula Scale", Range(0.1, 5)) = 1.0
        _ScrollSpeed("Scroll Speed", Range(0, 2)) = 0.5
        _PrimaryColor("Primary Color", Color) = (0.3, 0.0, 0.5, 1.0)
        _SecondaryColor("Secondary Color", Color) = (0.0, 0.0, 0.2, 1.0)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float _StarDensity;
                float _StarBrightness;
                float _NebulaScale;
                float _ScrollSpeed;
                half4 _PrimaryColor;
                half4 _SecondaryColor;
            CBUFFER_END

            // gave 2D coordinate and return a pseudorandom float, so that yk, randomness
            float hash(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            // smooth noise i thought looked neat
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep curve

                return lerp(
                    lerp(hash(i + float2(0,0)), hash(i + float2(1,0)), u.x),
                    lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x),
                    u.y
                );
            }

            // layered noise for complexity, since I had to make it fancy
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p);
                    p *= 2.0;           // zoom in each octave
                    amplitude *= 0.5;   // quieter each octave
                }
                return value;
            }

            // generate a star at a given cell and returns brightness of star at this uv, pain to analyze
            float star(float2 uv, float density)
            {
                float2 cell = floor(uv * density);
                float2 localUV = frac(uv * density);

                // star at random position
                float2 starPos = float2(hash(cell), hash(cell + float2(1.0, 0.0)));
                float dist = length(localUV - starPos);

                // twinkle twinkle my little star
                float twinkle = 0.7 + 0.3 * sin(_Time.y * 3.0 + hash(cell) * 6.28);
                // density control cuz it gave me a headache
                float hasStar = step(0.6, hash(cell + float2(0.0, 1.0)));

                // dot looked neat
                return hasStar * twinkle * smoothstep(0.08, 0.0, dist);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // scroll the whole portal cuz why not
                float2 scrolledUV = uv + float2(0.0, _Time.y * _ScrollSpeed * 0.05);

                //layered noise so cloudy purple regions in bg
                float nebula = fbm(scrolledUV * _NebulaScale * 3.0);
                float nebula2 = fbm(scrolledUV * _NebulaScale * 5.0 + float2(1.7, 3.4));

                // blend purpl black
                float3 bgColor = lerp(_SecondaryColor.rgb, _PrimaryColor.rgb, nebula * 0.7);
                // why not just add another one
                bgColor = lerp(bgColor, float3(0.1, 0.0, 0.3), nebula2 * 0.4);

                // stars!!!
                float stars = 0.0;
                stars += star(scrolledUV, _StarDensity * 0.5);   // large
                stars += star(scrolledUV * 1.3 + 0.5, _StarDensity);  // medium
                stars += star(scrolledUV * 2.1 + 1.3, _StarDensity * 2.0); // tiny

                stars *= _StarBrightness;

                // vortex stuff that was not fun to write
                float2 centered = uv - 0.5;
                float angle = atan2(centered.y, centered.x);
                float radius = length(centered);
                float spiral = sin(angle * 3.0 - _Time.y * 0.5 + radius * 8.0) * 0.5 + 0.5;
                float vortex = spiral * (1.0 - smoothstep(0.0, 0.5, radius)) * 0.3;

                // vortex purple towards center
                float3 vortexColor = float3(0.5, 0.0, 0.8) * vortex;

                // COMBINE everything!!!!!!!!!!
                float3 finalColor = bgColor + float3(stars, stars, stars) + vortexColor;

                // edge fade portal fades to transparent at the very edges but i dont think i ever ended up needing it
                float edgeFade = smoothstep(0.0, 0.05, uv.x) * smoothstep(1.0, 0.95, uv.x)
                               * smoothstep(0.0, 0.05, uv.y) * smoothstep(1.0, 0.95, uv.y);

                return half4(finalColor, edgeFade);
            }
            ENDHLSL
        }
    }
}