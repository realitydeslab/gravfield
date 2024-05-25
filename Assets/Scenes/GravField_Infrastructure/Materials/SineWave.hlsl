#define MAX_LINE_COUNT 5

uniform float waveShakeY[MAX_LINE_COUNT];
uniform float waveSinShift[MAX_LINE_COUNT];
uniform float waveSinFrequncy[MAX_LINE_COUNT];
uniform float waveThickness[MAX_LINE_COUNT];
uniform float waveChaos[MAX_LINE_COUNT];


// 2D Random
float random (float2 st) {
    return frac(sin(dot(st.xy,
                         float2(12.9898,78.233)))
                 * 43758.5453123);
}

// 2D Noise based on Morgan McGuire @morgan3d
// https://www.shadertoy.com/view/4dS3Wd
float noise (float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + float2(1.0, 0.0));
    float c = random(i + float2(0.0, 1.0));
    float d = random(i + float2(1.0, 1.0));

    // Smooth Interpolation

    // Cubic Hermine Curve.  Same as SmoothStep()
    float2 u = f*f*(3.0-2.0*f);
    // u = smoothstep(0.,1.,f);

    // Mix 4 coorners percentages
    return lerp(a, b, u.x) +
            (c - a)* u.y * (1.0 - u.x) +
            (d - b) * u.x * u.y;
}

float remap(float v, float src_min, float src_max, float dst_min, float dst_max, bool need_clamp = true)
{
    if (need_clamp)
        v = clamp(v, src_min, src_max);

    if (src_min == src_max)
        v = 0;
    else
        v = (v - src_min) / (src_max - src_min);

    return v * (dst_max - dst_min) + dst_min;
}

float fadeinout(float v, float min, float max, float t1, float t2)
{
    if(min == max) return v;
    v = clamp(v, min, max);
    float per = (v-min) / (max-min);
    if(per <= t1)per = t1 == 0 ? 1 : per / t1;
    else if(per >= t2) per = t2 == 1 ? 0 : (1-per)/(1-t2);
    else per = 1;
    return per;
}

void CalculateLine_float(float2 uv, float frequency, float strength, float thickness, float noise_factor, out float final_value)
{
    final_value = 0;

    [unroll]
    for(int i=0; i<MAX_LINE_COUNT; i++)
    {
        float shake_y = waveShakeY[i];
        float sin_base = uv.x;

        float sin_y = sin(uv.x * waveSinFrequncy[i] + waveSinShift[i]) * shake_y;
        thickness = waveThickness[i];

        
        // float sin_y = sin(uv.x * frequency) * abs(strength);
        float uv_y = (uv.y-0.5)*2;
        // uv_y += noise_factor * noise(uv);
        sin_y += noise_factor * waveChaos[i];

        float value = 0;

        if(sin_y > 0)
        {
            if(uv_y < sin_y - thickness*abs(sin_y))
            {
                value = remap(uv_y, 0, sin_y - thickness*abs(sin_y), 0, 1, true);
            }
            else if(uv_y < sin_y)
            {
                value = 1;
            }
            else
            {
                value = 0;
            }
        }
        else if(sin_y < 0)
        {
            if(uv_y > sin_y + thickness*abs(sin_y))
            {
                value = remap(uv_y, sin_y + thickness*abs(sin_y),0,  1, 0, true);
            }
            else if(uv_y > sin_y)
            {
                value = 1;
            }
            else
            {
                value = 0;
            }
        }        

        final_value = max(final_value, value);
    }
    
}