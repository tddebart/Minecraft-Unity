using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Noise Settings", menuName = "Noise/Noise Settings")]
public class NoiseSettings : ScriptableObject
{
    public float noiseZoom = 0.01f;
    public int octaves = 4;
    public Vector3Int offset;
    [FormerlySerializedAs("worldOffset")] public Vector3Int worldSeedOffset;
    [Range(0,1)]
    public float persistence;
    [Space]
    public float redistributionModifier;
    public float exponent;
}
