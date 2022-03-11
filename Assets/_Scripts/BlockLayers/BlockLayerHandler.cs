using UnityEngine;

public abstract class BlockLayerHandler : MonoBehaviour
{
    [SerializeField] private BlockLayerHandler Next;
    [SerializeField] private bool IsLast;

    public bool Handle(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector3Int mapSeedOffset)
    {
        if(TryHandling(chunk, worldPos,localPos, surfaceHeightNoise, mapSeedOffset))
        {
            return true;
        }
        else if(!IsLast)
        {
            return Next.Handle(chunk, worldPos,localPos, surfaceHeightNoise, mapSeedOffset);
        }
        else
        {
            return false;
        }
    }
    
    protected abstract bool TryHandling(ChunkData chunk, Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector3Int mapSeedOffset);
}