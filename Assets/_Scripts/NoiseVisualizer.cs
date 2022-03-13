using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NoiseVisualizer : MonoBehaviour
{
    public NoiseSettings settings;
    
    public int resolution = 256;
    
    public Renderer _renderer;

    public void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnValidate()
    {
        // UpdateTexture();
    }

    public void UpdateTexture()
    {
        Color[] pixs = new Color[resolution * resolution];
        for (var x = 0; x < resolution; x++)
        {
            for (var y = 0; y < resolution; y++)
            {
                var color = Color.Lerp(Color.black, Color.white, MyNoise.Redistribution(MyNoise.OctavePerlin(x, y, settings), settings));
                pixs[y * resolution + x] = color;
            }
        }
        
        var texture = new Texture2D(resolution, resolution);
        texture.SetPixels(pixs);
        texture.Apply();
        
        _renderer.sharedMaterial.mainTexture = texture;
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(NoiseVisualizer))]
    public class NoiseVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var biomeGenerator = (NoiseVisualizer)this.target;
            DrawDefaultInspector();
            if (GUILayout.Button("Update Texture"))
            {
                biomeGenerator.UpdateTexture();
            }
            var customEditor = Editor.CreateEditor(biomeGenerator.settings);
            customEditor.OnInspectorGUI();
        }
    }
    
#endif
}
