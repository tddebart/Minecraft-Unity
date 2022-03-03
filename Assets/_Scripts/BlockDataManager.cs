using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BlockDataManager : MonoBehaviour
{
    public static float textureOffset = 0;
    public static float tileSizeX, tileSizeY;
    public static readonly Dictionary<BlockType, TextureData> textureDataDictionary = new Dictionary<BlockType, TextureData>();
    public BlockDataSO textureData;

    private void Start()
    {
        OnValidate();
    }

    public void OnValidate()
    {
        foreach (var item in textureData.textureDataList)
        {
            if (!textureDataDictionary.ContainsKey(item.blockType))
            {
                textureDataDictionary.Add(item.blockType, item);
            }
        }
        tileSizeX = textureData.textureSizeX;
        tileSizeY = textureData.textureSizeY;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BlockDataManager))]
public class BlockDataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Validate"))
        {
            ((BlockDataManager) target).OnValidate();
        }
    }
}
#endif
