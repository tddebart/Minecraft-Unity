using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public static class LightTextureCreator
{
    public static readonly Vector4[] lightColors = new Vector4[16*16];
    private static float[] lightBrightnessTable = new float[16];
    public static float gamma;
    public static float skyLightMultiplier = 0.75f;
    public static float blockLightMultiplier = 1.5f;

    public static void CreateLightTexture()
    {
        GenerateLightBrightnessTable();
        GenerateLightMapColors();
    }

    public static void GenerateLightMapColors()
    {
        Vector3 vector3f = new Vector3(skyLightMultiplier, skyLightMultiplier, 1f);
        vector3f = WeirdLerp(vector3f, Vector3.one, 0.35f);
        for (var k = 0; k < 16; ++k) {
            for (var l = 0; l < 16; ++l) {
                float n;
                var m = lightBrightnessTable[k] * skyLightMultiplier;
                var o = n = lightBrightnessTable[l] * blockLightMultiplier;
                var p = n * ((n * 0.6f + 0.4f) * 0.6f + 0.4f);
                var q = n * (n * n * 0.6f + 0.4f);
                var vector3f2 = new Vector3(o, p, q);

                Vector3 vector3f3 = vector3f;
                vector3f3 *= m;
                vector3f2 += vector3f3;
                vector3f2 = Vector3.Lerp(vector3f2, new Vector3(0.75f, 0.75f, 0.75f), 0.04f);

                vector3f2.x = Mathf.Clamp(vector3f2.x, 0.0f, 1.0f);
                vector3f2.y = Mathf.Clamp(vector3f2.y, 0.0f, 1.0f);
                vector3f2.z = Mathf.Clamp(vector3f2.z, 0.0f, 1.0f);
                var s = gamma;
                Vector3 vector3f5 = vector3f2;
                vector3f5.x = modifyVector(vector3f5.x);
                vector3f5.y = modifyVector(vector3f5.y);
                vector3f5.z = modifyVector(vector3f5.z);
                vector3f2 = Vector3.LerpUnclamped(vector3f2, vector3f5, s);
                vector3f2 = Vector3.Lerp(vector3f2, new Vector3(0.75f, 0.75f, 0.75f), 0.04f);
                vector3f2.x = Mathf.Clamp(vector3f2.x, 0.0f, 1.0f);
                vector3f2.y = Mathf.Clamp(vector3f2.y, 0.0f, 1.0f);
                vector3f2.z = Mathf.Clamp(vector3f2.z, 0.0f, 1.0f);
                // vector3f2*=255.0f;
                const int t = 255;
                lightColors[k * 16 + l] = new Vector4(vector3f2.x, vector3f2.y, vector3f2.z, t);
                // texture.SetPixel(Mathf.Abs(l-15),k, new Color32(u,v,w,255));
                // this.image.setPixelColor(l, k, 0xFF000000 | w << 16 | v << 8 | u);
            }
        }
    }

    public static Vector3 WeirdLerp(Vector3 v1, Vector3 v2, float delta)
    {
        float f = 1 - delta;
        v1.x = v1.x * f + v2.x * delta;
        v1.y = v1.y * f + v2.y * delta;
        v1.z = v1.z * f + v2.z * delta;
        
        return v1;
    }

    public static float modifyVector(float f)
    {
        float g = 1.0f - f;
        return 1.0f - g * g * g * g;
    }

    public static void GenerateLightBrightnessTable()
    {
        var dimensionAmbient = 0;
        float[] fs = new float[16];
        for (int i = 0; i <= 15; ++i)
        {
            float g = (float)i / 15.0f;
            float h = g / (4.0f - 3.0f * g);
            fs[i] = Mathf.Lerp(h, 1.0f, dimensionAmbient);
        }
        lightBrightnessTable = fs;
    }
}
