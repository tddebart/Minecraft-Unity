using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

public static class MyNoise
{
    public static Stopwatch noiseStopwatch = new Stopwatch();
    public static Stopwatch noise3DStopwatch = new Stopwatch();
    
    public static float RemapValue(float value, float initialMin, float initialMax, float outputMin, float outputMax)
    {
        return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
    }

    public static float Redistribution(float noise, NoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.redistributionModifier, settings.exponent);
    }
    
    public static float OctavePerlin(float x, float z, NoiseSettings settings)
    {
        noiseStopwatch.Start();
        x *= settings.noiseZoom;
        z *= settings.noiseZoom;
        x += settings.noiseZoom;
        z += settings.noiseZoom;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0 range
        for (int i = 0; i < settings.octaves; i++)
        {
            total += Mathf.PerlinNoise((settings.offset.x + settings.worldSeedOffset.x + x) * frequency, (settings.offset.y + settings.worldSeedOffset.y + z) * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= settings.persistence;
            frequency *= 2;
        }
        
        var result = total / maxValue;
        if (result < 0)
        {
            result = 0;
        } else if (result > 1)
        {
            result = 1;
        }

        noiseStopwatch.Stop();
        return result;
    }
    
    public static bool OctavePerlin3D(Vector3 pos, NoiseSettings settings, float threshold)
    {
        noise3DStopwatch.Start();
        pos *= settings.noiseZoom;
        var x = settings.offset.x + settings.worldSeedOffset.x + pos.x;
        var y = settings.offset.y + settings.worldSeedOffset.y + pos.y;
        var z = settings.offset.z + settings.worldSeedOffset.z + pos.z;
        
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);
        
        noise3DStopwatch.Stop();
        
        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
            return true;
        else
            return false;
    }
    
}