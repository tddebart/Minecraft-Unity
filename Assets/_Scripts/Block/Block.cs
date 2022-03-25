using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Block
{
    public Vector3Int position;
    public Vector3Int localChunkPosition => position + new Vector3Int(0, section.yOffset, 0);
    public Vector3Int globalWorldPosition => section.dataRef.GetGlobalBlockCoords(localChunkPosition);
    
    public BlockType type;
    public BlockTypeData BlockData => BlockDataManager.blockTypeDataDictionary[(int)type];
    
    [System.NonSerialized] public ChunkSection section;
    public ChunkData chunkData => section.dataRef;

    public Block(BlockType type, Vector3Int position, [CanBeNull] ChunkSection section = null)
    {
        this.type = type;
        this.position = position;
        this.section = section;
    }

    public Block(BlockType type)
    {
        this.type = type;
    }

    public int GetBlockLight()
    {
        return section.GetBlockLight(position);
    }
    
    public int GetSkyLight()
    {
        return section.GetSkylight(position);
    }
    
    public void SetBlockLight(int value)
    {
        section.SetBlockLight(position, value);
    }
    
    public void SetSkyLight(int value)
    {
        section.SetSunlight(position, value);
    }
    
    // This will return the highest value of either the block or the sunlight
    public int GetLight()
    {
        return Mathf.Max(GetSkyLight(), GetBlockLight());
    }

    public virtual void OnBlockPlaced()
    {
        if(!section.dataRef.worldRef.IsWorldCreated) return;
        
        // Debug.Log($"Block {type.ToString()} placed");
        World.Instance.ExecuteAfterFrames(1, () => World.Instance.blockToUpdate.Enqueue(this));
        UpdateNeighbours();
    }
    
    public virtual void OnBlockDestroyed()
    {
        if(section?.dataRef.worldRef.IsWorldCreated != true) return;
        // Debug.Log($"Block {type.ToString()} destroyed");
        UpdateNeighbours();
    }
    
    public void UpdateNeighbours()
    {
        if(section?.dataRef.worldRef.IsWorldCreated != true) return;
        
        foreach (var direction in BlockHelper.directions)
        {
            var pos = position + direction.GetVector();
            pos.y += section.yOffset;
            var neighbour = section.dataRef.GetBlock(pos);
            if (neighbour != null)
            {
                World.Instance.ExecuteAfterFrames(1, () => World.Instance.blockToUpdate.Enqueue(neighbour));
            }
        }
    }

    public virtual void OnBlockUpdate()
    {
        if(section?.dataRef.worldRef.IsWorldCreated != true) return;
        
        // Debug.Log($"Block {type.ToString()} updated");
    }
}
