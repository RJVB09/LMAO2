float3 pcg3d(int3 v) 
{
    v = v * 1664525u + 1013904223u;

    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;

    v ^= v >> 16u;

    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;

    return v / 2147483648.0;
}

float Perlin(float3 pos, float amp, float freq)
{
    if (amp == 0)
        return 0;
    pos = mad(pos, float3(0.3, 0.3, 0.3), float3(100, 100, 100)) * freq;
    float3 final = 0;
    for (int i = 0; i < 8; i++)
    {
        int3 r = int3(i / 4u, (i / 2u) % 2u, i % 2u);
        int3 s = int3(pos)+r;
        final += pcg3d(s) * saturate(1 - distance(r, frac(pos)));
    }
    return clamp(final.y - final.z + final.x, -2, 2) * 0.5 * amp;
}
            
float2 Perlin2D(float3 pos, float amp, float freq)
{
    if (amp == 0)
        return 0;
    pos = mad(pos, float3(0.3, 0.3, 0.3), float3(100, 100, 100)) * freq;
    float3 final = 0;
    for (int i = 0; i < 8; i++)
    {
        int3 r = int3(i / 4u, (i / 2u) % 2u, i % 2u);
        int3 s = int3(pos)+r;
        final += pcg3d(s) * saturate(1 - distance(r, frac(pos)));
    }
    return final.xy * 0.5 * amp;
}