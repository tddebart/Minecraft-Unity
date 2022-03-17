using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public GameObject playerPrefab;
    public GameObject localPlayer;
    public Vector3Int playerChunkPosition;
    private Vector3Int currentChunkCenter = Vector3Int.zero;

    public World world;

    public float detectionTime = 1;

    public void SpawnPlayer()
    {
        if (localPlayer != null) return;

        for (var i = world.worldHeight - 1; i >= 0; i--)
        {
            if (world.GetBlock(new Vector3Int(0, i, 0)).type != BlockType.Air)
            {
                localPlayer = Instantiate(playerPrefab, new Vector3(world.chunkSize/2, i + 1, world.chunkSize/2), Quaternion.identity);
                StartCheckingForChunks();
                break;
            }
        }
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

    private IEnumerator CheckForChunkLoading()
    {
        yield return new WaitForSeconds(detectionTime);
        if (
            Mathf.Abs(currentChunkCenter.x - localPlayer.transform.position.x) > world.chunkSize / 2 ||
            Mathf.Abs(currentChunkCenter.z - localPlayer.transform.position.z) > world.chunkSize / 2 ||
            Mathf.Abs(playerChunkPosition.y - localPlayer.transform.position.y) > world.chunkHeight / 2
        )
        {
            world.LoadAdditionalChunks(localPlayer);
        }
        else
        {
            StartCoroutine(CheckForChunkLoading());
        }
    }
}