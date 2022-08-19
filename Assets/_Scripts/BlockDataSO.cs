using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Block Data", menuName = "Data/Block Data")]
public class BlockDataSO : ScriptableObject
{
    public float textureSizeX , textureSizeY;
    public List<BlockTypeData> textureDataList;
}

[Serializable]
public class BlockTypeData
{
    public BlockType blockType;
    public TextureData textureData;
    public bool isTransparent = false;
    public bool generateCollider = true;
    [Range(0,15)]
    public byte opacity = 15;
    [Range(0,15)]
    public byte lightEmission = 0;
}

[Serializable]
public class TextureData
{
    public Vector2Int up, down, side;
    [Space]
    public Vector2 upExtends = Vector2.one;
    public Vector2 downExtends = Vector2.one;
    public Vector2 sideExtends = Vector2.one;
}