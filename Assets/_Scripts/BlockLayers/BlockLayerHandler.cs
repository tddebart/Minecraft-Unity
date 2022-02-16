using UnityEngine;

public abstract class BlockLayerHandler : MonoBehaviour
{
    [SerializeField] private BlockLayerHandler Next;

    public bool Handle(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if(TryHandling(chunk, pos, surfaceHeightNoise, mapSeedOffset))
        {
            return true;
        }
        else if(Next != null)
        {
            return Next.Handle(chunk, pos, surfaceHeightNoise, mapSeedOffset);
        }
        else
        {
            return false;
        }
    }
    
    protected abstract bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset);
}