Shader "Combat/AmbientBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Background", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _AmbientStrength ("Motion strength", Range(0,1)) = 0.7
        [HideInInspector] _AmbientTime ("Time", Float) = 0
        [HideInInspector] _Chapter ("Chapter", Float) = 0
        [HideInInspector] _CloudOffset ("Cloud offset", Float) = 0
        [HideInInspector] _CameraX ("Camera X", Float) = 0
        [HideInInspector] _WorldHeight ("World height", Float) = 15.36
        [HideInInspector] _TileDirection ("Tile direction", Float) = 1
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex AmbientVertex
            #pragma fragment AmbientFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
            };
            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
                float2 worldXY : TEXCOORD1;
            };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _AmbientStrength, _AmbientTime, _Chapter, _CloudOffset;
                float _CameraX, _WorldHeight, _TileDirection;
            CBUFFER_END
            float4 _MainTex_TexelSize;

            Varyings AmbientVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                output.worldXY = TransformObjectToWorld(input.positionOS).xy;
                return output;
            }

            // The two background tiles alternate their horizontal flip. Mirroring the
            // sample coordinate too keeps the moving sky continuous at their join.
            float Mirror(float x) { return 1.0 - abs(frac(x * 0.5) * 2.0 - 1.0); }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            float Bubbles(float2 position, float time, float density)
            {
                position.y -= time;
                float2 cell = floor(position);
                float seed = Hash(cell);
                float2 center = float2(0.25 + seed * 0.5, 0.25 + Hash(cell + 9.2) * 0.5);
                float2 local = frac(position) - center;
                local.x += sin(time * 3.0 + seed * 25.0) * 0.045;
                float radius = lerp(0.045, 0.095, seed);
                float distance = length(local);
                float ring = (1.0 - smoothstep(radius, radius + 0.018, distance))
                    * smoothstep(radius - 0.026, radius - 0.012, distance);
                return ring * step(seed, density);
            }

            half4 AmbientFragment(Varyings input) : SV_Target
            {
                float time = _AmbientTime;
                float strength = _AmbientStrength;
                float2 uv = input.uv;
                float2 pixel = _MainTex_TexelSize.xy;
                float worldX = input.worldXY.x / max(_WorldHeight, 0.01);
                float cameraX = _CameraX / max(_WorldHeight, 0.01);

                if (_Chapter < 0.5)
                {
                    // Only sky moves with parallax. The horizon and combat lane stay fixed.
                    // The artwork has a straight, uniform horizon at this row.
                    // Move the entire sky strip equally: blending offsets by height
                    // would stretch cloud bases after long camera travel.
                    float sky = step(0.5835, uv.y);
                    float cloudShift = (_CloudOffset - time * 0.0015) * strength;
                    uv.x = Mirror(uv.x + sky * cloudShift * _TileDirection);
                    float sea = smoothstep(0.355, 0.39, uv.y) * (1.0 - smoothstep(0.55, 0.585, uv.y));
                    float wave = sin(worldX * 23.0 - time * 1.3 + uv.y * 85.0);
                    // Quantized displacements keep pixel clusters crisp; no sand distortion.
                    uv.x = Mirror(uv.x + round(wave * 2.0 * strength) * pixel.x * sea * _TileDirection);
                    uv.y += round(sin(worldX * 16.0 + time) * strength) * pixel.y * sea;
                    half4 result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;
                    float2 rippleGrid = float2(worldX * 36.0, uv.y * 140.0);
                    float rippleSeed = Hash(floor(rippleGrid));
                    float ripple = step(0.86, rippleSeed) * (1.0 - step(0.22, frac(rippleGrid.y)))
                        * smoothstep(0.2, 0.95, sin(time * 1.4 + rippleSeed * 35.0));
                    result.rgb += half3(0.06, 0.11, 0.10) * ripple * sea * strength;
                    return result;
                }

                half4 water = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;
                float waterMask = smoothstep(0.30, 0.40, uv.y) * (1.0 - smoothstep(0.94, 1.0, uv.y));
                // Two sparse layers move at different speeds and parallax depths.
                float2 nearPosition = float2(worldX - cameraX * 0.15, uv.y);
                float2 farPosition = float2(worldX - cameraX * 0.60, uv.y);
                if (_Chapter < 1.5)
                {
                    float rays = pow(saturate(sin((worldX - cameraX * 0.7) * 11.0 + uv.y * 3.5 + sin(time * 0.18) * 0.3)), 12.0);
                    water.rgb += half3(0.06, 0.11, 0.10) * rays * smoothstep(0.33, 0.95, uv.y) * strength;
                    float bubbles = Bubbles(nearPosition * 9.0, time * 0.18, 0.20)
                        + Bubbles(farPosition * 15.0, time * 0.11, 0.16) * 0.45;
                    water.rgb = lerp(water.rgb, half3(0.82, 1.0, 0.98), saturate(bubbles) * waterMask * strength * 0.5);
                }
                else
                {
                    float2 drift = farPosition * 30.0 - float2(time * 0.025, time * 0.07);
                    float2 cell = floor(drift);
                    float seed = Hash(cell);
                    float2 local = frac(drift) - 0.5;
                    float mote = (1.0 - step(0.055, max(abs(local.x), abs(local.y)))) * step(seed, 0.075);
                    float pulse = 0.35 + 0.65 * pow(0.5 + 0.5 * sin(time * 0.8 + seed * 50.0), 2.0);
                    half3 glow = lerp(half3(0.15, 0.7, 0.95), half3(0.5, 0.3, 0.95), seed);
                    water.rgb += glow * mote * pulse * waterMask * strength;
                    float bubbles = Bubbles(nearPosition * 11.0, time * 0.10, 0.08);
                    water.rgb += half3(0.10, 0.22, 0.32) * bubbles * waterMask * strength;
                }
                return water;
            }
            ENDHLSL
        }
    }
}
