using UnityEngine;

public class LightTextureVisualizer : MonoBehaviour
{
    public Texture2D lightTexture;
    [Range(0, 1)] public float gamma;
    public float skyLightMultiplier = 0.75f;
    public float blockLightMultiplier = 1.5f;

    private void OnValidate()
    {
        if (lightTexture != null)
        {
            LightTextureCreator.gamma = gamma;
            LightTextureCreator.skyLightMultiplier = skyLightMultiplier;
            LightTextureCreator.blockLightMultiplier = blockLightMultiplier;
            LightTextureCreator.CreateLightTexture();
            var pixels = lightTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = LightTextureCreator.lightColors[i];
            }
            lightTexture.SetPixels(pixels);
            lightTexture.Apply();
        }
        else
        {
            lightTexture = new Texture2D(16, 16);
            lightTexture.filterMode = FilterMode.Point;
            lightTexture.wrapMode = TextureWrapMode.Clamp;
        }
        GetComponent<Renderer>().sharedMaterial.mainTexture = lightTexture;
    }
}