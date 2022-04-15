using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldNetworkManager : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.singleton != null)
        {
            DestroyImmediate(this);
        }
    }

    private void Start()
    {
        SteamClient.Init(480);
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
