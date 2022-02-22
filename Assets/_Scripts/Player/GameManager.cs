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
        
        var raycastStartposition = new Vector3Int(world.chunkSize / 2+Random.Range(-3,3), world.chunkHeight, world.chunkSize / 2+Random.Range(-3,3));
        RaycastHit hit;
        if (Physics.Raycast(raycastStartposition, Vector3.down, out hit, world.chunkHeight))
        {
            localPlayer = Instantiate(playerPrefab, hit.point+Vector3Int.up, Quaternion.identity);
            StartCheckingForChunks();
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
