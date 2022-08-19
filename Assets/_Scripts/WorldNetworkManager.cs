using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldNetworkManager : MonoBehaviour
{
    private void Start()
    {
        if (NetworkClient.active) return;

        try
        {
            SteamClient.Init(480);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Steam failed to initialize");
        }
        GetComponent<NetworkManager>().StartHost();

        SceneManager.sceneUnloaded += scene =>
        {
            if (scene.name.Contains("World"))
            {
                Destroy(gameObject);
            }
        };
    }
}
