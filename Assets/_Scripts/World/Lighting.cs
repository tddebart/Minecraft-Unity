
using UnityEngine;

public class Lighting
{
    public static void RecastSunLightFirstTime(ChunkData chunkData)
    {
        for (var x = 0; x < chunkData.chunkSize; x++)
        {
            for (var z = 0; z < chunkData.chunkSize; z++)
            {
                RecastSunLight(chunkData, new Vector3Int(x,chunkData.worldRef.worldHeight,z));
            }
        }
    }
    
    public static void RecastSunLight(ChunkData chunkData, Vector3Int startPos)
    {
        return;
        bool obstructed = false;

        // Loop from top to bottom of chunk.
        for (int y = startPos.y; y > -1; y--) {
            var block = chunkData.GetBlock(new Vector3Int(startPos.x, y, startPos.z));

            // If light has been obstructed, all blocks below that point are set to 0.
            if (obstructed) {
                block.SetSkyLight(0);
                // Else if block has opacity, set light to 0 and obstructed to true.
            } else if (block.BlockData.opacity > 0) {
                block.SetSkyLight(0);
                obstructed = true;
                // Else set light to 15.
            } else {
                block.SetSkyLight(15);
            }
        }
    }
}
