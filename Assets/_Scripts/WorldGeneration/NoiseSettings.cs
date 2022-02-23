using UnityEngine;


[CreateAssetMenu(fileName = "Noise Settings", menuName = "Noise/Noise Settings")]
public class NoiseSettings : ScriptableObject
{
    public float noiseZoom;
    public int octaves = 4;
    public Vector2Int offset;
    public Vector2Int worldOffset;
    public float persistence;
    public float redistributionModifier;
    public float exponent;
}
