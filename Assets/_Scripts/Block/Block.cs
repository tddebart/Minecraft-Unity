using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Block
{
    public Vector3Int position;
    public Vector3Int localChunkPosition => position + new Vector3Int(0, section.yOffset, 0);
    public Vector3Int globalWorldPosition => section.dataRef.GetGlobalBlockCoords(localChunkPosition);
    
    public BlockType type { get; private set; }
    public BlockTypeData BlockData => BlockDataManager.blockTypeDataDictionary[(int)type];
    
    [System.NonSerialized] public ChunkSection section;
    public ChunkData chunkData => section.dataRef;

    public Block(BlockType type, Vector3Int position, ChunkSection section)
    {
        this.type = type;
        this.position = position;
        this.section = section;
    }

    public Block(Block block)
    {
        this.type = block.type;
        this.position = block.position;
        this.section = block.section;
    }

    public int GetBlockLight()
    {
        return section.GetBlockLight(position);
    }
    
    public int GetSkyLight()
    {
        if (type == BlockType.Nothing) return 0;
        
        return section.GetSkylight(position);
    }
    
    public void SetBlockLight(int value)
    {
        section.SetBlockLight(position, value);
    }
    
    public void SetSkyLight(int value)
    {
        if (type == BlockType.Nothing || value == GetSkyLight()) return;

        section.SetSunlight(position, value);
    }
    
    // This will return the highest value of either the block or the sunlight
    public int GetLight()
    {
        if (type == BlockType.Nothing) return 0;
        
        return Mathf.Max(GetSkyLight(), GetBlockLight());
    }
    
    public void SetType(BlockType type)
    {
        var x = position.x;
        var y = position.y;
        var z = position.z;
        
        var oldOpacity = BlockData.opacity;

        section.blocks[x, y, z] = BlockMapping.MapTypeToBlock(type, this);
        this.type = type;
        
        // If the block opacity has changed and the block above is in direct sunlight (or top of world)
        // if(BlockData.opacity != oldOpacity && (localChunkPosition.y == World.Instance.worldHeight-1 || chunkData.GetBlock(localChunkPosition + Vector3Int.up).GetSkyLight() == 15))
        // {
        //     Lighting.RecastSunLight(chunkData, new Vector3Int(x, localChunkPosition.y+1, z));
        // }

        if (BlockData.lightEmission > 0)
        {
            SetBlockLight(BlockData.lightEmission);
        }
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
