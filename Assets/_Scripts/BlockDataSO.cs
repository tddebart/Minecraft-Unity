using System;
using System.Collections.Generic;
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
    public Vector2Int up, down, side;
    public bool isTransparent = false;
    public bool generateCollider = true;
    [Range(0,15)]
    public byte opacity = 15;
    [Range(0,15)]
    public byte lightEmission = 0;
}