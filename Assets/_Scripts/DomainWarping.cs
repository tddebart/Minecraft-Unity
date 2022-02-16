using UnityEngine;

public class DomainWarping : MonoBehaviour
{
    public NoiseSettings noiseDomainX, noiseDomainY;
    public Vector2Int amplitude = new Vector2Int(10, 10);

    public float GenerateDomainNoise(int x, int z, NoiseSettings defaultNoiseSettings)
    {
        var domainOffset = GenerateDomainOffset(x, z);
        return MyNoise.OctavePerlin(x+ domainOffset.x, z+ domainOffset.y, defaultNoiseSettings);
    }

    public Vector2 GenerateDomainOffset(int x, int z)
    {
        var noiseX = MyNoise.OctavePerlin(x, z, noiseDomainX) * amplitude.x;
        var noiseY = MyNoise.OctavePerlin(x, z, noiseDomainY) * amplitude.y;
        return new Vector2(noiseX, noiseY);
    }
    
    public Vector2Int GenerateDomainOffsetInt(int x, int z)
    {
        return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
    }
}