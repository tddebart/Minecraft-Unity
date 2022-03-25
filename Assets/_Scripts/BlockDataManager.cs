using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BlockDataManager : MonoBehaviour
{
    public static float textureOffset = 0;
    public static float tileSizeX, tileSizeY;
    public static BlockTypeData[] blockTypeDataDictionary;
    public BlockDataSO textureData;

    private void Start()
    {
        OnValidate();
    }

    public void OnValidate()
    {
        blockTypeDataDictionary = new BlockTypeData[textureData.textureDataList.Max(t => (int)t.blockType+1)];
        foreach (var item in textureData.textureDataList)
        {
            blockTypeDataDictionary[(int)item.blockType] = item;
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
