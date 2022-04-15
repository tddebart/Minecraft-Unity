using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject playerPrefab;
    public Player localPlayer;
    public List<Player> players = new List<Player>();
    public Vector3Int playerChunkPosition;
    private Vector3Int currentChunkCenter = Vector3Int.zero;
    public bool playerSpawned = false;

    public World world;

    public float detectionTime = 1;

    private void Awake()
    {
        Instance = this;
    }

    public void StartCheckingForChunks()
    {
        SetCurrentChunkCenter();
        StopAllCoroutines();
        StartCoroutine(CheckForChunkLoading());
    }

    private void SetCurrentChunkCenter()
    {
        playerChunkPosition = WorldDataHelper.GetChunkPosition(world, Vector3Int.RoundToInt(localPlayer.transform.position));
        currentChunkCenter.x = playerChunkPosition.x + world.chunkSize / 2;
        currentChunkCenter.y = playerChunkPosition.y + world.chunkHeight / 2;
    }

    public Vector3Int GetCurrentChunkCenter()
    {
        return WorldDataHelper.GetChunkPosition(world, Vector3Int.RoundToInt(localPlayer.transform.position));
    }

    private IEnumerator CheckForChunkLoading()
    {
        yield return null;
        if (
            Mathf.Abs(currentChunkCenter.x - localPlayer.transform.position.x) > world.chunkSize / 2 ||
            Mathf.Abs(currentChunkCenter.z - localPlayer.transform.position.z) > world.chunkSize / 2 ||
            Mathf.Abs(playerChunkPosition.y - localPlayer.transform.position.y) > world.chunkHeight / 2
        )
        {
            world.LoadAdditionalChunks(localPlayer.gameObject);
        }
        else
        {
            StartCoroutine(CheckForChunkLoading());
        }
    }
}
