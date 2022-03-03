using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

public static class MyNoise
{
    public static Stopwatch stopWatch = new Stopwatch();
    
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
        stopWatch.Start();
        x *= settings.noiseZoom;
        z *= settings.noiseZoom;
        x += settings.noiseZoom;
        z += settings.noiseZoom;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
        for (int i = 0; i < settings.octaves; i++)
        {
            total += Mathf.PerlinNoise((settings.offset.x + settings.worldSeedOffset.x + x) * frequency, (settings.offset.y + settings.worldSeedOffset.y + z) * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= settings.persistence;
            frequency *= 2;
        }
        
        var result = total / amplitudeSum;
        if (result < 0)
        {
            result = 0;
        }

        stopWatch.Stop();
        return result;
    }
}