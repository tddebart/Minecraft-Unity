using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Block Data", menuName = "Data/Block Data")]
public class BlockDataSO : ScriptableObject
{
    public float textureSizeX , textureSizeY;
    public List<TextureData> textureDataList;
}

[Serializable]
public class TextureData
{
    public BlockType blockType;
    public Vector2Int up, down, side;
    public bool isTransparent = false;
    public bool generateCollider = true;
    [Range(0,15)]
    public int transparency = 15;
    [FormerlySerializedAs("lightValue")] [Range(0, 15)] 
    public int lightEmission = 0;
}